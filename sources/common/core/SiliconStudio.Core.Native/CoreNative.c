// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

#include "../../../../deps/NativePath/NativeThreading.h"

#ifdef __cplusplus
extern "C" {
#endif

void cnSleep(int milliseconds)
{
    npThreadSleep(milliseconds);
}

#ifdef __cplusplus
}
#endif
