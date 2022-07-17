#!/usr/bin/env bash

set -e

isWsl=$(grep -i microsoft /proc/version)

if [[ "$isWsl" ]]; then
  echo "WSL detected"
  file=BizHawk-2.6.2-win-x64.zip
else
  echo "Native linux detected"
  file=BizHawk-2.6.2-linux-x64.tar.zip
fi

pushd $(dirname $0)/..
if [[ ! -f $file ]]; then
  wget "https://github.com/TASEmulators/BizHawk/releases/download/2.6.2/$file"
fi
rm -rf BizHawk
mkdir BizHawk
unzip "$file" -d BizHawk
cd BizHawk
if [[ ! "$isWsl" ]]; then
  tar xf BizHawk_release_*.tar
  rm BizHawk_release_*.tar
fi
popd
