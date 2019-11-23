FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-buster AS builder

WORKDIR /source/Build
RUN wget -qO hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-linux-amd64.tar.gz && \
    tar -zxvf hwi.tar.gz -C . hwi && \
    echo "00cb4b2c6eb78d848124e1c3707bdae9c95667f1397dd32cf3b51b579b3a010a hwi" | sha256sum -c - && \
    rm hwi.tar.gz

WORKDIR /source
ENV RUNTIME "linux-x64"
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
ENV BUILD_ARGS "--runtime $RUNTIME -p:Configuration=Release -p:GithubDistrib=true"
RUN dotnet restore $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
ENV FRAMEWORK "netcoreapp3.0"
COPY BTCPayServerVault.ico BTCPayServerVault.ico
RUN dotnet publish --no-restore --framework $FRAMEWORK $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj

COPY Build/extract-project-variables.sh Build/extract-project-variables.sh

SHELL ["/bin/bash", "-c"]
RUN source Build/extract-project-variables.sh "/source/BTCPayServer.Vault/BTCPayServer.Vault.csproj" && \
    mkdir Build/Output && \
    cd "$PUBLISH_FOLDER" && \
    && tar -czf "/source/Build/Output/BTCPayServerVault-Linux-$VERSION.tar.gz" *

ENTRYPOINT [ "/bin/bash", "-c", "cp /source/Build/Output/* /opt/Output/" ]
