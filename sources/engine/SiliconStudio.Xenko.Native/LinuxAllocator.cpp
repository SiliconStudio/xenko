// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
