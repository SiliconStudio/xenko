// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

// Provide implementation for new/delete operator for C++ code but only for our Linux
// implementation as other implementations seems to be able to get it otherwise

#if PLATFORM_LINUX

#include "../../../deps/NativePath/NativeMemory.h"


void* operator new(size_t sz) {
    return calloc(sz, 1);
}
void operator delete(void* ptr) noexcept
{
    free(ptr);
}

#endif
