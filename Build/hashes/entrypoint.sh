#!/bin/bash

touch /tmp/SHA256SUMS
cd /opt/dist
for f in *; do
  if [[ "$f" == "SHA256SUMS" ]]; then continue; fi
  sha256sum $f >> /tmp/SHA256SUMS
done

mv /tmp/SHA256SUMS /opt/dist/SHA256SUMS

