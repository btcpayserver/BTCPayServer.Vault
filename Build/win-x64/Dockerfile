FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-buster AS builder

# Optimize docker cache, do not make it one layer
RUN apt-get update
RUN apt-get install -y --no-install-recommends imagemagick
###

RUN apt-get install -y --no-install-recommends nsis unzip wine
RUN wget -qO "/tmp/hwi.zip" https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-windows-amd64.zip && \
    unzip "/tmp/hwi.zip" -d "/tmp" && \
    echo "f52ec4c8dd2dbef4aabe28d8a49580bceb54fd609b84c753d6354eeecbd6dc7a /tmp/hwi.exe" | sha256sum -c - && \
    rm "/tmp/hwi.zip" && \
    # Need to setup with rcedit because https://github.com/dotnet/sdk/issues/3943
    wget -qO "/tmp/rcedit.exe" https://ci.appveyor.com/api/buildjobs/fcus8m4triujcj2b/artifacts/Default%2Frcedit-x64.exe && \
    echo "b8dda19cd775798beeca7b4bf6fe2d27580d38d7d8c833ea173f7a1ba529d9cb /tmp/rcedit.exe" | sha256sum -c -

WORKDIR /source
ENV RUNTIME "win-x64"
COPY "Build/common" "Build/common"
ENV EXPORT_VARIABLES "source Build/common/export-variables.sh"
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj

SHELL ["/bin/bash", "-c"]
RUN $EXPORT_VARIABLES && dotnet_restore
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
COPY BTCPayServerVault.png BTCPayServerVault.png

RUN $EXPORT_VARIABLES && \
    dotnet_publish && mv /tmp/hwi.exe "$PUBLISH_FOLDER/"

COPY "Build/${RUNTIME}" "Build/${RUNTIME}"
RUN $EXPORT_VARIABLES && \
    mkdir -p "/tmp/BTCPayServerVault.ico.tmp" && \
    for size in 256x256 48x48 32x32 16x16; do \
        convert -background none -resize "!$size" "BTCPayServerVault.png" "PNG32:/tmp/BTCPayServerVault.ico.tmp/BTCPayServerVault-$size.png"; \
    done && \
    convert /tmp/BTCPayServerVault.ico.tmp/*.png /tmp/BTCPayServerVault.ico && \
    wine /tmp/rcedit.exe "$PUBLISH_FOLDER/$EXECUTABLE.exe" \
    --set-icon "/tmp/BTCPayServerVault.ico" \
    --set-version-string "LegalCopyright" "$LICENSE" \
    --set-version-string "CompanyName" "$COMPANY" \
    --set-version-string "FileDescription" "$DESCRIPTION" \
    --set-version-string "ProductName" "$TITLE" \
    --set-file-version "$VERSION" \
    --set-product-version "$VERSION" && \
    makensis \
    "-DICON=/tmp/BTCPayServerVault.ico" \
    "-DICONNAME=BTCPayServerVault.ico" \
    "-DPRODUCT_VERSION=$VERSION" \
    "-DPRODUCT_NAME=$TITLE" \
    "-DPRODUCT_PUBLISHER=$COMPANY" \
    "-DPRODUCT_DESCRIPTION=$DESCRIPTION" \
    "-DDIST=$DIST" \
    "-DEXECUTABLE=$EXECUTABLE" \
    "-DPUBLISH_FOLDER=$PUBLISH_FOLDER" \
    "-DRESOURCES=${RESOURCES}" \
    "$RESOURCES/vault.nsis"

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp $DIST/* /opt/dist/" ]
