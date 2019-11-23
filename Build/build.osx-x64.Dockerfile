FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-buster AS builder
RUN apt-get update && \
    apt-get install -y --no-install-recommends genisoimage git zlib1g-dev cmake make gcc g++ icnsutils imagemagick
WORKDIR /source/Build
RUN wget -qO hwi.tar.gz https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-mac-amd64.tar.gz && \
    tar -zxvf hwi.tar.gz -C . hwi && \
    echo "b0219f4f51d74e4525dd57a19f1aee9df409a879e041ea65f2d70cf90ac48a32 hwi" | sha256sum -c - && \
    rm hwi.tar.gz

WORKDIR /tmp
ENV CC "gcc"
ENV CXX "g++"
RUN git clone https://github.com/theuni/libdmg-hfsplus && \
    cd libdmg-hfsplus && \
    git checkout libdmg-hfsplus-v0.1 && \
    cmake . && \
    make && \
    cp dmg/dmg /bin && \
    cd .. && rm -rf libdmg-hfsplus
    

WORKDIR /source
ENV RUNTIME "osx-x64"
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
COPY Build/Info.plist.xml Build/Info.plist
SHELL ["/bin/bash", "-c"]
RUN source Build/extract-project-variables.sh "BTCPayServer.Vault/BTCPayServer.Vault.csproj" && \
    sed -i "s/{VersionPrefix}/$VERSION/g" Build/Info.plist && \
    sed -i "s/{ApplicationName}/$TITLE/g" Build/Info.plist

COPY BTCPayServerVault.png /tmp/BTCPayServerVault.png
COPY Build/Osx Build/Osx
WORKDIR /tmp
RUN src=/tmp/BTCPayServerVault.png && \
    dest=/tmp/BTCPayServerVault.iconset && mkdir $dest && \
    convert -background none -resize '!16x16' "$src" "$dest/icon_16x16.png" && \
    convert -background none -resize '!32x32' "$src" "$dest/icon_16x16@2x.png" && \
    cp "$dest/icon_16x16@2x.png" "$dest/icon_32x32.png" && \
    convert -background none -resize '!64x64' "$src" "$dest/icon_32x32@2x.png" && \
    convert -background none -resize '!128x128' "$src" "$dest/icon_128x128.png" && \
    convert -background none -resize '!256x256' "$src" "$dest/icon_128x128@2x.png" && \
    cp "$dest/icon_128x128@2x.png" "$dest/icon_256x256.png" && \
    convert -background none -resize '!512x512' "$src" "$dest/icon_256x256@2x.png" && \
    cp "$dest/icon_256x256@2x.png" "$dest/icon_512x512.png" && \
    convert -background none -resize '!1024x1024' "$src" "$dest/icon_512x512@2x.png" && \
    # available on linux
    # png2icns is more finicky about its input than iconutil
    # 1. it doesn't support a 64x64 (aka 32x32@2x)
    # 2. it doesn't like duplicates (128x128@2x == 256x256)
    rm "$dest/icon_128x128@2x.png" && \
    rm "$dest/icon_256x256@2x.png" && \
    rm "$dest/icon_16x16@2x.png" && \
    rm "$dest/icon_32x32@2x.png" && \
    png2icns /source/Build/BTCPayServerVault.icns "$dest"/*png

RUN source /source/Build/extract-project-variables.sh "/source/BTCPayServer.Vault/BTCPayServer.Vault.csproj" && \
    dmgroot="/tmp/Output/$TITLE" && \
    appfolder="$dmgroot/$TITLE.app" && \
    mkdir -p "$appfolder/Contents" && \
    cp /source/Build/Info.plist "$appfolder/Contents/" && \
    mv "$PUBLISH_FOLDER" "$appfolder/Contents/MacOS" && \
    mv /source/Build/hwi "$appfolder/Contents/MacOS/" && \
    mkdir -p "$appfolder/Contents/Resources" && \
    cp /source/Build/BTCPayServerVault.icns "$appfolder/Contents/Resources/" && \
    cp /source/Build/BTCPayServerVault.icns "$dmgroot/.VolumeIcon.icns" && \
    ln -s /Applications "$dmgroot" && \
    # If one day you need to regenerate the DS_Store in /source/Build/Osx:
    # 1. Get a Mac
    # 2. Install XCode
    # 3. Run "brew install create-dmg"
    # 4. From an old version of BTCPayServer Vault dmg file, mount the dmg, then extract ".VolumeIcon.icns" and
    #    ".background/Logo_with_text_small.png" from inside (!hidden)
    # 5. Create empty dir "mkdir empty"
    # 6. Run create-dmg --volname "BTCPayServer Vault" --volicon .VolumeIcon.icns --background Logo_with_text_small.png --window-pos 200 120 --window-size 600 440 --app-drop-link 500 150 --icon "BTCPayServer Vault" 110 150 --hdiutil-verbose "btcpay.dmg" empty
    # 7. Now you can change .background, .fseventsd and .DS_Store from Build/Osx
    cp -r /source/Build/Osx/. "$dmgroot" && \
    genisoimage -no-cache-inodes \
    -D \
    -l \
    -probe \
    -V "$TITLE" \
    -no-pad \
    -r \
    -dir-mode 0755 \
    -apple \
    -o uncompressed.dmg \
    "$dmgroot" && \
    mkdir -p /source/Build/Output && \
    dmg dmg uncompressed.dmg /source/Build/Output/BTCPayServerVault-$VERSION.dmg && rm uncompressed.dmg

ENTRYPOINT [ "/bin/bash", "-c", "cp /source/Build/Output/* /opt/Output/" ]
