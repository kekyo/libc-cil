#!/bin/bash

set -e

#./build-crt0.sh
#dotnet build

#----------------------------------

NEWLIB_BIN_PATH=`pwd`/newlib-bin
NEWLIB_PREFIX=$DOTNET_ROOT/tools/

export PATH=$NEWLIB_BIN_PATH:$PATH

export CHIBIAS_CIL_PATH=$NEWLIB_BIN_PATH/cil-ecma-as
export CHIBIAR_CIL_PATH=$NEWLIB_BIN_PATH/cil-ecma-ar
export CHIBILD_CIL_PATH=$NEWLIB_BIN_PATH/cil-ecma-ld

export CHIBICC_CIL_INCLUDE_PATH=`pwd`/../chibicc-cil/include
#export CHIBICC_CIL_INCLUDE_PATH=`pwd`/newlib/newlib/libc/include
#export CHIBICC_CIL_ADDITIONAL_INCLUDE_PATH=`pwd`/newlib-include

export CHIBICC_CIL_LIB_PATH=`pwd`/libc-bootstrap/bin/Debug/netstandard2.0

#----------------------------------

rm -rf .build-newlib
mkdir .build-newlib
cd .build-newlib

export CFLAGS_FOR_TARGET;

CC=$NEWLIB_BIN_PATH/cil-ecma-cc ../newlib/configure \
    --prefix=${NEWLIB_PREFIX} \
    --target=cil-ecma \
    --enable-shared \
    --disable-libssp \
    --disable-nls \
    --disable-multilib \
    --with-sysroot=${NEWLIB_PREFIX}

make -j 16
#make
