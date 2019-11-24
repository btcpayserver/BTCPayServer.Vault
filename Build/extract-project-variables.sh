#!/bin/bash

LICENSE="$(cat $1 | sed -n 's/.*<PackageLicenseExpression>\(.*\)<\/PackageLicenseExpression>.*/\1/p')"
DESCRIPTION="$(cat $1 | sed -n 's/.*<Description>\(.*\)<\/Description>.*/\1/p')"
ICON="$(cat $1 | sed -n 's/.*<ApplicationIcon>\(.*\)<\/ApplicationIcon>.*/\1/p')"
COMPANY="$(cat $1 | sed -n 's/.*<Company>\(.*\)<\/Company>.*/\1/p')"
TITLE="$(cat $1 | sed -n 's/.*<Title>\(.*\)<\/Title>.*/\1/p')"
VERSION="$(cat $1 | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')"
PUBLISH_FOLDER="/source/BTCPayServer.Vault/bin/Release/$FRAMEWORK/$RUNTIME/publish"
EXECUTABLE="$(cat $1 | sed -n 's/.*<TargetName>\(.*\)<\/TargetName>.*/\1/p')"


replaceProjectVariables () {
  sed -i "s/{VersionPrefix}/$VERSION/g" "$1"
  sed -i "s/{VERSION}/$VERSION/g" "$1"
  sed -i "s/{ApplicationName}/$TITLE/g" "$1"
  sed -i "s/{TITLE}/$TITLE/g" "$1"
  sed -i "s/{DESCRIPTION}/$DESCRIPTION/g" "$1"
  sed -i "s/{EXECUTABLE}/$EXECUTABLE/g" "$1"
  sed -i "s/{COMPANY}/$COMPANY/g" "$1"
}
