// NativeLibrary.cpp : Defines the exported functions for the DLL application.
//

#include <math.h>

#if  defined(WIN32) || defined(_WINDLL)
#define EXPORTDLL __declspec(dllexport)
#else
#define EXPORTDLL
#endif

struct Vector3
{
	float X;
	float Y;
	float Z;
};

extern "C" EXPORTDLL void GetCurrentPositionNative(float time, Vector3* pPosition)
{
	pPosition->X = cos(-2*time);
	pPosition->Y = sin(-2*time);
	pPosition->Z = 0;
}

extern "C" EXPORTDLL void GetCurrentRotationNative(float time, Vector3* pRotation)
{
	pRotation->X = 0;
	pRotation->Y = -time;
	pRotation->Z = 0;
}

