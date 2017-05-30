// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#ifndef _CoreNative_h_
#define _CoreNative_h_

/*
 * Some platforms requires a special declaration before the function declaration to export them 
 * in the shared library. Defining NEED_DLL_EXPORT will define DLL_EXPORT_API to do the right thing
 * for those platforms.
 *
 * To export void foo(int a), do:
 *
 *   DLL_EXPORT_API void foo (int a);
 */
#ifdef NEED_DLL_EXPORT
#define DLL_EXPORT_API __declspec(dllexport)
#else
#define DLL_EXPORT_API
#endif

typedef void(*CnPrintDebugFunc)(const char* string);

DLL_EXPORT_API CnPrintDebugFunc cnDebugPrintLine;

#endif

