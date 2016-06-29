// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(WINDOWS_DESKTOP) || !defined(__clang__)

#include "../../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../../deps/NativePath/NativePath.h"

#ifndef __clang__
//Make resharper work!
#define OVR_ALIGNAS(n)
#error "The compiler must be clang!"
#endif

#include "../../../../deps/OculusOVR/Include/OVR_CAPI.h"

typedef struct _GUID {
	unsigned long  Data1;
	unsigned short Data2;
	unsigned short Data3;
	unsigned char  Data4[8];
} GUID;

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

typedef ovrResult (*ovr_CreateTextureSwapChainDXPtr)(ovrSession session, void* d3dPtr, const ovrTextureSwapChainDesc* desc, ovrTextureSwapChain* out_TextureSwapChain);
typedef ovrResult (*ovr_GetTextureSwapChainBufferDXPtr)(ovrSession session, ovrTextureSwapChain chain, int index, GUID iid, void** out_Buffer);
typedef ovrResult (*ovr_CreateMirrorTextureDXPtr)(ovrSession session, void* d3dPtr,	const ovrMirrorTextureDesc* desc, ovrMirrorTexture* out_MirrorTexture);
typedef ovrResult (*ovr_GetMirrorTextureBufferDXPtr)(ovrSession session, ovrMirrorTexture mirrorTexture, GUID iid, void** out_Buffer);

typedef void (*ovr_CalcEyePosesPtr)(ovrPosef headPose, const ovrVector3f hmdToEyeOffset[2], ovrPosef outEyePoses[2]);
typedef ovrMatrix4f (*ovrMatrix4f_ProjectionPtr)(ovrFovPort fov, float znear, float zfar, unsigned int projectionModFlags);

extern "C" {

	void* __libOvr = NULL;
	
	ovr_InitializePtr ovr_InitializeFunc = NULL;
	ovr_ShutdownPtr ovr_ShutdownFunc = NULL;
	ovr_GetLastErrorInfoPtr ovr_GetLastErrorInfoFunc = NULL;
	ovr_GetVersionStringPtr ovr_GetVersionStringFunc = NULL;
	ovr_TraceMessagePtr ovr_TraceMessageFunc = NULL;
	ovr_GetHmdDescPtr ovr_GetHmdDescFunc = NULL;
	ovr_GetTrackerCountPtr ovr_GetTrackerCountFunc = NULL;
	ovr_GetTrackerDescPtr ovr_GetTrackerDescFunc = NULL;
	ovr_CreatePtr ovr_CreateFunc = NULL;
	ovr_DestroyPtr ovr_DestroyFunc = NULL;
	ovr_GetSessionStatusPtr ovr_GetSessionStatusFunc = NULL;
	ovr_SetTrackingOriginTypePtr ovr_SetTrackingOriginTypeFunc = NULL;
	ovr_GetTrackingOriginTypePtr ovr_GetTrackingOriginTypeFunc = NULL;
	ovr_RecenterTrackingOriginPtr ovr_RecenterTrackingOriginFunc = NULL;
	ovr_ClearShouldRecenterFlagPtr ovr_ClearShouldRecenterFlagFunc = NULL;
	ovr_GetTrackingStatePtr ovr_GetTrackingStateFunc = NULL;
	ovr_GetTrackerPosePtr ovr_GetTrackerPoseFunc = NULL;
	ovr_GetInputStatePtr ovr_GetInputStateFunc = NULL;
	ovr_GetConnectedControllerTypesPtr ovr_GetConnectedControllerTypesFunc = NULL;
	ovr_SetControllerVibrationPtr ovr_SetControllerVibrationFunc = NULL;
	ovr_GetTextureSwapChainLengthPtr ovr_GetTextureSwapChainLengthFunc = NULL;
	ovr_GetTextureSwapChainCurrentIndexPtr ovr_GetTextureSwapChainCurrentIndexFunc = NULL;
	ovr_GetTextureSwapChainDescPtr ovr_GetTextureSwapChainDescFunc = NULL;
	ovr_CommitTextureSwapChainPtr ovr_CommitTextureSwapChainFunc = NULL;
	ovr_DestroyTextureSwapChainPtr ovr_DestroyTextureSwapChainFunc = NULL;
	ovr_DestroyMirrorTexturePtr ovr_DestroyMirrorTextureFunc = NULL;
	ovr_GetFovTextureSizePtr ovr_GetFovTextureSizeFunc = NULL;
	ovr_GetRenderDescPtr ovr_GetRenderDescFunc = NULL;
	ovr_SubmitFramePtr ovr_SubmitFrameFunc = NULL;
	ovr_GetPredictedDisplayTimePtr ovr_GetPredictedDisplayTimeFunc = NULL;
	ovr_GetTimeInSecondsPtr ovr_GetTimeInSecondsFunc = NULL;
	ovr_GetBoolPtr ovr_GetBoolFunc = NULL;
	ovr_SetBoolPtr ovr_SetBoolFunc = NULL;
	ovr_GetIntPtr ovr_GetIntFunc = NULL;
	ovr_SetIntPtr ovr_SetIntFunc = NULL;
	ovr_GetFloatPtr ovr_GetFloatFunc = NULL;
	ovr_SetFloatPtr ovr_SetFloatFunc = NULL;
	ovr_GetFloatArrayPtr ovr_GetFloatArrayFunc = NULL;
	ovr_SetFloatArrayPtr ovr_SetFloatArrayFunc = NULL;
	ovr_GetStringPtr ovr_GetStringFunc = NULL;
	ovr_SetStringPtr ovr_SetStringFunc = NULL;

	ovr_CreateTextureSwapChainDXPtr ovr_CreateTextureSwapChainDXFunc = NULL;
	ovr_GetTextureSwapChainBufferDXPtr ovr_GetTextureSwapChainBufferDXFunc = NULL;
	ovr_CreateMirrorTextureDXPtr ovr_CreateMirrorTextureDXFunc = NULL;
	ovr_GetMirrorTextureBufferDXPtr ovr_GetMirrorTextureBufferDXFunc = NULL;

	ovr_CalcEyePosesPtr ovr_CalcEyePosesFunc = NULL;
	ovrMatrix4f_ProjectionPtr ovrMatrix4f_ProjectionFunc = NULL;

	bool xnOvrStartup()
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

			ovr_CreateTextureSwapChainDXFunc = (ovr_CreateTextureSwapChainDXPtr)GetSymbolAddress(__libOvr, "ovr_CreateTextureSwapChainDX");
			if (!ovr_CreateTextureSwapChainDXFunc) { printf("Failed to get ovr_CreateTextureSwapChainDX\n"); return false; }
			ovr_GetTextureSwapChainBufferDXFunc = (ovr_GetTextureSwapChainBufferDXPtr)GetSymbolAddress(__libOvr, "ovr_GetTextureSwapChainBufferDX");
			if (!ovr_GetTextureSwapChainBufferDXFunc) { printf("Failed to get ovr_GetTextureSwapChainBufferDX\n"); return false; }
			ovr_CreateMirrorTextureDXFunc = (ovr_CreateMirrorTextureDXPtr)GetSymbolAddress(__libOvr, "ovr_CreateMirrorTextureDX");
			if (!ovr_CreateMirrorTextureDXFunc) { printf("Failed to get ovr_CreateMirrorTextureDX\n"); return false; }
			ovr_GetMirrorTextureBufferDXFunc = (ovr_GetMirrorTextureBufferDXPtr)GetSymbolAddress(__libOvr, "ovr_GetMirrorTextureBufferDX");
			if (!ovr_GetMirrorTextureBufferDXFunc) { printf("Failed to get ovr_GetMirrorTextureBufferDX\n"); return false; }

			ovr_CalcEyePosesFunc = (ovr_CalcEyePosesPtr)GetSymbolAddress(__libOvr, "ovr_CalcEyePoses");
			if (!ovr_CalcEyePosesFunc) { printf("Failed to get ovr_CalcEyePoses\n"); return false; }
			ovrMatrix4f_ProjectionFunc = (ovrMatrix4f_ProjectionPtr)GetSymbolAddress(__libOvr, "ovrMatrix4f_Projection");
			if (!ovrMatrix4f_ProjectionFunc) { printf("Failed to get ovrMatrix4f_Projection\n"); return false; }
		}

		ovrResult result = ovr_InitializeFunc(NULL);

		return OVR_SUCCESS(result);
	}

	void xnOvrShutdown()
	{
		if (!__libOvr) return;

		ovr_ShutdownFunc();

		FreeDynamicLibrary(__libOvr);
		__libOvr = NULL;
	}

	int xnOvrGetError(char* errorString)
	{
		ovrErrorInfo errInfo;
		ovr_GetLastErrorInfoFunc(&errInfo);
		strcpy(errorString, errInfo.ErrorString);
		return errInfo.Result;
	}

	struct xnOvrSession
	{
		ovrSession Session;
		ovrTextureSwapChain SwapChain;
		ovrMirrorTexture Mirror;
		ovrEyeRenderDesc EyeRenderDesc[2];
		ovrVector3f HmdToEyeViewOffset[2];
		ovrLayerEyeFov Layer;
		ovrHmdDesc HmdDesc;
	};

	xnOvrSession* xnOvrCreateSessionDx(int64_t* luidOut)
	{
		ovrSession session;
		ovrGraphicsLuid luid;
		ovrResult result = ovr_CreateFunc(&session, &luid);

		bool success = OVR_SUCCESS(result);
		if(success)
		{
			auto sessionOut = new xnOvrSession();
			sessionOut->Session = session;
			sessionOut->SwapChain = NULL;

			*luidOut = *((int64_t*)luid.Reserved);
			return sessionOut;
		}

		return NULL;
	}

	void xnOvrDestroySession(xnOvrSession* session)
	{
		ovr_DestroyFunc(session->Session);
	}

	bool xnOvrCreateTexturesDx(xnOvrSession* session, void* dxDevice, int* outTextureCount, int backBufferWidth, int backBufferHeight)
	{
		session->HmdDesc = ovr_GetHmdDescFunc(session->Session);
		ovrSizei sizel = ovr_GetFovTextureSizeFunc(session->Session, ovrEye_Left, session->HmdDesc.DefaultEyeFov[0], 1.0f);
		ovrSizei sizer = ovr_GetFovTextureSizeFunc(session->Session, ovrEye_Right, session->HmdDesc.DefaultEyeFov[1], 1.0f);
		ovrSizei bufferSize;
		bufferSize.w = sizel.w + sizer.w;
		bufferSize.h = fmax(sizel.h, sizer.h);

		ovrTextureSwapChainDesc texDesc = {};
		texDesc.Type = ovrTexture_2D;
		texDesc.Format = OVR_FORMAT_R8G8B8A8_UNORM_SRGB;
		texDesc.ArraySize = 1;
		texDesc.Width = bufferSize.w;
		texDesc.Height = bufferSize.h;
		texDesc.MipLevels = 1;
		texDesc.SampleCount = 1;
		texDesc.StaticImage = ovrFalse;
		texDesc.MiscFlags = ovrTextureMisc_None;
		texDesc.BindFlags = ovrTextureBind_DX_RenderTarget;

		if(!OVR_SUCCESS(ovr_CreateTextureSwapChainDXFunc(session->Session, dxDevice, &texDesc, &session->SwapChain)))
		{
			return false;
		}

		auto count = 0;
		ovr_GetTextureSwapChainLengthFunc(session->Session, session->SwapChain, &count);
		*outTextureCount = count;
		
		//init structures
		session->EyeRenderDesc[0] = ovr_GetRenderDescFunc(session->Session, ovrEye_Left, session->HmdDesc.DefaultEyeFov[0]);
		session->EyeRenderDesc[1] = ovr_GetRenderDescFunc(session->Session, ovrEye_Right, session->HmdDesc.DefaultEyeFov[1]);
		session->HmdToEyeViewOffset[0] = session->EyeRenderDesc[0].HmdToEyeOffset;
		session->HmdToEyeViewOffset[1] = session->EyeRenderDesc[1].HmdToEyeOffset;

		session->Layer.Header.Type = ovrLayerType_EyeFov;
		session->Layer.Header.Flags = 0;
		session->Layer.ColorTexture[0] = session->SwapChain;
		session->Layer.ColorTexture[1] = session->SwapChain;
		session->Layer.Fov[0] = session->EyeRenderDesc[0].Fov;
		session->Layer.Fov[1] = session->EyeRenderDesc[1].Fov;
		session->Layer.Viewport[0].Pos.x = 0;
		session->Layer.Viewport[0].Pos.y = 0;
		session->Layer.Viewport[0].Size.w = bufferSize.w / 2;
		session->Layer.Viewport[0].Size.h = bufferSize.h;
		session->Layer.Viewport[1].Pos.x = bufferSize.w / 2;
		session->Layer.Viewport[1].Pos.y = 0;
		session->Layer.Viewport[1].Size.w = bufferSize.w / 2;
		session->Layer.Viewport[1].Size.h = bufferSize.h;

		//create mirror as well
		ovrMirrorTextureDesc mirrorDesc = {};
		mirrorDesc.Format = OVR_FORMAT_R8G8B8A8_UNORM_SRGB;
		mirrorDesc.Width = backBufferWidth;
		mirrorDesc.Height = backBufferHeight;
		if (!OVR_SUCCESS(ovr_CreateMirrorTextureDXFunc(session->Session, dxDevice, &mirrorDesc, &session->Mirror)))
		{
			return false;
		}
		
		return true;
	}

	void* xnOvrGetTextureAtIndexDx(xnOvrSession* session, GUID textureGuid, int index)
	{
		void* texture = NULL;
		if (!OVR_SUCCESS(ovr_GetTextureSwapChainBufferDXFunc(session->Session, session->SwapChain, index, textureGuid, &texture)))
		{
			return NULL;
		}
		return texture;
	}

	void* xnOvrGetMirrorTextureDx(xnOvrSession* session, GUID textureGuid)
	{
		void* texture = NULL;
		if (!OVR_SUCCESS(ovr_GetMirrorTextureBufferDXFunc(session->Session, session->Mirror, textureGuid, &texture)))
		{
			return NULL;
		}
		return texture;
	}

	int xnOvrGetCurrentTargetIndex(xnOvrSession* session)
	{
		int index;
		ovr_GetTextureSwapChainCurrentIndexFunc(session->Session, session->SwapChain, &index);
		return index;
	}

	void xnOvrPrepareRender(xnOvrSession* session, 
		float near, float far, 
		float* projLeft, float* projRight, 
		float* positionLeft, float* positionRight, 
		float* rotationLeft, float* rotationRight)
	{
		session->EyeRenderDesc[0] = ovr_GetRenderDescFunc(session->Session, ovrEye_Left, session->HmdDesc.DefaultEyeFov[0]);
		session->EyeRenderDesc[1] = ovr_GetRenderDescFunc(session->Session, ovrEye_Right, session->HmdDesc.DefaultEyeFov[1]);
		session->HmdToEyeViewOffset[0] = session->EyeRenderDesc[0].HmdToEyeOffset;
		session->HmdToEyeViewOffset[1] = session->EyeRenderDesc[1].HmdToEyeOffset;

		session->Layer.SensorSampleTime = ovr_GetPredictedDisplayTimeFunc(session->Session, 0);
		auto hmdState = ovr_GetTrackingStateFunc(session->Session, session->Layer.SensorSampleTime, ovrTrue);
		ovr_CalcEyePosesFunc(hmdState.HeadPose.ThePose, session->HmdToEyeViewOffset, session->Layer.RenderPose);

		auto leftProj = ovrMatrix4f_ProjectionFunc(session->Layer.Fov[0], near, far, 0);
		auto rightProj = ovrMatrix4f_ProjectionFunc(session->Layer.Fov[1], near, far, 0);

		memcpy(projLeft, &leftProj, sizeof(float) * 16);
		memcpy(positionLeft, &session->Layer.RenderPose[0].Position, sizeof(float) * 3);
		memcpy(rotationLeft, &session->Layer.RenderPose[0].Orientation, sizeof(float) * 4);
		
		memcpy(projRight, &rightProj, sizeof(float) * 16);
		memcpy(positionRight, &session->Layer.RenderPose[1].Position, sizeof(float) * 3);
		memcpy(rotationRight, &session->Layer.RenderPose[1].Orientation, sizeof(float) * 4);
	}

	bool xnOvrCommitFrame(xnOvrSession* session)
	{
		ovr_CommitTextureSwapChainFunc(session->Session, session->SwapChain);
		auto layers = &session->Layer.Header;
		if(OVR_SUCCESS(ovr_SubmitFrameFunc(session->Session, 0, NULL, &layers, 1)))
		{
			return true;
		}

		return false;
	}

}

#else

extern "C" {
	typedef struct _GUID {
		unsigned long  Data1;
		unsigned short Data2;
		unsigned short Data3;
		unsigned char  Data4[8];
	} GUID;

	bool xnOvrStartup()
	{
		return true;
	}

	void xnOvrShutdown()
	{
		
	}

	int xnOvrGetError(char* errorString)
	{
		return 0;
	}

	void* xnOvrCreateSessionDx(void* luidOut)
	{
		return 0;
	}

	void xnOvrDestroySession(void* session)
	{
		
	}

	bool xnOvrCreateTexturesDx(void* session, void* dxDevice, int* outTextureCount)
	{
		return true;
	}

	void* xnOvrGetTextureAtIndexDx(void* session, GUID textureGuid, int index)
	{
		return 0;
	}

	void* xnOvrGetMirrorTextureDx(void* session, GUID textureGuid)
	{
		return 0;
	}

	int xnOvrGetCurrentTargetIndex(void* session)
	{
		return 0;
	}

	void xnOvrPrepareRender(void* session,
		float near, float far,
		float* projLeft, float* projRight,
		float* positionLeft, float* positionRight,
		float* rotationLeft, float* rotationRight)
	{
		
	}

	bool xnOvrCommitFrame(void* session)
	{
		return true;
	}
}

#endif
