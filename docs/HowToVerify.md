# How to verify release binaries signatures

## Introduction

Downloading binaries on internet might be dangerous. When you download the binaries of the release of BTCPayServer Vault on our github release page, you only ensure that the uploader had access to our github repository.

This might be fine, but sometimes you download the same binaries from a different source, or you want additional assurance that those binaries are signed by the developers of the project. (In this case, Nicolas Dorier)

If you do not care about who signed the executable and verifying the integrity of the files you downloaded, you don't have to read this document.

## Cheking PGP signatures<a name="pgp"></a>

For this you need the `gpg` tool, make sure it is installed on your machine.

In the [release page](https://github.com/btcpayserver/BTCPayServer.Vault/releases/latest), download:

1. The release binary of your choice.
2. The `SHA256SUMS.asc` file

Then we will go through how to install Nicolas Dorier PGP keys on your system, and check the actual binaries.

### Importing Nicolas Dorier pgp keys (only first time)

This step should be done only one time. It makes sure your system knows Nicolas Dorier PGP Keys.

Nicolas Dorier has a [keybase](https://keybase.io/NicolasDorier) account that allow you to verify that his identity is linked to several well-known social media accounts.
And as you can see on his profile page, the PGP key `62FE 8564 7DED DA2E` is linked to his keybase identity.

You can import this key from keybase:

```bash
curl https://keybase.io/nicolasdorier/pgp_keys.asc?fingerprint=7121bde3555d9be06bddc68162fe85647dedda2e | gpg --import
```
or
```
keybase pgp pull nicolasdorier
```

Alternatively, you can just download the file via the browser and run:

```bash
gpg --import pgp_keys.asc
```

This step won't have to be repeated the next time you need to check a signature.

### Checking the actual PGP signature

```
sha256sum --check SHA256SUMS.asc
```

You should see that the file you downloaded has the right hash:
```
BTCPayServerVault-1.0.7-setup.exe: OK
```

If you are on Windows you can check the hash manually:
```powershell
certUtil -hashfile BTCPayServerVault-1.0.7-setup.exe SHA256
type SHA256SUMS.asc
```
And verify the hashes match identically.

Then check the actual signature:

```
gpg --verify SHA256SUMS.asc
```

Which should output something like:

```
gpg: Signature made Thu Dec  5 20:40:47 2019 JST
gpg:                using RSA key 62FE85647DEDDA2E
gpg: Good signature from "BTCPayServer Vault <nicolas.dorier@gmail.com>" [unknown]
gpg: WARNING: This key is not certified with a trusted signature!
gpg:          There is no indication that the signature belongs to the owner.
Primary key fingerprint: 7121 BDE3 555D 9BE0 6BDD  C681 62FE 8564 7DED DA2E
```
