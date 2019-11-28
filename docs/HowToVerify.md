# How to verify release binaries signatures

## Introduction

Downloading binaries on internet might be dangerous. When you download the binaries of the release of BTCPayServer Vault on our github release page, you only ensure that the uploader had access to our github repository.

This might be fine, but sometimes you download the same binaries from a different source, or you want additional assurance that those binaries are signed by the developers of the project. (In this case, Nicolas Dorier)
This document explain the process on linux.

## On linux

From the release page, download the release binary of your choice, as well as the `SHA256SUMS.asc` file and put those in the same directory.

```bash
sha256sum --check SHA256SUMS.asc
```

If you downloaded archive `BTCPayServerVault-0.0.7.tar.gz` you should see `BTCPayServerVault-0.0.7.tar.gz: OK`.
This make sure that the hashes in `SHA256SUMS.asc` are the same as the file, making sure the file has not been corrupted.
However, now you want to make sure that Nicolas Dorier signed the binaries.

Nicolas Dorier has a [keybase](https://keybase.io/NicolasDorier) account that allow you to verify that his identity is linked to several well-known social media accounts.
And as you can see on his profile page, the PGP key `62FE 8564 7DED DA2E` is linked to his keybase identity.

You can import this key from keybase:

```bash
curl https://keybase.io/nicolasdorier/pgp_keys.asc?fingerprint=7121bde3555d9be06bddc68162fe85647dedda2e | gpg --import
```

Then you can finally check the hashes:
```bash
gpg2 --verify SHA256SUMS.asc
```

Which should output something like:

```
gpg: Signature made Thu Nov 28 01:14:12 2019 JST
gpg:                using RSA key 62FE85647DEDDA2E
gpg: Good signature from "BTCPayServer Vault <nicolas.dorier@gmail.com>" [unknown]
gpg: WARNING: This key is not certified with a trusted signature!
gpg:          There is no indication that the signature belongs to the owner.
Primary key fingerprint: 7121 BDE3 555D 9BE0 6BDD  C681 62FE 8564 7DED DA2E
```
