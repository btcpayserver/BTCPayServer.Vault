FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-buster AS builder
RUN apt-get update && \
    apt-get install -y --no-install-recommends imagemagick
WORKDIR /source/Build
RUN wget -qO hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-linux-amd64.tar.gz && \
    tar -zxvf hwi.tar.gz -C . hwi && \
    echo "00cb4b2c6eb78d848124e1c3707bdae9c95667f1397dd32cf3b51b579b3a010a hwi" | sha256sum -c - && \
    rm hwi.tar.gz

WORKDIR /source
ENV RUNTIME "debian-x64"
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
COPY Build/control Build/control
COPY Build/BTCPayServer.Vault.desktop Build/BTCPayServer.Vault.desktop
COPY BTCPayServerVault.png BTCPayServerVault.png
SHELL ["/bin/bash", "-c"]
RUN source Build/extract-project-variables.sh "/source/BTCPayServer.Vault/BTCPayServer.Vault.csproj" && \
    mkdir Build/Output && \
    cd "$PUBLISH_FOLDER" && \
    cp /source/Build/hwi . && \
    find . -type f -exec chmod 644 {} \; && \
    find . -type f \( -name 'hwi' -o -name 'BTCPayServer.Vault' \) -exec chmod +x {} \; && \
    debiandir=/tmp/debian && \
    mkdir -p "$debiandir/DEBIAN" && \
    cp "/source/Build/control" "$debiandir/DEBIAN/" && \
    replaceProjectVariables "$debiandir/DEBIAN/control" && \
    mkdir -p "$debiandir/usr/local/bin" && \
    mv "$PUBLISH_FOLDER" "$debiandir/usr/local/bin/BTCPayServer.Vault.Binaries" && \
    echo "#!/bin/sh" > "$debiandir/usr/local/bin/$EXECUTABLE" && \
    echo "/usr/local/bin/BTCPayServer.Vault.Binaries/$EXECUTABLE" >> "$debiandir/usr/local/bin/$EXECUTABLE" && \
    chmod +x "$debiandir/usr/local/bin/$EXECUTABLE" && \
    for size in 128x128 16x16 192x192 22x22 24x24 256x256 32x32 36x36 42x42 48x48 512x512 52x52 64x64 72x72 8x8 96x96; do \
        imagepath="$debiandir/usr/share/icons/hicolor/$size/apps" && \
        mkdir -p "$imagepath" && \
        convert -background none -resize "!$size" "/source/BTCPayServerVault.png" "$imagepath/$EXECUTABLE.png"; \
    done && \
    mkdir -p "$debiandir/usr/share/applications" && \
    cp "/source/Build/BTCPayServer.Vault.desktop" "$debiandir/usr/share/applications/" && \
    replaceProjectVariables "$debiandir/usr/share/applications/BTCPayServer.Vault.desktop" && \
    sizeinkb="$(du -k --max-depth=0 $debiandir | cut -f 1)" && \
    sed -i "s/{SIZEINKB}/$sizeinkb/g" "$debiandir/DEBIAN/control" && \
    dpkg --build "$debiandir" && mv /tmp/debian.deb /source/Build/Output/BTCPayServerVault-$VERSION.deb

ENTRYPOINT [ "/bin/bash", "-c", "cp /source/Build/Output/* /opt/Output/" ]
