$ver = [regex]::Match((Get-Content ../BTCPayServer.Vault/BTCPayServer.Vault.csproj), '<Version>([^<]+)<').Groups[1].Value
git tag -a "Vault/v$ver" -m "Vault/v$ver"
git push origin "Vault/v$ver"
