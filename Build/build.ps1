New-Item -Path "Output" -ItemType "Directory" -Force
rm -Force -Recurse "Output\**"
docker build -t temp -f build.Dockerfile ..
docker run --rm --name vaultbuild -d temp sleep 100
docker cp vaultbuild:/source/BTCPayServer.Vault/bin/Release/netcoreapp3.0/osx-x64/publish  Output/osx-x64
docker cp vaultbuild:/source/BTCPayServer.Vault/bin/Release/netcoreapp3.0/linux-x64/publish  Output/linux-x64
docker cp vaultbuild:/source/BTCPayServer.Vault/bin/Release/netcoreapp3.0/win-x64/publish  Output/win-x64
docker rm --force vaultbuild
