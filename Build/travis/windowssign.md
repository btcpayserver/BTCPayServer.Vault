# Windows signing process

## Introduction
This document how to setup your travis environment so that it can properly sign BTCPayServer Vault as part of the build process.

At the end of this process, you will have configured `WINDOWS_CERT` and `WINDOWS_CERT_PASSWORD` to the correct value for travis to sign the windows installer.

## How to

### Generate a self-signed certificate

You can skip this if you managed to get a certificate for code signing from a Certificate Authority. This part is only meant to test that the installer get properly signed.

Else, this part will assume you are on linux.

```bash
# This will prompt you to give twice some information
# We advise you to enter Country Code, State, Company and Common Name, keeping the rest blank
# Do not pick a blank password
selfsignedcert.sh
```
This generate `codesign.pfx`, which are the private keys you can use to sign windows binaries.

You can install this private key in windows as a Root Authority certificate to see if windows properly open the built installer.

### Configure the certificate in Travis

For travis to sign BTCPayServer.Vault setup file, you need to convert the certificate to base64 with this command line:
```bash
cat codesign.pfx | base64 -w0
```

Then setup your travis environment variable `WINDOWS_CERT` to this value **SURROUNDED BY DOUBLE QUOTE ("")**.

Additionally setup the travis environment variable `WINDOWS_CERT_PASSWORD`, **SURROUNDED BY DOUBLE QUOTE ("")**.

