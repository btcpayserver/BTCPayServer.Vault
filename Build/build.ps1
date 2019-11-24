New-Item -Path "..\dist" -ItemType "Directory" -Force
Remove-Item -Force -Recurse "..\dist\**"


foreach ($arch in "debian-x64","linux-x64","win-x64","osx-x64")
{
  docker build -t "vault-$arch" -f "$arch/Dockerfile" ..
  docker run --rm -v "$(Get-Location)\..\dist:/opt/dist" "vault-$arch"
}
