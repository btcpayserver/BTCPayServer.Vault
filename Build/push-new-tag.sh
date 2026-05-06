#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
version_file="$script_dir/../BTCPayServer.Vault/Version.csproj"

ver="$(grep -oPm1 '(?<=<Version>)[^<]+' "$version_file")"
tag="Vault/v$ver"

git tag -a "$tag" -m "$tag"
git push origin "$tag"
