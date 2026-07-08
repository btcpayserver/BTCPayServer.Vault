#!/bin/bash

set -e

DOCKER_IMAGE_NAME="vault-$RID"
DOCKER_BUILD_ARGS=""
DOCKERFILE="Build/$RID/Dockerfile"
if [[ "$PGP_KEY" ]]; then
     DOCKER_BUILD_ARGS="--build-arg "PGP_KEY=$PGP_KEY""
fi
if [[ "$WINDOWS_CERT" ]]; then
     DOCKER_BUILD_ARGS="$DOCKER_BUILD_ARGS --build-arg "WINDOWS_CERT=$WINDOWS_CERT" --build-arg "WINDOWS_CERT_PASSWORD=$WINDOWS_CERT_PASSWORD""
fi
if [[ "$RID" == "osx-arm64" ]]; then
     DOCKERFILE="Build/osx-x64/Dockerfile"
     DOCKER_BUILD_ARGS="$DOCKER_BUILD_ARGS --build-arg RUNTIME=osx-arm64 --build-arg HWI_PACKAGE_ARCH=arm64 --build-arg HWI_SHA256=87a8991848a0216213ddf6497c753cebbda492626afaf5608c30931155c922c3 --build-arg ARCHITECTURE_PRIORITY=arm64"
fi

docker build -t "$DOCKER_IMAGE_NAME" $DOCKER_BUILD_ARGS -f "$DOCKERFILE" .
docker run --rm -v "$(pwd)/dist:/opt/dist" "$DOCKER_IMAGE_NAME"

if [[ "$GITHUB_REF" ]]; then
    # GITHUB_REF= refs/tags/Vault/v1.0.6-test
    GITHUB_REF_NAME="$(echo $GITHUB_REF | cut -d'/' -f4 | cut -d'-' -f1)"
    GITHUB_REF_NAME="Vault/$GITHUB_REF_NAME"
    # GITHUB_REF_NAME= Vault/v1.0.6
    ci_version="$(echo "$GITHUB_REF_NAME" | cut -d'/' -f2)"
    if [[ "$ci_version" ]]; then
        csproj_version="v$(cat BTCPayServer.Vault/Version.csproj | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')"
        if [[ "$ci_version" != "$csproj_version" ]]; then
            echo "The tagged version on travis ($ci_version) is different from the csproj ($csproj_version)"
            exit 1
        fi
    fi
fi

if ! [[ "$AZURE_STORAGE_CONNECTION_STRING" ]] || ! [[ "$AZURE_STORAGE_CONTAINER" ]]; then
    echo "Skipped upload to Azure (AZURE_STORAGE_CONNECTION_STRING or AZURE_STORAGE_CONTAINER not set)"
    exit 0
fi
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
az storage container create --name "$AZURE_STORAGE_CONTAINER" --public-access "container"

for file in dist/*; do
    BLOB_NAME="dist-$GITHUB_RUN_ID/$(basename -- $file)"
    echo "Uploading $BLOB_NAME"
    az storage blob upload -f "$file" -c "$AZURE_STORAGE_CONTAINER" -n "$BLOB_NAME"
    url="$(az storage blob url --container-name "$AZURE_STORAGE_CONTAINER" --name "$BLOB_NAME" --protocol "https")"
    echo "Uploaded file to $url"
done
