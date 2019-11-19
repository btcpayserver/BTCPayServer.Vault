FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100 AS builder

WORKDIR /source
COPY BTCPayServer.Hwi/BTCPayServer.Hwi.csproj BTCPayServer.Hwi/BTCPayServer.Hwi.csproj
COPY BTCPayServer.Vault/BTCPayServer.Vault.csproj BTCPayServer.Vault/BTCPayServer.Vault.csproj

ENV  RUNTIME_IDS "win-x64 linux-x64 osx-x64"
ENV  BUILD_ARGS "-p:Configuration=Release -p:GithubDistrib=true"
RUN for rid in $RUNTIME_IDS; do dotnet restore --runtime $rid $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj; done

COPY BTCPayServer.Hwi BTCPayServer.Hwi
COPY BTCPayServer.Vault BTCPayServer.Vault
RUN for rid in $RUNTIME_IDS; do dotnet publish --no-restore --framework netcoreapp3.0 --runtime $rid $BUILD_ARGS BTCPayServer.Vault/BTCPayServer.Vault.csproj; done
