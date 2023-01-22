@echo off

rem libc-cil - libc implementation on CIL, part of chibicc-cil
rem Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
rem
rem Licensed under MIT: https://opensource.org/licenses/MIT

echo.
echo "==========================================================="
echo "Build libc"
echo.

rem git clean -xfd

dotnet restore

dotnet build -p:Configuration=Release -p:Platform="Any CPU"
dotnet pack -p:Configuration=Release -p:Platform="Any CPU" -o artifacts
