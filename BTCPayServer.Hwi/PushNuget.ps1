rm "bin\release\" -Recurse -Force
dotnet pack --configuration Release .\BTCPayServer.Hwi.csproj
dotnet nuget push "bin\Release\" --source "https://api.nuget.org/v3/index.json" .\BTCPayServer.Hwi.csproj
$ver = ((ls .\bin\release\*.nupkg)[0].Name -replace 'BTCPayServer\.Hwi\.(\d+(\.\d+){1,3}).*', '$1')
git tag -a "Hwi/v$ver" -m "Hwi/$ver"
git push --tags
