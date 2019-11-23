#!/bin/bash

LICENSE="$(cat $1 | sed -n 's/.*<PackageLicenseExpression>\(.*\)<\/PackageLicenseExpression>.*/\1/p')"
DESCRIPTION="$(cat $1 | sed -n 's/.*<Description>\(.*\)<\/Description>.*/\1/p')"
ICON="$(cat $1 | sed -n 's/.*<ApplicationIcon>\(.*\)<\/ApplicationIcon>.*/\1/p')"
COMPANY="$(cat $1 | sed -n 's/.*<Company>\(.*\)<\/Company>.*/\1/p')"
TITLE="$(cat $1 | sed -n 's/.*<Title>\(.*\)<\/Title>.*/\1/p')"
VERSION="$(cat $1 | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')"
PUBLISH_FOLDER="/source/BTCPayServer.Vault/bin/Release/$FRAMEWORK/$RUNTIME/publish"
