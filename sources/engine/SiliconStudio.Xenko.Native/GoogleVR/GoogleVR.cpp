// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(ANDROID) || !defined(__clang__)

#include "../../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../../deps/NativePath/NativePath.h"

#define GVR_NO_CPP_WRAPPER
#include "../../../../deps/GoogleVR/vr/gvr/capi/include/gvr_types.h"
#include "../../../../deps/GoogleVR/vr/gvr/capi/include/gvr.h"

extern "C" {

	void* gGvrLibrary = NULL;

	typedef gvr_context* (*gvr_create_ptr)(void* env, void* app_context, void* class_loader);

	npBool xnGvrStartup()
	{
		if (gGvrLibrary) return true;

		gGvrLibrary = LoadDynamicLibrary("libgvr");
		if (!gGvrLibrary) return false;

		auto gvr_create_func = (gvr_create_ptr)GetSymbolAddress(gGvrLibrary, "gvr_create");
		if (!gvr_create_func) return false;

		return true;
	}
}

#else

#endif