// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(ANDROID) || !defined(__clang__)

#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../deps/NativePath/NativeThreading.h"
#include "../../../deps/NativePath/TINYSTL/unordered_map.h"
#include "../../../deps/OpenSLES/OpenSLES.h"
#include "../../../deps/OpenSLES/OpenSLES_Android.h"

extern "C" {
	class SpinLock
	{
	public:
		SpinLock()
		{
			mLocked = false;
		}

		void Lock()
		{			
			while(!__sync_bool_compare_and_swap(&mLocked, false, true)) {}
		}

		void Unlock()
		{
			mLocked = false;
		}

	private:
		volatile bool mLocked;
	};

	namespace OpenSLES
	{
		typedef SLresult SLAPIENTRY (*slCreateEnginePtr)(SLObjectItf* pEngine, SLuint32 numOptions, const SLEngineOption* pEngineOptions, SLuint32 numInterfaces, const SLInterfaceID* pInterfaceIds, const SLboolean* pInterfaceRequired);

		void* OpenSLESLibrary = NULL;
		slCreateEnginePtr slCreateEngineFunc = NULL;

		npBool xnAudioInit()
		{
			if (OpenSLESLibrary) return true;

			OpenSLESLibrary = LoadDynamicLibrary("libOpenSLES");
			if (!OpenSLESLibrary) return false;

			slCreateEngineFunc = (slCreateEnginePtr)GetSymbolAddress(OpenSLESLibrary, "slCreateEngine");

			return true;
		}

		#define AL_ERROR //if (auto err = GetErrorAL() != AL_NO_ERROR) debugtrap()
		#define ALC_ERROR(__device__) //if (auto err = GetErrorALC(__device__) != ALC_NO_ERROR) debugtrap()

		struct xnAudioDevice
		{
			SLObjectItf engine;
		};

		struct xnAudioBuffer
		{
		};

		struct xnAudioListener
		{

		};

		struct xnAudioSource
		{
			int sampleRate;
			bool mono;
			xnAudioListener* listener;
		};

		xnAudioDevice* xnAudioCreate(const char* deviceName)
		{
			auto res = new xnAudioDevice;
			
			SLresult result;
			result = slCreateEngineFunc(&res->engine, 0, NULL, 0, NULL, NULL);
			if(SL_RESULT_SUCCESS != result)
			{
				delete res;
				return NULL;
			}

			return res;
		}

		void xnAudioDestroy(xnAudioDevice* device)
		{
			(*device->engine)->Destroy(device->engine);
			delete device;
		}

		xnAudioListener* xnAudioListenerCreate(xnAudioDevice* device)
		{
			auto res = new xnAudioListener;
			return res;
		}

		void xnAudioListenerDestroy(xnAudioListener* listener)
		{
			delete listener;
		}

		npBool xnAudioListenerEnable(xnAudioListener* listener)
		{
			return true;
		}

		void xnAudioListenerDisable(xnAudioListener* listener)
		{
		}

		xnAudioSource* xnAudioSourceCreate(xnAudioListener* listener, int sampleRate, npBool mono, npBool spatialized)
		{
			(void)spatialized;

			auto res = new xnAudioSource;
			res->listener = listener;
			res->sampleRate = sampleRate;
			res->mono = mono;
		
			return res;
		}

		void xnAudioSourceDestroy(xnAudioSource* source)
		{
			delete source;
		}

		void xnAudioSourceSetPan(xnAudioSource* source, float pan)
		{
		}

		void xnAudioSourceSetLooping(xnAudioSource* source, npBool looping)
		{
		}

		void xnAudioSourceSetGain(xnAudioSource* source, float gain)
		{
		}

		void xnAudioSourceSetPitch(xnAudioSource* source, float pitch)
		{
		}

		void xnAudioSourceSetBuffer(xnAudioSource* source, xnAudioBuffer* buffer)
		{
		}

		void xnAudioSourceQueueBuffer(xnAudioSource* source, xnAudioBuffer* buffer, short* pcm, int bufferSize, bool endOfStream)
		{
		}

		xnAudioBuffer* xnAudioSourceGetFreeBuffer(xnAudioSource* source)
		{
			return NULL;
		}

		void xnAudioSourcePlay(xnAudioSource* source)
		{
		}

		void xnAudioSourcePause(xnAudioSource* source)
		{
		}

		void xnAudioSourceStop(xnAudioSource* source)
		{
		}

		void xnAudioListenerPush3D(xnAudioListener* listener, float* pos, float* forward, float* up, float* vel)
		{
		}

		void xnAudioSourcePush3D(xnAudioSource* source, float* pos, float* forward, float* up, float* vel)
		{
		}

		npBool xnAudioSourceIsPlaying(xnAudioSource* source)
		{
			return true;
		}

		xnAudioBuffer* xnAudioBufferCreate()
		{
			auto res = new xnAudioBuffer;
			return res;
		}

		void xnAudioBufferDestroy(xnAudioBuffer* buffer)
		{
			delete buffer;
		}

		void xnAudioBufferFill(xnAudioBuffer* buffer, short* pcm, int bufferSize, int sampleRate, npBool mono)
		{
		}

		void xnSleep(int milliseconds)
		{
			npThreadSleep(milliseconds);
		}
	}
}

#endif
