FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100 AS builder

# Optimize docker cache, do not make it one layer
RUN apt-get update
RUN apt-get install -y --no-install-recommends imagemagick
###

RUN apt-get install -y --no-install-recommends nsis unzip wine xxd osslsigncode openssl
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

RUN $EXPORT_VARIABLES && \
    dotnet_publish && mv /tmp/hwi.exe "$PUBLISH_FOLDER/"

COPY BTCPayServerVault.png BTCPayServerVault.png
COPY "Build/${RUNTIME}" "Build/${RUNTIME}"
RUN $EXPORT_VARIABLES && \
    mkdir -p "/tmp/BTCPayServerVault.ico.tmp" && \
    for size in 256x256 48x48 32x32 16x16; do \
        convert -background none -resize "!$size" "BTCPayServerVault.png" "PNG32:/tmp/BTCPayServerVault.ico.tmp/BTCPayServerVault-$size.png"; \
    done && \
    convert /tmp/BTCPayServerVault.ico.tmp/*.png /tmp/BTCPayServerVault.ico && \
    executable="$PUBLISH_FOLDER/$EXECUTABLE.exe" && \
    # Workaround https://github.com/dotnet/sdk/issues/3990
    e_lfanew_loc=$(xxd -p -l1 -s $((16#3C)) "$executable") && \
    e_lfanew_loc="$((16#$e_lfanew_loc))" && \
    subsystem_loc=$(($e_lfanew_loc + 84)) && \
    printf "%06x: 0200" $subsystem_loc | xxd -r - "$executable" && \
    # End of workaround
    wine /tmp/rcedit.exe "$executable" \
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

ARG WINDOWS_CERT
ARG WINDOWS_CERT_PASSWORD

# Let's sign the installer if we can
RUN ! ([[ "$WINDOWS_CERT" ]] && [[ "$WINDOWS_CERT_PASSWORD" ]]) || (\
        $EXPORT_VARIABLES && cd "$DIST" && \
        echo "$WINDOWS_CERT" | base64 --decode > codesign.pfx && \
        openssl pkcs12 -in codesign.pfx -nocerts \
            -passin "pass:$WINDOWS_CERT_PASSWORD" -passout "pass:$WINDOWS_CERT_PASSWORD" \
            -out codesign.key && \
        openssl pkcs12 -in codesign.pfx -clcerts -nokeys \
            -passin "pass:$WINDOWS_CERT_PASSWORD" \
            -out codesign.crt && \
        installer_file="$(ls -1 *.exe)" && \
        intaller_file_signed="${installer_file}.signed" && \
        osslsigncode sign -certs "codesign.crt" -t "http://timestamp.comodoca.com" \
                        -pass "$WINDOWS_CERT_PASSWORD" \
                        -in "$installer_file" -out "$intaller_file_signed" \
                        -key "codesign.key" && \
        rm codesign.key codesign.pfx "$installer_file" && \
        mv "$intaller_file_signed" "$installer_file" && \
        chmod 777 "codesign.crt" && \
        echo "Installer successfully signed" \
    )

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp $DIST/* /opt/dist/" ]
