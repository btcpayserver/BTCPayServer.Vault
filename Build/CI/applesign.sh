#!/usr/bin/env bash

# Apple signing process is really tricky.
# 1. The way we handle it is by: Downloading the dmg file generated in previous travis step
# 2. Extracting the content via cp
# 3. Signing the .App folder inside the copy, with hardened runtime and the entitlements
# 4. Create a new DMG file, compressing it
# 5. Signing the DMG itself
# 6. Upload to Apple notary
# For this to work, you will need to get an "Developer Id Application" certificate and the credentials to notarize the app
# please read applesign.md for properly creating the certificate

export LC_ALL=C.UTF-8
set -e

if ! [[ "$AZURE_STORAGE_CONNECTION_STRING" ]] || ! [[ "$AZURE_STORAGE_CONTAINER" ]]; then
    echo "Skipping SHA256SUMS (AZURE_STORAGE_CONNECTION_STRING or AZURE_STORAGE_CONTAINER not set)"
    exit 0
fi

if ! [[ "$APPLE_DEV_ID_CERT" ]]; then
    echo "Skipping applesign (APPLE_DEV_ID_CERT is not set)"
    exit 0
fi

if ! [[ "$APPLE_ID" ]]; then
    echo "Skipping applesign (APPLE_ID is not set)"
    exit 0
fi

if ! [[ "$APPLE_ID_PASSWORD" ]]; then
    echo "Skipping applesign (APPLE_ID_PASSWORD is not set)"
    exit 0
fi

if ! [[ "$APPLE_DEV_ID_CERT_PASSWORD" ]]; then
    echo "Skipping applesign (APPLE_DEV_ID_CERT_PASSWORD is not set)"
    exit 0
fi

echo "Starting apple signing..."
version="$(cat BTCPayServer.Vault/Version.csproj | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')"
title="$(cat BTCPayServer.Vault/BTCPayServer.Vault.csproj | sed -n 's/.*<Title>\(.*\)<\/Title>.*/\1/p')"
AZURE_ACCOUNT_NAME="$(echo "$AZURE_STORAGE_CONNECTION_STRING" | cut -d'=' -f3 | cut -d';' -f1)"
DIRECTORY_NAME="dist-$GITHUB_RUN_ID"
RUNTIME="osx-x64"
tar_file="BTCPayServerVault-${RUNTIME}-$version.tar.gz"

mkdir -p dist
download_url="https://$AZURE_ACCOUNT_NAME.blob.core.windows.net/$AZURE_STORAGE_CONTAINER/$DIRECTORY_NAME/$tar_file"
echo "Download $download_url to dist/$tar_file"
wget -qO "dist/$tar_file" "$download_url"
echo "Downloaded"
cd dist
if ! (echo "$APPLE_DEV_ID_CERT" | base64 --decode > dev.p12); then
    echo "Could not decode APPLE_DEV_ID_CERT from base64"
    exit 1
fi

dmg_folder="dmg_root"
app_path="$dmg_folder/$title.app"
mkdir -p "$dmg_folder"
tar -C "$dmg_folder" -xvf "$tar_file"
bundle_id="$(cat "$app_path/Contents/Info.plist" | grep -A1 "CFBundleIdentifier" | sed -n 's/\s*<string>\([^<]*\)<\/string>/\1/p' | xargs)"
rm "$tar_file"

key_chain="build.keychain"
key_chain_pass="mysecretpassword"
security create-keychain -p "$key_chain_pass" "$key_chain"
security default-keychain -s "$key_chain"
security unlock-keychain -p "$key_chain_pass" "$key_chain"
security import dev.p12 -k "$key_chain" -P "$APPLE_DEV_ID_CERT_PASSWORD" -A
CERT_IDENTITY=$(security find-identity -v -p codesigning "$key_chain" | head -1 | grep '"' | sed -e 's/[^"]*"//' -e 's/".*//')
echo "Signing with identity $CERT_IDENTITY"
security set-key-partition-list -S apple-tool:,apple: -s -k "$key_chain_pass" "$key_chain"

# codesign don't like that the entitlements file path have spaces, so we move it to local folder.
sudo cp "$app_path/Contents/entitlements.plist" "./"
echo "Signing $app_path..."
code_sign_args="--deep --force --options runtime --timestamp --entitlements entitlements.plist"
sudo codesign $code_sign_args --sign "$CERT_IDENTITY" "$app_path"

dmg_file="BTCPayServerVault-${RUNTIME}-$version.dmg"
echo "Create $dmg_file with signature"
dmg_file_writable="$dmg_file.writable.dmg"
sudo hdiutil create "$dmg_file_writable" -ov -volname "$title" -fs HFS+ -srcfolder "$dmg_folder" 
sudo hdiutil convert "$dmg_file_writable" -format UDZO -o "$dmg_file"
sudo rm -rf "$dmg_file_writable"
rm -rf "$dmg_folder"

echo "Signing $dmg_file..."
sudo codesign $code_sign_args --sign "$CERT_IDENTITY" "$dmg_file"
echo "DMG signed"

echo "Notarize $dmg_file with bundle id $bundle_id"

sudo xcrun notarytool submit --apple-id "$APPLE_ID" --password "$APPLE_ID_PASSWORD" --team-id "$APPLE_TEAM_ID" --wait "$dmg_file"

sudo xcrun stapler staple "$dmg_file"

echo "Installing az..."
export HOMEBREW_NO_INSTALLED_DEPENDENTS_CHECK=1
brew update
brew install azure-cli || true
BLOB_NAME="$DIRECTORY_NAME/$dmg_file"
echo "Uploading $BLOB_NAME"
sudo az storage blob upload -f "$dmg_file" --connection-string "$AZURE_STORAGE_CONNECTION_STRING" -c "$AZURE_STORAGE_CONTAINER" -n "$BLOB_NAME"
url="$(sudo az storage blob url --connection-string "$AZURE_STORAGE_CONNECTION_STRING" --container-name "$AZURE_STORAGE_CONTAINER" --name "$BLOB_NAME" --protocol "https")"
echo "Uploaded file to $url"

echo "Deleting $DIRECTORY_NAME/$tar_file"
sudo az storage blob delete --connection-string "$AZURE_STORAGE_CONNECTION_STRING" -c "$AZURE_STORAGE_CONTAINER" -n "$DIRECTORY_NAME/$tar_file"
