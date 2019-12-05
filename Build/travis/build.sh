#!/bin/bash

set -e

DOCKER_IMAGE_NAME="vault-$RID"
docker build -t "$DOCKER_IMAGE_NAME" $DOCKER_BUILD_ARGS -f "Build/$RID/Dockerfile" .
docker run --rm -v "$(pwd)/dist:/opt/dist" "$DOCKER_IMAGE_NAME"

if [[ "$TRAVIS_TAG" ]]; then
    travis_version="$(echo "$TRAVIS_TAG" | cut -d'/' -f2)"
    csproj_version="v$(cat BTCPayServer.Vault/Version.csproj | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')"
    if [[ "$travis_version" != "$csproj_version" ]]; then
        echo "The tagged version on travis ($travis_version) is different from the csproj ($csproj_version)"
        exit 1
    fi
fi

if ! [[ "$AZURE_STORAGE_CONNECTION_STRING" ]] || ! [[ "$AZURE_STORAGE_CONTAINER" ]]; then
    echo "Skipped upload to Azure (AZURE_STORAGE_CONNECTION_STRING or AZURE_STORAGE_CONTAINER not set)"
    exit 0
fi
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
az storage container create --name "$AZURE_STORAGE_CONTAINER" --public-access "container"
for file in dist/*; do
    BLOB_NAME="dist-$TRAVIS_BUILD_ID/$(basename -- $file)"
    echo "Uploading $BLOB_NAME"
    az storage blob upload -f "$file" -c "$AZURE_STORAGE_CONTAINER" -n "$BLOB_NAME"
    url="$(az storage blob url --container-name "$AZURE_STORAGE_CONTAINER" --name "$BLOB_NAME" --protocol "https")"
    echo "Uploaded file to $url"
done
