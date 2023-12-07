FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

RUN wget -qO /tmp/hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/2.1.1/hwi-2.1.1-linux-amd64.tar.gz && \
    tar -zxvf /tmp/hwi.tar.gz -C /tmp hwi && \
    echo "7f4cbe4e5c2cd1ac892f9bd8ac35fb1f837b6a547b528b61aca895a212a90062 /tmp/hwi" | sha256sum -c - && \
    rm /tmp/hwi.tar.gz

WORKDIR /source
ENV RUNTIME "linux-x64"
COPY "Build/common" "Build/common"
ENV EXPORT_VARIABLES "source Build/common/export-variables.sh"
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj

SHELL ["/bin/bash", "-c"]
RUN $EXPORT_VARIABLES && dotnet_restore
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
RUN $EXPORT_VARIABLES && dotnet_publish && mv /tmp/hwi "$PUBLISH_FOLDER/"

RUN $EXPORT_VARIABLES && \
    find "$PUBLISH_FOLDER" -type f -exec chmod 644 {} \; && \
    find "$PUBLISH_FOLDER" -type f \( -name 'hwi' -o -name "$EXECUTABLE" \) -exec chmod +x {} \; && \
    # We need to cd in "$PUBLISH_FOLDER", because tar's -C option always add a root folder to the tar otherwise
    cd "$PUBLISH_FOLDER" && tar -czf "$DIST/BTCPayServerVault-Linux-$VERSION.tar.gz" *

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp $DIST/* /opt/dist/" ]
