#!/bin/bash

# Source http://thecuriousgeek.org/2014/02/creating-openssl-code-signing-certs-on-windows/
# This create a self signed .pfx which you can import in your CA to test authenticode

echo "[ v3_req  ] 
# Extensions to add to a certificate request
subjectKeyIdentifier=hash
basicConstraints = CA:FALSE
keyUsage = digitalSignature
extendedKeyUsage = codeSigning, msCodeInd, msCodeCom
nsCertType = objsign" > v3.cfg

subject_root="/CN=Nicolas Dorier (Root)/O=BTCPayServer/ST=Tokyo/C=JP"
subject="/CN=Nicolas Dorier/O=BTCPayServer/ST=Tokyo/C=JP"
password="greenbanana"
# Create the key for the Certificate Authority.  2048 is the bit encryptiong, you can set it whatever you want
openssl genrsa -out ca.key 2048
# Next create the certificate and self-sign it (what the -new and -x509 do).  Note, I'm explicitly telling it the main config path.  You have to.
openssl req -new -x509 -days 1826 -key ca.key -out ca.crt -subj "$subject_root"
# Now I'm creating the private key that will be for the actual code signing cert
openssl genrsa -out codesign.key 2048
# Creating the request for a certificate here.  Note the -reqexts you need to tell it to pull that section from the main config or it wont do it.
openssl req -new -key codesign.key -reqexts v3_req -out codesign.csr -subj "$subject"
# Signing the code signing cert with the certificate authority I created.  Note the -extfile this is where you point to the new .cfg you made.
openssl x509 -req -days 1826 -in codesign.csr -CA ca.crt -CAkey ca.key -extensions v3_req -extfile v3.cfg -set_serial 01 -out codesign.crt
# Now I"m expoorting my key and crt into a PKCS12 (.pfx file) so that I can import it onto the machine that I'm going to use it to sign on.
openssl pkcs12 -export -out codesign.pfx -inkey codesign.key -in codesign.crt -password "pass:$password" -chain -CAfile ca.crt
rm ca.crt  ca.key  codesign.crt  codesign.csr  codesign.key  v3.cfg
