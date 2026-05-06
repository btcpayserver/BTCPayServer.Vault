FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

RUN wget -qO /tmp/hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/3.1.0/hwi-3.1.0-linux-x86_64.tar.gz && \
    tar -zxvf /tmp/hwi.tar.gz -C /tmp hwi && \
    echo "4732e06e879b51eb5b42ce334c54373d7901556227dca4a63d342190f04b22c9 /tmp/hwi" | sha256sum -c - && \
    rm /tmp/hwi.tar.gz

RUN apt-get update && \
    apt-get install -y --no-install-recommends imagemagick && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /source
ENV RUNTIME "linux-x64"
COPY "Build/common" "Build/common"
COPY "Build/linux-x64" "Build/linux-x64"
ENV EXPORT_VARIABLES "source Build/common/export-variables.sh"
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj

SHELL ["/bin/bash", "-c"]
RUN $EXPORT_VARIABLES && dotnet_restore
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
RUN $EXPORT_VARIABLES && dotnet_publish && mv /tmp/hwi "$PUBLISH_FOLDER/"

COPY "BTCPayServerVault.png" "BTCPayServerVault.png"

RUN $EXPORT_VARIABLES && \
    cp -r "$RESOURCES_LINUX/udev" "$PUBLISH_FOLDER/" && \
    cp -r $RESOURCES_LINUX/install-*.sh "$PUBLISH_FOLDER/" && \
    cp -r "$RESOURCES_LINUX/BTCPayServerVault.desktop" "$PUBLISH_FOLDER/" && \
    convert -background none -resize "64x64" "BTCPayServerVault.png" "/tmp/BTCPayServerVault.png" && \
    cp "/tmp/BTCPayServerVault.png" "$PUBLISH_FOLDER/" && \
    find "$PUBLISH_FOLDER" -type f -exec chmod 644 {} \; && \
    find "$PUBLISH_FOLDER" -type f \( -name 'hwi' -o -name "$EXECUTABLE" -o -name '*.sh' \) -exec chmod +x {} \; && \
    # We need to cd in "$PUBLISH_FOLDER", because tar's -C option always add a root folder to the tar otherwise
    cd "$PUBLISH_FOLDER" && tar -czf "$DIST/BTCPayServerVault-Linux-$VERSION.tar.gz" *

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp -a $DIST/* /opt/dist/" ]
