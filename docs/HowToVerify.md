# How to verify release signatures

## Introduction

Downloading binaries from the internet might be dangerous. When you download a release of BTCPayServer Vault on our [GitHub releases page](https://github.com/btcpayserver/BTCPayServer.Vault/releases), you only ensure that the uploader had access to our GitHub repository.

This might be fine, but sometimes you download the same binaries from a different source, or you want additional assurance that those binaries are signed by the developers of the project. (In this case, Nicolas Dorier)

If you do not care about who signed the executable and verifying the integrity of the files you downloaded, you don't have to read this document.

## Checking PGP signatures<a name="pgp"></a>

For this you need the `gpg` tool, make sure it is installed on your machine.

On the [release page](https://github.com/btcpayserver/BTCPayServer.Vault/releases/latest), download:

1. The release binary of your choice.
2. The `SHA256SUMS.asc` file

### Importing Nicolas Dorier pgp keys (only first time)

This step should be done only one time. It ensures your system knows Nicolas Dorier's PGP keys.

Nicolas Dorier has a [keybase](https://keybase.io/NicolasDorier) account that allow you to verify that his identity is linked to several well-known social media accounts.
And as you can see on his profile page, the PGP key `62FE 8564 7DED DA2E` is linked to his keybase identity.

You can import this key from keybase:

```bash
curl https://keybase.io/nicolasdorier/pgp_keys.asc | gpg --import
```

or

```bash
keybase pgp pull nicolasdorier
```

Alternatively, you can just download the file via the browser and run:

```bash
gpg --import pgp_keys.asc
```

This step won't have to be repeated the next time you need to check a signature.

### Checking the actual PGP signature

```bash
sha256sum --check SHA256SUMS.asc --ignore-missing
```

You should see that the file you downloaded has the right hash:

```text
BTCPayServerVault-1.0.7-setup.exe: OK
```

If you are on Windows you can check the hashes are identical manually:

```powershell
certUtil -hashfile BTCPayServerVault-1.0.7-setup.exe SHA256
type SHA256SUMS.asc
```

If you are on macOS:

```bash
shasum -a 256 --check SHA256SUMS.asc
```

You should see that the file you downloaded has the right hash:

```text
BTCPayServerVault-osx-x64-1.0.7.dmg: OK
```

Then check the actual signature:

```bash
gpg --verify SHA256SUMS.asc
```

Which should output something like:

```text
gpg: Signature made Thu Dec  5 20:40:47 2019 JST
gpg:                using RSA key 62FE85647DEDDA2E
gpg: Good signature from "BTCPayServer Vault <nicolas.dorier@gmail.com>" [unknown]
gpg: WARNING: This key is not certified with a trusted signature!
gpg:          There is no indication that the signature belongs to the owner.
Primary key fingerprint: 7121 BDE3 555D 9BE0 6BDD  C681 62FE 8564 7DED DA2E
```
