#!/bin/bash

set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root." >&2
    exit 1
fi

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
RULES_DIR="$SCRIPT_DIR/udev"

install -Dm644 "$SCRIPT_DIR/BTCPayServerVault.desktop" /usr/share/applications/BTCPayServerVault.desktop
install -Dm644 "$SCRIPT_DIR/BTCPayServerVault.png" /usr/share/icons/hicolor/64x64/apps/BTCPayServerVault.png

rm -rf /opt/BTCPayServer.Vault 
mkdir -p /opt/BTCPayServer.Vault
cp -r "$SCRIPT_DIR"/. "/opt/BTCPayServer.Vault/"
chmod +x /opt/BTCPayServer.Vault/BTCPayServer.Vault

echo "/opt/BTCPayServer.Vault created"

ln -s /opt/BTCPayServer.Vault/BTCPayServer.Vault /usr/local/bin/BTCPayServer.Vault

chmod +x /usr/local/bin/BTCPayServer.Vault
echo "/usr/local/bin/BTCPayServer.Vault created"

echo "If the Vault cannot access your hardware wallet, you may need to restart your computer."