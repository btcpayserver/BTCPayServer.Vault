New-Item -Path "..\dist" -ItemType "Directory" -Force
Remove-Item -Force -Recurse "..\dist\**"

$distFolder="$(Get-Location)\..\dist"
foreach ($arch in "debian-x64")
{
  docker build -t "vault-$arch" -f "$arch/Dockerfile" ..
  docker run --rm -v "${distFolder}:/opt/dist" "vault-$arch"
}
docker build -t "vault-hashes" -f "hashes/Dockerfile" "hashes"
docker run --rm -v "${distFolder}:/opt/dist" "vault-hashes"
keybase pgp sign -i "${distFolder}/SHA256SUMS" -k "015b4c837b245509e4ac8995223fda69debea82d" -c -t -o "${distFolder}/SHA256SUMS.asc"
Remove-Item "${distFolder}/SHA256SUMS"
