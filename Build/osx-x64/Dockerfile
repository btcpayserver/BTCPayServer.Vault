FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

# Optimize docker cache, do not make it one layer
RUN apt-get update
RUN apt-get install -y --no-install-recommends imagemagick
###

RUN apt-get install -y --no-install-recommends git icnsutils

RUN wget -qO /tmp/hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/2.3.1/hwi-2.3.1-mac-x86_64.tar.gz && \
    tar -zxvf /tmp/hwi.tar.gz -C /tmp hwi && \
    echo "9059b8f7cf6fe42f6e37cd8015cd11cb8fb736650797b25da849c625ed61ea62 /tmp/hwi" | sha256sum -c - && \
    rm /tmp/hwi.tar.gz

WORKDIR /source
ENV RUNTIME "osx-x64"
COPY "Build/common" "Build/common"
ENV EXPORT_VARIABLES "source Build/common/export-variables.sh"
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj

SHELL ["/bin/bash", "-c"]
RUN $EXPORT_VARIABLES && dotnet_restore
COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
RUN $EXPORT_VARIABLES && dotnet_publish && mv /tmp/hwi "$PUBLISH_FOLDER/"

COPY "Build/${RUNTIME}" "Build/${RUNTIME}"
COPY BTCPayServerVault.png BTCPayServerVault.png
RUN $EXPORT_VARIABLES && \
    replaceProjectVariables "$RESOURCES/Info.plist" && \
    dest=/tmp/BTCPayServerVault.iconset && mkdir -p $dest && \
    for size in 16x16 32x32 128x128 256x256 512x512 1024x1024; do \
        convert -background none -resize "!$size" "BTCPayServerVault.png" "/tmp/BTCPayServerVault.iconset/icon_${size}.png"; \
    done && \
    png2icns /tmp/BTCPayServerVault.icns /tmp/BTCPayServerVault.iconset/*png

RUN $EXPORT_VARIABLES && \
    dmgroot="/tmp/dmgroot/$TITLE" && \
    appfolder="$dmgroot/$TITLE.app" && \
    mkdir -p "$appfolder/Contents" && \
    cp "$RESOURCES/Info.plist" "$appfolder/Contents/" && \
    cp "$RESOURCES/entitlements.plist" "$appfolder/Contents/" && \
    mv "$PUBLISH_FOLDER" "$appfolder/Contents/MacOS" && \
    mkdir -p "$appfolder/Contents/Resources" && \
    cp /tmp/BTCPayServerVault.icns "$appfolder/Contents/Resources/" && \
    cp /tmp/BTCPayServerVault.icns "$dmgroot/.VolumeIcon.icns" && \
    ln -s /Applications "$dmgroot" && \
    # If one day you need to regenerate the DS_Store in $RESOURCES/Metadata:
    # 1. Get a Mac
    # 2. Install XCode
    # 3. Run "brew install create-dmg"
    # 4. From an old version of BTCPayServer Vault dmg file, mount the dmg, then extract ".VolumeIcon.icns" and
    #    ".background/Logo_with_text_small.png" from inside (!hidden)
    # 5. Create empty dir "mkdir empty"
    # 6. Run create-dmg --volname "BTCPayServer Vault" --volicon .VolumeIcon.icns --background Logo_with_text_small.png --window-pos 200 120 --window-size 600 440 --app-drop-link 500 150 --icon "BTCPayServer Vault" 110 150 --hdiutil-verbose "btcpay.dmg" empty
    # 7. Now you can change .background, .fseventsd and .DS_Store from Build/Osx
    # It is impossible to use create-dmg in shell script, because we can't run Apple scripts without a UI.
    cp -r "$RESOURCES/Metadata/." "$dmgroot" && \
    # We need to cd in "$dmgroot", because tar's -C option always add a root folder to the tar otherwise
    cd "$dmgroot" && shopt -s dotglob && tar -czf "$DIST/BTCPayServerVault-${RUNTIME}-$VERSION.tar.gz" *

ENTRYPOINT [ "/bin/bash", "-c", "$EXPORT_VARIABLES && cp $DIST/* /opt/dist/" ]
