New-Item -Path "Output" -ItemType "Directory" -Force
rm -Force -Recurse "Output\**"
docker build -t temp -f build.win-x64.Dockerfile ..
docker run --rm --name vaultbuild -d temp sleep 100
docker cp vaultbuild:/source/Build/Output/  .
docker rm --force vaultbuild
