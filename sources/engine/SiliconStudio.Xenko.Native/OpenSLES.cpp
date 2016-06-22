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
		SLInterfaceID* SL_IID_ENGINE_PTR = NULL;
		SLInterfaceID* SL_IID_BUFFERQUEUE_PTR = NULL;
		SLInterfaceID* SL_IID_VOLUME_PTR = NULL;
		SLInterfaceID* SL_IID_PLAY_PTR = NULL;

		npBool xnAudioInit()
		{
			if (OpenSLESLibrary) return true;

			OpenSLESLibrary = LoadDynamicLibrary("libOpenSLES");
			if (!OpenSLESLibrary) return false;

			SL_IID_ENGINE_PTR = (SLInterfaceID*)GetSymbolAddress(OpenSLESLibrary, "SL_IID_ENGINE");
			if (!SL_IID_ENGINE_PTR) return false;

			SL_IID_BUFFERQUEUE_PTR = (SLInterfaceID*)GetSymbolAddress(OpenSLESLibrary, "SL_IID_BUFFERQUEUE");
			if (!SL_IID_BUFFERQUEUE_PTR) return false;

			SL_IID_VOLUME_PTR = (SLInterfaceID*)GetSymbolAddress(OpenSLESLibrary, "SL_IID_VOLUME");
			if (!SL_IID_VOLUME_PTR) return false;

			SL_IID_PLAY_PTR = (SLInterfaceID*)GetSymbolAddress(OpenSLESLibrary, "SL_IID_PLAY");
			if (!SL_IID_PLAY_PTR) return false;

			slCreateEngineFunc = (slCreateEnginePtr)GetSymbolAddress(OpenSLESLibrary, "slCreateEngine");
			if (!slCreateEngineFunc) return false;

			return true;
		}

		struct xnAudioDevice
		{
			SLObjectItf device; 
			SLEngineItf engine;
			SLObjectItf outputMix;

		};

		struct xnAudioBuffer
		{
			char* dataPtr;
			int dataLength;
		};

		struct xnAudioListener
		{
			SLObjectItf object;
			SL3DLocationItf listener;
			xnAudioDevice* audioDevice;
		};

		struct xnAudioSource
		{
			int sampleRate;
			bool mono;
			bool streamed;
			xnAudioListener* listener;

			SLObjectItf object;
			SLPlayItf player;
			SLAndroidSimpleBufferQueueItf queue;
			SLVolumeItf volume;
		};

#define DEBUG_BREAK debugtrap();

		xnAudioDevice* xnAudioCreate(const char* deviceName)
		{
			auto res = new xnAudioDevice;
			
			SLEngineOption options[] = { { SL_ENGINEOPTION_THREADSAFE, SL_BOOLEAN_TRUE } };

			SLresult result;
			result = slCreateEngineFunc(&res->device, 1, options, 0, NULL, NULL);
			if(SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK
				delete res;
				return NULL;
			}

			result = (*res->device)->Realize(res->device, SL_BOOLEAN_FALSE);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK
				delete res;
				return NULL;
			}

			result = (*res->device)->GetInterface(res->device, *SL_IID_ENGINE_PTR, &res->engine);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK
				delete res;
				return NULL;
			}

			result = (*res->engine)->CreateOutputMix(res->engine, &res->outputMix, 0, NULL, NULL);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK
				delete res;
				return NULL;
			}

			result = (*res->outputMix)->Realize(res->outputMix, SL_BOOLEAN_FALSE);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK
				delete res;
				return NULL;
			}

			return res;
		}

		void xnAudioDestroy(xnAudioDevice* device)
		{
			(*device->device)->Destroy(device->device);
			delete device;
		}

		xnAudioListener* xnAudioListenerCreate(xnAudioDevice* device)
		{
			auto res = new xnAudioListener;
			res->audioDevice = device;
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

		void PlayerCallback(SLAndroidSimpleBufferQueueItf bq, void *context) {
//			assert(bq == bqPlayerBufferQueue);
//			assert(NULL == context);
//
//			short *nextBuffer = buffer[curBuffer];
//			int nextSize = sizeof(buffer[0]);
//
//			SLresult result;
//			result = (*bqPlayerBufferQueue)->Enqueue(bqPlayerBufferQueue, nextBuffer, nextSize);
//
//			// Comment from sample code:
//			// the most likely other result is SL_RESULT_BUFFER_INSUFFICIENT,
//			// which for this code example would indicate a programming error
//			assert(SL_RESULT_SUCCESS == result);
//
//			curBuffer ^= 1;  // Switch buffer
//							 // Render to the fresh buffer
//			audioCallback(buffer[curBuffer], BUFFER_SIZE_IN_SAMPLES);
		}

		xnAudioSource* xnAudioSourceCreate(xnAudioListener* listener, int sampleRate, int maxNBuffers, npBool mono, npBool spatialized, npBool streamed)
		{
			(void)spatialized;

			auto res = new xnAudioSource;
			res->listener = listener;
			res->sampleRate = sampleRate;
			res->mono = mono;
			res->streamed = streamed;

			SLDataFormat_PCM format;
			format.bitsPerSample = SL_PCMSAMPLEFORMAT_FIXED_16;
			format.samplesPerSec = 1000 * SLuint32(sampleRate); //milliHz
			format.numChannels = mono ? 1 : 2;
			format.containerSize = 16;
			format.formatType = SL_DATAFORMAT_PCM;
			format.endianness = SL_BYTEORDER_LITTLEENDIAN;
			format.channelMask = mono ? SL_SPEAKER_FRONT_LEFT : SL_SPEAKER_FRONT_LEFT | SL_SPEAKER_FRONT_RIGHT;

			SLDataLocator_AndroidSimpleBufferQueue bufferQueue = { SL_DATALOCATOR_ANDROIDSIMPLEBUFFERQUEUE, (SLuint32) maxNBuffers };

			SLDataSource audioSrc = { &bufferQueue, &format };
			SLDataLocator_OutputMix outMix = { SL_DATALOCATOR_OUTPUTMIX, listener->audioDevice->outputMix };
			SLDataSink sink = { &outMix, NULL };
			const SLInterfaceID ids[2] = { *SL_IID_BUFFERQUEUE_PTR, *SL_IID_VOLUME_PTR };
			const SLboolean req[2] = { SL_BOOLEAN_TRUE, SL_BOOLEAN_TRUE };
			SLresult result = (*listener->audioDevice->engine)->CreateAudioPlayer(listener->audioDevice->engine, &res->object, &audioSrc, &sink, 2, ids, req);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->object)->Realize(res->object, SL_BOOLEAN_FALSE);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_PLAY_PTR, &res->player);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_BUFFERQUEUE_PTR, &res->queue);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_BUFFERQUEUE_PTR, &res->queue);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_VOLUME_PTR, &res->volume);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->queue)->RegisterCallback(res->queue, PlayerCallback, res);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

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
			if (source->streamed) return;

			(*source->queue)->Enqueue(source->queue, buffer->dataPtr, buffer->dataLength);
		}

		void xnAudioSourceQueueBuffer(xnAudioSource* source, xnAudioBuffer* buffer, short* pcm, int bufferSize, bool endOfStream)
		{
			if (!source->streamed) return;

			(*source->queue)->Enqueue(source->queue, pcm, bufferSize);
		}

		xnAudioBuffer* xnAudioSourceGetFreeBuffer(xnAudioSource* source)
		{
			if (!source->streamed) return NULL;
			return NULL;
		}

		void xnAudioSourcePlay(xnAudioSource* source)
		{
			(*source->player)->SetPlayState(source->player, SL_PLAYSTATE_PLAYING);
		}

		void xnAudioSourcePause(xnAudioSource* source)
		{
			(*source->player)->SetPlayState(source->player, SL_PLAYSTATE_PAUSED);
		}

		void xnAudioSourceStop(xnAudioSource* source)
		{
			(*source->player)->SetPlayState(source->player, SL_PLAYSTATE_STOPPED);
		}

		void xnAudioListenerPush3D(xnAudioListener* listener, float* pos, float* forward, float* up, float* vel)
		{
		}

		void xnAudioSourcePush3D(xnAudioSource* source, float* pos, float* forward, float* up, float* vel)
		{
		}

		npBool xnAudioSourceIsPlaying(xnAudioSource* source)
		{
			SLuint32 res;
			(*source->player)->GetPlayState(source->player, &res);
			return res == SL_PLAYSTATE_PLAYING;
		}

		xnAudioBuffer* xnAudioBufferCreate(int maxBufferSize)
		{
			auto res = new xnAudioBuffer;
			res->dataPtr = new char[maxBufferSize];
			res->dataLength = maxBufferSize;
			return res;
		}

		void xnAudioBufferDestroy(xnAudioBuffer* buffer)
		{
			delete[] buffer->dataPtr;
			delete buffer;
		}

		void xnAudioBufferFill(xnAudioBuffer* buffer, short* pcm, int bufferSize, int sampleRate, npBool mono)
		{
			(void)sampleRate;
			(void)mono;
			buffer->dataLength = bufferSize;
			memcpy(buffer->dataPtr, pcm, bufferSize);
		}

		void xnSleep(int milliseconds)
		{
			npThreadSleep(milliseconds);
		}
	}
}

#endif
