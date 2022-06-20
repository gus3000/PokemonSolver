#!/usr/bin/env bash

set -e

pushd $(dirname $0)/..
if [[ -z Bizhawk.tar.zip ]]; then
  wget https://github.com/TASEmulators/BizHawk/releases/download/2.6.2/BizHawk-2.6.2-linux-x64.tar.zip -O BizHawk.tar.zip
fi
rm -rf BizHawk
mkdir BizHawk 
unzip BizHawk.tar.zip -d BizHawk
cd BizHawk
tar xf BizHawk_release_*.tar
rm BizHawk_release_*.tar
popd
