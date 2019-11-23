New-Item -Path "Output" -ItemType "Directory" -Force
rm -Force -Recurse "Output\**"


foreach ($arch in "linux-x64","win-x64","osx-x64")
{
  docker build -t "vault-$arch" -f "build.${arch}.Dockerfile" ..
  docker run --rm -v "$(pwd)/Output:/opt/Output" "vault-$arch"
}
