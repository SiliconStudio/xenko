// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#include "../../../../../deps/NativePath/NativeThreading.h"
#include "CoreNative.h"

#ifdef __cplusplus
extern "C" {
#endif

	DLL_EXPORT_API void cnSleep(int milliseconds)
	{
		npThreadSleep(milliseconds);
	}

	DLL_EXPORT_API void cnSetup(void* printDebugPtr)
	{
		cnDebugPrintLine = (CnPrintDebugFunc)printDebugPtr;
	}

#ifdef __cplusplus
}
#endif
