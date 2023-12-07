#!/bin/bash

DOTNET_RUNTIME=${DOTNET_RUNTIME:-$RUNTIME}
BUILD_ARGS="--runtime $DOTNET_RUNTIME -p:Configuration=Release -p:GithubDistrib=true"
FRAMEWORK="net8.0"
DIST="/source/dist"
RESOURCES="/source/Build/${RUNTIME}"
RESOURCES_COMMON="/source/Build/common"
PROJECT_FILE="/source/BTCPayServer.Vault/BTCPayServer.Vault.csproj"
VERSION_FILE="/source/BTCPayServer.Vault/Version.csproj"
LICENSE="$(cat $PROJECT_FILE | sed -n 's/.*<PackageLicenseExpression>\(.*\)<\/PackageLicenseExpression>.*/\1/p')"
DESCRIPTION="$(cat $PROJECT_FILE | sed -n 's/.*<Description>\(.*\)<\/Description>.*/\1/p')"
COMPANY="$(cat $PROJECT_FILE | sed -n 's/.*<Company>\(.*\)<\/Company>.*/\1/p')"
TITLE="$(cat $PROJECT_FILE | sed -n 's/.*<Title>\(.*\)<\/Title>.*/\1/p')"
if [ -f "$VERSION_FILE" ]; then
    VERSION="$(cat $VERSION_FILE | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')"
fi
PUBLISH_FOLDER="/source/BTCPayServer.Vault/bin/Release/$FRAMEWORK/$DOTNET_RUNTIME/publish"
EXECUTABLE="$(cat $PROJECT_FILE | sed -n 's/.*<TargetName>\(.*\)<\/TargetName>.*/\1/p')"

mkdir -p "$DIST"

replaceProjectVariables () {
  sed -i "s/{VersionPrefix}/$VERSION/g" "$1"
  sed -i "s/{VERSION}/$VERSION/g" "$1"
  sed -i "s/{ApplicationName}/$TITLE/g" "$1"
  sed -i "s/{TITLE}/$TITLE/g" "$1"
  sed -i "s/{DESCRIPTION}/$DESCRIPTION/g" "$1"
  sed -i "s/{EXECUTABLE}/$EXECUTABLE/g" "$1"
  sed -i "s/{COMPANY}/$COMPANY/g" "$1"
}

dotnet_publish () {
    dotnet publish --no-restore --framework $FRAMEWORK $BUILD_ARGS $ADDITIONAL_PUBLISH_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj
}

dotnet_restore () {
    dotnet restore $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj
}
