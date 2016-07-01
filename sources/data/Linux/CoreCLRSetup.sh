#!/bin/sh

dotnet=`which dotnet`
dotnet=`readlink -f $dotnet`

is64=`file $dotnet | grep "64-bit"`

if [ -n "$is64" ]; then
    echo Copying 64-bit native libraries
    cp -f x64/*.so .
else
    echo Copying 32-bit native libraries
    cp -f x86/*.so
fi
