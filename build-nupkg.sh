#!/bin/sh

# libc-cil - libc implementation on CIL, part of chibicc-cil
# Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
#
# Licensed under MIT: https://opensource.org/licenses/MIT

echo
echo "==========================================================="
echo "Build libc"
echo

# git clean -xfd

dotnet restore

dotnet build -p:Configuration=Release -p:Platform="Any CPU"
dotnet pack -p:Configuration=Release -p:Platform="Any CPU" -o artifacts
