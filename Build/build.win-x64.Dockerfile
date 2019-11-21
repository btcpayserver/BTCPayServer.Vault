FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-buster AS builder
RUN apt-get update
RUN apt-get install -y --no-install-recommends nsis unzip wine
WORKDIR /source/Build
RUN wget -qO hwi.zip https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-windows-amd64.zip && \
    unzip hwi.zip && \
    echo "f52ec4c8dd2dbef4aabe28d8a49580bceb54fd609b84c753d6354eeecbd6dc7a hwi.exe" | sha256sum -c - && \
    rm hwi.zip

RUN wget -qO rcedit.exe https://ci.appveyor.com/api/buildjobs/fcus8m4triujcj2b/artifacts/Default%2Frcedit-x64.exe && \
    echo "b8dda19cd775798beeca7b4bf6fe2d27580d38d7d8c833ea173f7a1ba529d9cb rcedit.exe" | sha256sum -c -

WORKDIR /source
ENV RUNTIME "win-x64"
COPY BTCPayServerVault.ico BTCPayServerVault.ico
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
ENV BUILD_ARGS "--runtime $RUNTIME -p:Configuration=Release -p:GithubDistrib=true"
RUN dotnet restore $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
ENV FRAMEWORK "netcoreapp3.0"
RUN dotnet publish --no-restore --framework $FRAMEWORK $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj

COPY Build/extract-project-variables.sh Build/extract-project-variables.sh
COPY Build/nsis-header.bmp Build/nsis-header.bmp
COPY Build/nsis-wizard.bmp Build/nsis-wizard.bmp
# Need to setup with rcedit because https://github.com/dotnet/sdk/issues/3943
SHELL ["/bin/bash", "-c"]
RUN cd BTCPayServer.Vault && \
    source ../Build/extract-project-variables.sh "BTCPayServer.Vault.csproj" && \
    WINEDEBUG=fixme-all wine ../Build/rcedit.exe bin/Release/$FRAMEWORK/$RUNTIME/publish/BTCPayServer.Vault.exe \
    --set-icon "$ICON" \
    --set-version-string "LegalCopyright" "$LICENSE" \
    --set-version-string "CompanyName" "$COMPANY" \
    --set-version-string "FileDescription" "$DESCRIPTION" \
    --set-version-string "ProductName" "$TITLE" \
    --set-file-version "$VERSION" \
    --set-product-version "$VERSION"

WORKDIR /source/Build
COPY Build/vault.nsis vault.nsis
RUN mkdir -p Output && \
    . extract-project-variables.sh "../BTCPayServer.Vault/BTCPayServer.Vault.csproj" && \
    makensis \
    "-DPRODUCT_VERSION=$VERSION" \
    "-DPRODUCT_NAME=$TITLE" \
    "-DPRODUCT_PUBLISHER=$COMPANY" \
    "-DPRODUCT_DESCRIPTION=$DESCRIPTION" \
    vault.nsis
