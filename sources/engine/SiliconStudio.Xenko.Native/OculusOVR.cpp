// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/OculusOVR/Include/OVR_CAPI.h"
#include "../../../deps/NativePath/standard/stdio.h"

typedef ovrResult (*ovr_InitializePtr) (const ovrInitParams* params);
typedef void (*ovr_ShutdownPtr) ();
typedef void (*ovr_GetLastErrorInfoPtr) (ovrErrorInfo* errorInfo);
typedef const char* (*ovr_GetVersionStringPtr) ();
typedef int (*ovr_TraceMessagePtr) (int level, const char* message);
typedef ovrHmdDesc (*ovr_GetHmdDescPtr) (ovrSession session);
typedef unsigned int (*ovr_GetTrackerCountPtr) (ovrSession session);
typedef ovrTrackerDesc (*ovr_GetTrackerDescPtr) (ovrSession session, unsigned int trackerDescIndex);
typedef ovrResult (*ovr_CreatePtr) (ovrSession* pSession, ovrGraphicsLuid* pLuid);
typedef void (*ovr_DestroyPtr) (ovrSession session);
typedef ovrResult (*ovr_GetSessionStatusPtr) (ovrSession session, ovrSessionStatus* sessionStatus);
typedef ovrResult (*ovr_SetTrackingOriginTypePtr) (ovrSession session, ovrTrackingOrigin origin);
typedef ovrTrackingOrigin (*ovr_GetTrackingOriginTypePtr) (ovrSession session);
typedef ovrResult (*ovr_RecenterTrackingOriginPtr) (ovrSession session);
typedef void (*ovr_ClearShouldRecenterFlagPtr) (ovrSession session);
typedef ovrTrackingState (*ovr_GetTrackingStatePtr) (ovrSession session, double absTime, ovrBool latencyMarker);
typedef ovrTrackerPose (*ovr_GetTrackerPosePtr) (ovrSession session, unsigned int trackerPoseIndex);
typedef ovrResult (*ovr_GetInputStatePtr) (ovrSession session, ovrControllerType controllerType, ovrInputState* inputState);
typedef unsigned int (*ovr_GetConnectedControllerTypesPtr) (ovrSession session);
typedef ovrResult (*ovr_SetControllerVibrationPtr) (ovrSession session, ovrControllerType controllerType, float frequency, float amplitude);
typedef ovrResult (*ovr_GetTextureSwapChainLengthPtr) (ovrSession session, ovrTextureSwapChain chain, int* out_Length);
typedef ovrResult (*ovr_GetTextureSwapChainCurrentIndexPtr) (ovrSession session, ovrTextureSwapChain chain, int* out_Index);
typedef ovrResult (*ovr_GetTextureSwapChainDescPtr) (ovrSession session, ovrTextureSwapChain chain, ovrTextureSwapChainDesc* out_Desc);
typedef ovrResult (*ovr_CommitTextureSwapChainPtr) (ovrSession session, ovrTextureSwapChain chain);
typedef void (*ovr_DestroyTextureSwapChainPtr) (ovrSession session, ovrTextureSwapChain chain);
typedef void (*ovr_DestroyMirrorTexturePtr) (ovrSession session, ovrMirrorTexture mirrorTexture);
typedef ovrSizei (*ovr_GetFovTextureSizePtr) (ovrSession session, ovrEyeType eye, ovrFovPort fov, float pixelsPerDisplayPixel);
typedef ovrEyeRenderDesc (*ovr_GetRenderDescPtr) (ovrSession session, ovrEyeType eyeType, ovrFovPort fov);
typedef ovrResult (*ovr_SubmitFramePtr) (ovrSession session, long long frameIndex, const ovrViewScaleDesc* viewScaleDesc, ovrLayerHeader const * const * layerPtrList, unsigned int layerCount);
typedef double (*ovr_GetPredictedDisplayTimePtr) (ovrSession session, long long frameIndex);
typedef double (*ovr_GetTimeInSecondsPtr) ();
typedef ovrBool (*ovr_GetBoolPtr) (ovrSession session, const char* propertyName, ovrBool defaultVal);
typedef ovrBool (*ovr_SetBoolPtr) (ovrSession session, const char* propertyName, ovrBool value);
typedef int (*ovr_GetIntPtr) (ovrSession session, const char* propertyName, int defaultVal);
typedef ovrBool (*ovr_SetIntPtr) (ovrSession session, const char* propertyName, int value);
typedef float (*ovr_GetFloatPtr) (ovrSession session, const char* propertyName, float defaultVal);
typedef ovrBool (*ovr_SetFloatPtr) (ovrSession session, const char* propertyName, float value);
typedef unsigned int (*ovr_GetFloatArrayPtr) (ovrSession session, const char* propertyName, float values[], unsigned int valuesCapacity);
typedef ovrBool (*ovr_SetFloatArrayPtr) (ovrSession session, const char* propertyName, const float values[], unsigned int valuesSize);
typedef const char* (*ovr_GetStringPtr) (ovrSession session, const char* propertyName, const char* defaultVal);
typedef ovrBool (*ovr_SetStringPtr) (ovrSession session, const char* propertyName, const char* value);

extern "C" {

	void* __libOvr = nullptr;
	
	ovr_InitializePtr ovr_InitializeFunc = nullptr;
	ovr_ShutdownPtr ovr_ShutdownFunc = nullptr;
	ovr_GetLastErrorInfoPtr ovr_GetLastErrorInfoFunc = nullptr;
	ovr_GetVersionStringPtr ovr_GetVersionStringFunc = nullptr;
	ovr_TraceMessagePtr ovr_TraceMessageFunc = nullptr;
	ovr_GetHmdDescPtr ovr_GetHmdDescFunc = nullptr;
	ovr_GetTrackerCountPtr ovr_GetTrackerCountFunc = nullptr;
	ovr_GetTrackerDescPtr ovr_GetTrackerDescFunc = nullptr;
	ovr_CreatePtr ovr_CreateFunc = nullptr;
	ovr_DestroyPtr ovr_DestroyFunc = nullptr;
	ovr_GetSessionStatusPtr ovr_GetSessionStatusFunc = nullptr;
	ovr_SetTrackingOriginTypePtr ovr_SetTrackingOriginTypeFunc = nullptr;
	ovr_GetTrackingOriginTypePtr ovr_GetTrackingOriginTypeFunc = nullptr;
	ovr_RecenterTrackingOriginPtr ovr_RecenterTrackingOriginFunc = nullptr;
	ovr_ClearShouldRecenterFlagPtr ovr_ClearShouldRecenterFlagFunc = nullptr;
	ovr_GetTrackingStatePtr ovr_GetTrackingStateFunc = nullptr;
	ovr_GetTrackerPosePtr ovr_GetTrackerPoseFunc = nullptr;
	ovr_GetInputStatePtr ovr_GetInputStateFunc = nullptr;
	ovr_GetConnectedControllerTypesPtr ovr_GetConnectedControllerTypesFunc = nullptr;
	ovr_SetControllerVibrationPtr ovr_SetControllerVibrationFunc = nullptr;
	ovr_GetTextureSwapChainLengthPtr ovr_GetTextureSwapChainLengthFunc = nullptr;
	ovr_GetTextureSwapChainCurrentIndexPtr ovr_GetTextureSwapChainCurrentIndexFunc = nullptr;
	ovr_GetTextureSwapChainDescPtr ovr_GetTextureSwapChainDescFunc = nullptr;
	ovr_CommitTextureSwapChainPtr ovr_CommitTextureSwapChainFunc = nullptr;
	ovr_DestroyTextureSwapChainPtr ovr_DestroyTextureSwapChainFunc = nullptr;
	ovr_DestroyMirrorTexturePtr ovr_DestroyMirrorTextureFunc = nullptr;
	ovr_GetFovTextureSizePtr ovr_GetFovTextureSizeFunc = nullptr;
	ovr_GetRenderDescPtr ovr_GetRenderDescFunc = nullptr;
	ovr_SubmitFramePtr ovr_SubmitFrameFunc = nullptr;
	ovr_GetPredictedDisplayTimePtr ovr_GetPredictedDisplayTimeFunc = nullptr;
	ovr_GetTimeInSecondsPtr ovr_GetTimeInSecondsFunc = nullptr;
	ovr_GetBoolPtr ovr_GetBoolFunc = nullptr;
	ovr_SetBoolPtr ovr_SetBoolFunc = nullptr;
	ovr_GetIntPtr ovr_GetIntFunc = nullptr;
	ovr_SetIntPtr ovr_SetIntFunc = nullptr;
	ovr_GetFloatPtr ovr_GetFloatFunc = nullptr;
	ovr_SetFloatPtr ovr_SetFloatFunc = nullptr;
	ovr_GetFloatArrayPtr ovr_GetFloatArrayFunc = nullptr;
	ovr_SetFloatArrayPtr ovr_SetFloatArrayFunc = nullptr;
	ovr_GetStringPtr ovr_GetStringFunc = nullptr;
	ovr_SetStringPtr ovr_SetStringFunc = nullptr;

	bool XenkoOvrStartup()
	{
		if(!__libOvr)
		{
			__libOvr = LoadDynamicLibrary("LibOVR");
			if (!__libOvr) __libOvr = LoadDynamicLibrary("x86\\LibOVR");
			if (!__libOvr) __libOvr = LoadDynamicLibrary("x64\\LibOVR");
			if (!__libOvr) __libOvr = LoadDynamicLibrary("x64/LibOVR");
			if (!__libOvr) __libOvr = LoadDynamicLibrary("x64/LibOVR");
			if (!__libOvr)
			{
				return false;
			}

			ovr_InitializeFunc = (ovr_InitializePtr)GetSymbolAddress(__libOvr, "ovr_Initialize");
			if (!ovr_InitializeFunc) { printf("Failed to get ovr_Initialize\n"); return false; }
			ovr_ShutdownFunc = (ovr_ShutdownPtr)GetSymbolAddress(__libOvr, "ovr_Shutdown");
			if (!ovr_ShutdownFunc) { printf("Failed to get ovr_Shutdown\n"); return false; }
			ovr_GetLastErrorInfoFunc = (ovr_GetLastErrorInfoPtr)GetSymbolAddress(__libOvr, "ovr_GetLastErrorInfo");
			if (!ovr_GetLastErrorInfoFunc) { printf("Failed to get ovr_GetLastErrorInfo\n"); return false; }
			ovr_GetVersionStringFunc = (ovr_GetVersionStringPtr)GetSymbolAddress(__libOvr, "ovr_GetVersionString");
			if (!ovr_GetVersionStringFunc) { printf("Failed to get ovr_GetVersionString\n"); return false; }
			ovr_TraceMessageFunc = (ovr_TraceMessagePtr)GetSymbolAddress(__libOvr, "ovr_TraceMessage");
			if (!ovr_TraceMessageFunc) { printf("Failed to get ovr_TraceMessage\n"); return false; }
			ovr_GetHmdDescFunc = (ovr_GetHmdDescPtr)GetSymbolAddress(__libOvr, "ovr_GetHmdDesc");
			if (!ovr_GetHmdDescFunc) { printf("Failed to get ovr_GetHmdDesc\n"); return false; }
			ovr_GetTrackerCountFunc = (ovr_GetTrackerCountPtr)GetSymbolAddress(__libOvr, "ovr_GetTrackerCount");
			if (!ovr_GetTrackerCountFunc) { printf("Failed to get ovr_GetTrackerCount\n"); return false; }
			ovr_GetTrackerDescFunc = (ovr_GetTrackerDescPtr)GetSymbolAddress(__libOvr, "ovr_GetTrackerDesc");
			if (!ovr_GetTrackerDescFunc) { printf("Failed to get ovr_GetTrackerDesc\n"); return false; }
			ovr_CreateFunc = (ovr_CreatePtr)GetSymbolAddress(__libOvr, "ovr_Create");
			if (!ovr_CreateFunc) { printf("Failed to get ovr_Create\n"); return false; }
			ovr_DestroyFunc = (ovr_DestroyPtr)GetSymbolAddress(__libOvr, "ovr_Destroy");
			if (!ovr_DestroyFunc) { printf("Failed to get ovr_Destroy\n"); return false; }
			ovr_GetSessionStatusFunc = (ovr_GetSessionStatusPtr)GetSymbolAddress(__libOvr, "ovr_GetSessionStatus");
			if (!ovr_GetSessionStatusFunc) { printf("Failed to get ovr_GetSessionStatus\n"); return false; }
			ovr_SetTrackingOriginTypeFunc = (ovr_SetTrackingOriginTypePtr)GetSymbolAddress(__libOvr, "ovr_SetTrackingOriginType");
			if (!ovr_SetTrackingOriginTypeFunc) { printf("Failed to get ovr_SetTrackingOriginType\n"); return false; }
			ovr_GetTrackingOriginTypeFunc = (ovr_GetTrackingOriginTypePtr)GetSymbolAddress(__libOvr, "ovr_GetTrackingOriginType");
			if (!ovr_GetTrackingOriginTypeFunc) { printf("Failed to get ovr_GetTrackingOriginType\n"); return false; }
			ovr_RecenterTrackingOriginFunc = (ovr_RecenterTrackingOriginPtr)GetSymbolAddress(__libOvr, "ovr_RecenterTrackingOrigin");
			if (!ovr_RecenterTrackingOriginFunc) { printf("Failed to get ovr_RecenterTrackingOrigin\n"); return false; }
			ovr_ClearShouldRecenterFlagFunc = (ovr_ClearShouldRecenterFlagPtr)GetSymbolAddress(__libOvr, "ovr_ClearShouldRecenterFlag");
			if (!ovr_ClearShouldRecenterFlagFunc) { printf("Failed to get ovr_ClearShouldRecenterFlag\n"); return false; }
			ovr_GetTrackingStateFunc = (ovr_GetTrackingStatePtr)GetSymbolAddress(__libOvr, "ovr_GetTrackingState");
			if (!ovr_GetTrackingStateFunc) { printf("Failed to get ovr_GetTrackingState\n"); return false; }
			ovr_GetTrackerPoseFunc = (ovr_GetTrackerPosePtr)GetSymbolAddress(__libOvr, "ovr_GetTrackerPose");
			if (!ovr_GetTrackerPoseFunc) { printf("Failed to get ovr_GetTrackerPose\n"); return false; }
			ovr_GetInputStateFunc = (ovr_GetInputStatePtr)GetSymbolAddress(__libOvr, "ovr_GetInputState");
			if (!ovr_GetInputStateFunc) { printf("Failed to get ovr_GetInputState\n"); return false; }
			ovr_GetConnectedControllerTypesFunc = (ovr_GetConnectedControllerTypesPtr)GetSymbolAddress(__libOvr, "ovr_GetConnectedControllerTypes");
			if (!ovr_GetConnectedControllerTypesFunc) { printf("Failed to get ovr_GetConnectedControllerTypes\n"); return false; }
			ovr_SetControllerVibrationFunc = (ovr_SetControllerVibrationPtr)GetSymbolAddress(__libOvr, "ovr_SetControllerVibration");
			if (!ovr_SetControllerVibrationFunc) { printf("Failed to get ovr_SetControllerVibration\n"); return false; }
			ovr_GetTextureSwapChainLengthFunc = (ovr_GetTextureSwapChainLengthPtr)GetSymbolAddress(__libOvr, "ovr_GetTextureSwapChainLength");
			if (!ovr_GetTextureSwapChainLengthFunc) { printf("Failed to get ovr_GetTextureSwapChainLength\n"); return false; }
			ovr_GetTextureSwapChainCurrentIndexFunc = (ovr_GetTextureSwapChainCurrentIndexPtr)GetSymbolAddress(__libOvr, "ovr_GetTextureSwapChainCurrentIndex");
			if (!ovr_GetTextureSwapChainCurrentIndexFunc) { printf("Failed to get ovr_GetTextureSwapChainCurrentIndex\n"); return false; }
			ovr_GetTextureSwapChainDescFunc = (ovr_GetTextureSwapChainDescPtr)GetSymbolAddress(__libOvr, "ovr_GetTextureSwapChainDesc");
			if (!ovr_GetTextureSwapChainDescFunc) { printf("Failed to get ovr_GetTextureSwapChainDesc\n"); return false; }
			ovr_CommitTextureSwapChainFunc = (ovr_CommitTextureSwapChainPtr)GetSymbolAddress(__libOvr, "ovr_CommitTextureSwapChain");
			if (!ovr_CommitTextureSwapChainFunc) { printf("Failed to get ovr_CommitTextureSwapChain\n"); return false; }
			ovr_DestroyTextureSwapChainFunc = (ovr_DestroyTextureSwapChainPtr)GetSymbolAddress(__libOvr, "ovr_DestroyTextureSwapChain");
			if (!ovr_DestroyTextureSwapChainFunc) { printf("Failed to get ovr_DestroyTextureSwapChain\n"); return false; }
			ovr_DestroyMirrorTextureFunc = (ovr_DestroyMirrorTexturePtr)GetSymbolAddress(__libOvr, "ovr_DestroyMirrorTexture");
			if (!ovr_DestroyMirrorTextureFunc) { printf("Failed to get ovr_DestroyMirrorTexture\n"); return false; }
			ovr_GetFovTextureSizeFunc = (ovr_GetFovTextureSizePtr)GetSymbolAddress(__libOvr, "ovr_GetFovTextureSize");
			if (!ovr_GetFovTextureSizeFunc) { printf("Failed to get ovr_GetFovTextureSize\n"); return false; }
			ovr_GetRenderDescFunc = (ovr_GetRenderDescPtr)GetSymbolAddress(__libOvr, "ovr_GetRenderDesc");
			if (!ovr_GetRenderDescFunc) { printf("Failed to get ovr_GetRenderDesc\n"); return false; }
			ovr_SubmitFrameFunc = (ovr_SubmitFramePtr)GetSymbolAddress(__libOvr, "ovr_SubmitFrame");
			if (!ovr_SubmitFrameFunc) { printf("Failed to get ovr_SubmitFrame\n"); return false; }
			ovr_GetPredictedDisplayTimeFunc = (ovr_GetPredictedDisplayTimePtr)GetSymbolAddress(__libOvr, "ovr_GetPredictedDisplayTime");
			if (!ovr_GetPredictedDisplayTimeFunc) { printf("Failed to get ovr_GetPredictedDisplayTime\n"); return false; }
			ovr_GetTimeInSecondsFunc = (ovr_GetTimeInSecondsPtr)GetSymbolAddress(__libOvr, "ovr_GetTimeInSeconds");
			if (!ovr_GetTimeInSecondsFunc) { printf("Failed to get ovr_GetTimeInSeconds\n"); return false; }
			ovr_GetBoolFunc = (ovr_GetBoolPtr)GetSymbolAddress(__libOvr, "ovr_GetBool");
			if (!ovr_GetBoolFunc) { printf("Failed to get ovr_GetBool\n"); return false; }
			ovr_SetBoolFunc = (ovr_SetBoolPtr)GetSymbolAddress(__libOvr, "ovr_SetBool");
			if (!ovr_SetBoolFunc) { printf("Failed to get ovr_SetBool\n"); return false; }
			ovr_GetIntFunc = (ovr_GetIntPtr)GetSymbolAddress(__libOvr, "ovr_GetInt");
			if (!ovr_GetIntFunc) { printf("Failed to get ovr_GetInt\n"); return false; }
			ovr_SetIntFunc = (ovr_SetIntPtr)GetSymbolAddress(__libOvr, "ovr_SetInt");
			if (!ovr_SetIntFunc) { printf("Failed to get ovr_SetInt\n"); return false; }
			ovr_GetFloatFunc = (ovr_GetFloatPtr)GetSymbolAddress(__libOvr, "ovr_GetFloat");
			if (!ovr_GetFloatFunc) { printf("Failed to get ovr_GetFloat\n"); return false; }
			ovr_SetFloatFunc = (ovr_SetFloatPtr)GetSymbolAddress(__libOvr, "ovr_SetFloat");
			if (!ovr_SetFloatFunc) { printf("Failed to get ovr_SetFloat\n"); return false; }
			ovr_GetFloatArrayFunc = (ovr_GetFloatArrayPtr)GetSymbolAddress(__libOvr, "ovr_GetFloatArray");
			if (!ovr_GetFloatArrayFunc) { printf("Failed to get ovr_GetFloatArray\n"); return false; }
			ovr_SetFloatArrayFunc = (ovr_SetFloatArrayPtr)GetSymbolAddress(__libOvr, "ovr_SetFloatArray");
			if (!ovr_SetFloatArrayFunc) { printf("Failed to get ovr_SetFloatArray\n"); return false; }
			ovr_GetStringFunc = (ovr_GetStringPtr)GetSymbolAddress(__libOvr, "ovr_GetString");
			if (!ovr_GetStringFunc) { printf("Failed to get ovr_GetString\n"); return false; }
			ovr_SetStringFunc = (ovr_SetStringPtr)GetSymbolAddress(__libOvr, "ovr_SetString");
			if (!ovr_SetStringFunc) { printf("Failed to get ovr_SetString\n"); return false; }
		}

		ovrResult result = ovr_InitializeFunc(nullptr);

		return OVR_SUCCESS(result);
	}

	void XenkoOvrShutdown()
	{
		if (!__libOvr) return;

		ovr_ShutdownFunc();

		FreeDynamicLibrary(__libOvr);
		__libOvr = nullptr;
	}

	int XenkoOvrGetError(char* errorString)
	{
		ovrErrorInfo errInfo;
		ovr_GetLastErrorInfoFunc(&errInfo);
		strcpy(errorString, errInfo.ErrorString);
		return errInfo.Result;
	}

	bool XenkoOvrCreateSession(ovrSession sessionOut, char* luidOut)
	{
		ovrGraphicsLuid luid;
		ovrResult result = ovr_CreateFunc(&sessionOut, &luid);

		bool success = OVR_SUCCESS(result);
		if(success)
		{
			sprintf(luidOut, "%l", *((int64_t*)luid.Reserved));
			return true;
		}

		return false;
	}

	void XenkoOvrDestroySession(ovrSession session)
	{
		ovr_DestroyFunc(session);
	}

}