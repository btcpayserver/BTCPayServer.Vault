#!/bin/bash

set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root." >&2
    exit 1
fi

if ! getent group plugdev > /dev/null; then
    echo "Creating system group plugdev"
    groupadd -r plugdev
else
    echo "Group plugdev already exists"
fi

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
RULES_DIR="$SCRIPT_DIR/udev"

echo 'Adding udev rules to /usr/local/lib/udev/rules.d'

shopt -s nullglob
for f in "$RULES_DIR"/*; do
    install -Dm644 "$f" "/usr/local/lib/udev/rules.d/$(basename "$f")"
    echo "Installed $(basename "$f")"
done
shopt -u nullglob


udevadm control --reload-rules
udevadm trigger

echo "udevadm triggered and reloaded"

gpasswd -a $SUDO_USER plugdev

echo "User $SUDO_USER added to plugdev"

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