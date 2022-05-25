#!/bin/bash
set -e
[ -x "$(command -v jq)" ] || apt-get intall jq
if ! [[ "$AZURE_STORAGE_CONNECTION_STRING" ]] || ! [[ "$AZURE_STORAGE_CONTAINER" ]]; then
    echo "Skipping github release (AZURE_STORAGE_CONNECTION_STRING or AZURE_STORAGE_CONTAINER not set)"
    exit 0
fi

if ! [[ "$GITHUB_REF" ]]; then
    echo "Skipping github release (GITHUB_REF is not set)"
    exit 0
fi

# GITHUB_REF= refs/tags/Vault/v1.0.6-test
GITHUB_REF_NAME="$(echo $GITHUB_REF | cut -d'/' -f4)"
GITHUB_REF_NAME="Vault/$GITHUB_REF_NAME"
# GITHUB_REF_NAME= Vault/v1.0.6-test

draft=false
prerelease=false

if [[ "$GITHUB_REF_NAME" == *"-test" ]]; then
    draft=true
    prerelease=true
fi

AZURE_ACCOUNT_NAME="$(echo "$AZURE_STORAGE_CONNECTION_STRING" | cut -d'=' -f3 | cut -d';' -f1)"
DIRECTORY_NAME="dist-$GITHUB_RUN_ID"
wget -O azcopy.tar.gz https://aka.ms/downloadazcopy-v10-linux
tar -xf azcopy.tar.gz --strip-components=1
mkdir -p dist
./azcopy cp "https://$AZURE_ACCOUNT_NAME.blob.core.windows.net/$AZURE_STORAGE_CONTAINER/$DIRECTORY_NAME/*" "dist"

release="$(cat Build/RELEASE.md)"
version="$(echo "$GITHUB_REF_NAME" | cut -d'/' -f2)"
payload="$(jq -M --arg "tag_name" "$GITHUB_REF_NAME" \
   --arg "name" "BTCPayServer Vault $version" \
   --arg "body" "$release" \
   --argjson "draft" $draft \
   --argjson "prerelease" $prerelease \
   '. | .tag_name=$tag_name | .name=$name | .body=$body | .draft=$draft | .prerelease=$prerelease' \
   <<<'{}')"
echo "Creating release to https://api.github.com/repos/$GITHUB_REPOSITORY/releases"
echo "$payload"
response="$(curl --fail -s -S -X POST https://api.github.com/repos/$GITHUB_REPOSITORY/releases \
    -H "Accept: application/vnd.github.v3+json" \
    -H "Authorization: token $GITHUB_TOKEN" \
    -H "Content-Type: application/json" \
    -d "$payload")"
release_id="$(echo $response | jq ".id")"

cd dist
for f in *; do
  [[ $f == *.deb ]] && media_type="application/x-debian-package"
  [[ $f == *.dmg ]] && media_type="application/x-apple-diskimage"
  [[ $f == *.exe ]] && media_type="application/x-msdos-program"
  [[ $f == *.tar.gz ]] && media_type="application/x-tar"
  [[ $f == *.asc ]] && media_type="text/plain"
  if ! [[ "$media_type" ]]; then
    echo "Unable to guess media type for file $f"
    exit 1
  fi
  echo "Uploading $f to github release"
  curl --fail -s -S \
    -H "Accept: application/vnd.github.v3+json" \
    -H "Authorization: token $GITHUB_TOKEN" \
    -H "Content-Type: $media_type" \
    --data-binary @"$f" \
    "https://uploads.github.com/repos/$GITHUB_REPOSITORY/releases/$release_id/assets?name=$f"
done
