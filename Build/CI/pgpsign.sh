#!/bin/bash
set -e

if ! [[ "$AZURE_STORAGE_CONNECTION_STRING" ]] || ! [[ "$AZURE_STORAGE_CONTAINER" ]]; then
    echo "Skipping SHA256SUMS (AZURE_STORAGE_CONNECTION_STRING or AZURE_STORAGE_CONTAINER not set)"
    exit 0
fi
if ! [[ "$PGP_KEY" ]]; then
    echo "Skipping SHA256SUMS signature (PGP_KEY is not set)"
    exit 0
fi

AZURE_ACCOUNT_NAME="$(echo "$AZURE_STORAGE_CONNECTION_STRING" | cut -d'=' -f3 | cut -d';' -f1)"
DIRECTORY_NAME="dist-$GITHUB_RUN_ID"
wget -O azcopy.tar.gz https://aka.ms/downloadazcopy-v10-linux
tar -xf azcopy.tar.gz --strip-components=1
mkdir -p dist
./azcopy cp "https://$AZURE_ACCOUNT_NAME.blob.core.windows.net/$AZURE_STORAGE_CONTAINER/$DIRECTORY_NAME/*" "dist"

cd dist
for f in *; do
  if [[ "$f" == "SHA256SUMS" ]]; then continue; fi
  sha256sum $f >> /tmp/SHA256SUMS
done
mv /tmp/SHA256SUMS SHA256SUMS
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

echo "$PGP_KEY" | base64 --decode | gpg --import --no-tty
echo "PGP keys correctly imported"
gpg --digest-algo sha256 --clearsign SHA256SUMS
az storage blob upload -f "SHA256SUMS.asc" -c "$AZURE_STORAGE_CONTAINER" -n "$DIRECTORY_NAME/SHA256SUMS.asc"
rm SHA256SUMS
