#ifndef stdlib_h
#define stdlib_h

#include "../NativePath.h"
#include "../NativeMemory.h"

#ifdef __cplusplus
extern "C" {
#endif

//TODO more stdlib stuff

#undef exit
#define exit npExit

extern void npExit(int code);

#ifdef __cplusplus
}
#endif

#endif