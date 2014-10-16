#pragma once

#ifdef EXPORT
	#ifdef _MSC_VER
		#define CORE_EXPORT(x) __declspec(dllexport) x
	#else
		#define CORE_EXPORT(x) extern x
	#endif
#else
	#define CORE_EXPORT(x) x
#endif
