FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-buster AS builder
RUN apt-get update
RUN apt-get install -y nsis zip

WORKDIR /source
RUN wget -qO hwi.zip https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-windows-amd64.zip && \
    unzip hwi.zip && \
    echo "f52ec4c8dd2dbef4aabe28d8a49580bceb54fd609b84c753d6354eeecbd6dc7a hwi.exe" | sha256sum -c - && \
    mkdir -p Build && mv hwi.exe Build/hwi.exe

COPY BTCPayServerVault.ico BTCPayServerVault.ico
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
ENV BUILD_ARGS "--runtime win-x64 -p:Configuration=Release -p:GithubDistrib=true"
RUN dotnet restore $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
RUN dotnet publish --no-restore --framework netcoreapp3.0 $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj

WORKDIR /source/Build
COPY Build/vault.nsis vault.nsis
RUN mkdir -p Output && \
    PRODUCT_VERSION="$(cat ../BTCPayServer.Vault/BTCPayServer.Vault.csproj | sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p')" && \
    makensis -DPRODUCT_VERSION=$PRODUCT_VERSION vault.nsis
