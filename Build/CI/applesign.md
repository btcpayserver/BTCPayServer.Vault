# Apple signing process

## Introduction
This document how to setup your travis environment so that it can properly sign BTCPayServer Vault as part of the build process.

At the end of this process, you will have configured `APPLE_DEV_ID_CERT` and `APPLE_DEV_ID_CERT_PASSWORD` to the correct value for travis to sign your MAC application.

You will also need to set `APPLE_ID`, `APPLE_ID_PASSWORD` and `APPLE_TEAM_ID`, using [app-specific password](https://support.apple.com/en-us/HT204397).

## How to
If you are on linux and try to sign with an apple certificate you need to do the following steps:

1. You need an apple trusted device, because enrolling to apple developer program require two factor authentication, (SMS is not enough) with a trusted device confirmation. Once you enrolled to the apple developer program, you don't need it anymore. So you can ask a friend to help you for this, his device does not need to be registered with your Apple ID.
2. Enroll in the apple developer program (100USD per year).
3. Get the apple certificate in a `.p12` file (this is what the rest of this page is about)

Then, here is the script to create your certificate:
```bash
# Configure those variables to your need
email="nicolas.dorier@gmail.com"
common_name="Nicolas Dorier"
country_code="JP"
```

Now, go on apple developer program website, create a `Developer ID Application` certificate. It will ask you to upload a `certificate request file`.

Let's create it:
```bash
rsa_key_file="temp.key"
csr_file_name="request.csr"
openssl genrsa -out "$rsa_key_file" 2048
openssl req -new -key "$rsa_key_file" -out "$csr_file_name" -subj "/emailAddress=$email, CN=$common_name, C=$country_code"
```
This will create a request file as `request.csr` in your current folder.

Upload the `csr` back to apple, this will give you back a `.cer` file.

Save this file as `developerID_application.cer` in the current folder.

Now you need to export this in the `.p12` format, this will bundle the `.cer` and the private key together in the same file.

```bash
cer_file="developerID_application.cer"
pem_file="developerID_application.pem"
cert_output_file="developerID_application.p12"
openssl x509 -in "$cer_file" -inform DER -out "$pem_file" -outform PEM
openssl pkcs12 -export -inkey "$rsa_key_file" -in "$pem_file" -out "$cert_output_file"
```

Now enter a password, don't pick an empty one as the rest would fail.

```bash
# Cleanup what we don't need anymore
rm $csr_file_name $cer_file $pem_file $rsa_key_file
```

Now you should have a file called `developerID_application.p12`. 
This is your certificate that you can easily upload where you need to sign binaries.

For travis to sign BTCPayServer.Vault dmg file, you need to convert the certificate to base64 with this command line:
```bash
cat $cert_output_file | base64 -w0
```

