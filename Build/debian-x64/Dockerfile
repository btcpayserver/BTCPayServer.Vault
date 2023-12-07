FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

# Optimize docker cache, do not make it one layer
RUN apt-get update
RUN apt-get install -y --no-install-recommends imagemagick
###

RUN wget -qO /tmp/hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/2.3.1/hwi-2.3.1-linux-x86_64.tar.gz && \
    tar -zxvf /tmp/hwi.tar.gz -C /tmp hwi && \
    echo "9519023b3a485b68668675db8ab70be2e338be100fd2731eeddd6d34fc440580 /tmp/hwi" | sha256sum -c - && \
    rm /tmp/hwi.tar.gz

SHELL ["/bin/bash", "-c"]
ARG PGP_KEY=""
RUN ! [[ "${PGP_KEY}" ]] || apt-get install -y debsigs

WORKDIR /source
ENV DOTNET_RUNTIME "linux-x64"
ENV RUNTIME "debian-x64"
COPY "Build/common" "Build/common"
ENV EXPORT_VARIABLES "source Build/common/export-variables.sh"
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj

RUN $EXPORT_VARIABLES && dotnet_restore
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
RUN $EXPORT_VARIABLES && dotnet_publish && mv /tmp/hwi "$PUBLISH_FOLDER/"

COPY "Build/${RUNTIME}" "Build/${RUNTIME}"
COPY BTCPayServerVault.png BTCPayServerVault.png
RUN $EXPORT_VARIABLES && \
    find "$PUBLISH_FOLDER" -type f -exec chmod 644 {} \; && \
    find "$PUBLISH_FOLDER" -type f \( -name 'hwi' -o -name "$EXECUTABLE" \) -exec chmod +x {} \; && \
    debiandir=/tmp/debian && \
    mkdir -p "$debiandir/DEBIAN" && \
    cp "$RESOURCES/control" "$RESOURCES/postinst" "$debiandir/DEBIAN/" && \
    replaceProjectVariables "$debiandir/DEBIAN/control" && \
    mkdir -p "$debiandir/usr/local/bin" && \
    mv "$PUBLISH_FOLDER" "$debiandir/usr/local/bin/BTCPayServer.Vault.Binaries" && \
    echo "#!/bin/sh" > "$debiandir/usr/local/bin/$EXECUTABLE" && \
    echo "/usr/local/bin/BTCPayServer.Vault.Binaries/$EXECUTABLE" >> "$debiandir/usr/local/bin/$EXECUTABLE" && \
    chmod +x "$debiandir/usr/local/bin/$EXECUTABLE" && \
    for size in 128x128 16x16 192x192 22x22 24x24 256x256 32x32 36x36 42x42 48x48 512x512 52x52 64x64 72x72 8x8 96x96; do \
        imagepath="$debiandir/usr/share/icons/hicolor/$size/apps" && \
        mkdir -p "$imagepath" && \
        convert -background none -resize "!$size" "BTCPayServerVault.png" "$imagepath/$EXECUTABLE.png"; \
    done && \
    mkdir -p "$debiandir/usr/share/applications" && \
    cp "$RESOURCES/BTCPayServer.Vault.desktop" "$debiandir/usr/share/applications/" && \
    replaceProjectVariables "$debiandir/usr/share/applications/BTCPayServer.Vault.desktop" && \
    mkdir -p "$debiandir/lib/udev/rules.d" && \
    cp $RESOURCES/udev/* "$debiandir/lib/udev/rules.d/" && \
    sizeinkb="$(du -k --max-depth=0 $debiandir | cut -f 1)" && \
    sed -i "s/{SIZEINKB}/$sizeinkb/g" "$debiandir/DEBIAN/control" && \
    dpkg --build "$debiandir" && mv /tmp/debian.deb "$DIST/BTCPayServerVault-$VERSION.deb" && \
    ( ! [[ "${PGP_KEY}" ]] || \
    ( \
        echo "$PGP_KEY" | base64 --decode | gpg --import && \
        apt-get install -y debsigs && \
        debsigs --sign=maint "$DIST/BTCPayServerVault-$VERSION.deb" && \
        echo "The debian package has been pgp signed" \
    ))

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp $DIST/* /opt/dist/" ]
