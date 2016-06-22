// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(ANDROID) || !defined(__clang__)

#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../deps/NativePath/NativeThreading.h"
#include "../../../deps/NativePath/TINYSTL/vector.h"
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
		SLInterfaceID* SL_IID_PLAYBACKRATE_PTR = NULL;

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

			SL_IID_PLAYBACKRATE_PTR = (SLInterfaceID*)GetSymbolAddress(OpenSLESLibrary, "SL_IID_PLAYBACKRATE");
			if (!SL_IID_PLAYBACKRATE_PTR) return false;

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
			int dataLength;
			bool endOfStream;
			char* dataPtr;
		};

		struct xnAudioListener
		{
			xnAudioDevice* audioDevice;
		};

		struct xnAudioSource
		{
			int sampleRate;
			bool mono;
			bool streamed;
			bool looped;
			volatile bool endOfStream;

			xnAudioListener* listener;

			SLObjectItf object;
			SLPlayItf player;
			SLAndroidSimpleBufferQueueItf queue;
			SLVolumeItf volume;
			SLPlaybackRateItf playRate;

			tinystl::vector<xnAudioBuffer*> streamBuffers;
			tinystl::vector<xnAudioBuffer*> freeBuffers;
			SpinLock buffersLock;
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
			(void)bq;
			auto source = static_cast<xnAudioSource*>(context);
			if(!source->streamed) //looped
			{
				if (source->looped)
				{
					(*source->queue)->Enqueue(source->queue, source->streamBuffers[0]->dataPtr, source->streamBuffers[0]->dataLength);
				}
				else
				{
					(*source->player)->SetPlayState(source->player, SL_PLAYSTATE_STOPPED);
				}
			}
			else
			{
				source->buffersLock.Lock();

				//release the next buffer
				if (!source->streamBuffers.empty())
				{
					auto nextBuffer = source->streamBuffers.front();
					source->streamBuffers.erase(source->streamBuffers.begin());

					if(nextBuffer->endOfStream && !source->looped)
					{
						(*source->player)->SetPlayState(source->player, SL_PLAYSTATE_STOPPED);
						
						//flush buffers
						for(auto buffer : source->streamBuffers)
						{
							source->freeBuffers.push_back(buffer);
						}
						source->streamBuffers.clear();
					}

					source->freeBuffers.push_back(nextBuffer);
				}
				
				source->buffersLock.Unlock();				
			}
		}

		xnAudioSource* xnAudioSourceCreate(xnAudioListener* listener, int sampleRate, int maxNBuffers, npBool mono, npBool spatialized, npBool streamed)
		{
			(void)spatialized;

			auto res = new xnAudioSource;
			res->listener = listener;
			res->sampleRate = sampleRate;
			res->mono = mono;
			res->streamed = streamed;
			res->looped = false;

			SLDataFormat_PCM format;
			format.bitsPerSample = SL_PCMSAMPLEFORMAT_FIXED_16;
			format.samplesPerSec = 1000 * SLuint32(sampleRate); //milliHz
			format.numChannels = mono ? 1 : 2;
			format.containerSize = 16;
			format.formatType = SL_DATAFORMAT_PCM;
			format.endianness = SL_BYTEORDER_LITTLEENDIAN;
			format.channelMask = mono ? SL_SPEAKER_FRONT_CENTER : SL_SPEAKER_FRONT_LEFT | SL_SPEAKER_FRONT_RIGHT;

			SLDataLocator_AndroidSimpleBufferQueue bufferQueue = { SL_DATALOCATOR_ANDROIDSIMPLEBUFFERQUEUE, (SLuint32) maxNBuffers };

			SLDataSource audioSrc = { &bufferQueue, &format };
			SLDataLocator_OutputMix outMix = { SL_DATALOCATOR_OUTPUTMIX, listener->audioDevice->outputMix };
			SLDataSink sink = { &outMix, NULL };
			const SLInterfaceID ids[3] = { *SL_IID_BUFFERQUEUE_PTR,*SL_IID_PLAYBACKRATE_PTR, *SL_IID_VOLUME_PTR };
			const SLboolean req[3] = { SL_BOOLEAN_TRUE, SL_BOOLEAN_TRUE, SL_BOOLEAN_TRUE };
			auto result = (*listener->audioDevice->engine)->CreateAudioPlayer(listener->audioDevice->engine, &res->object, &audioSrc, &sink, 3, ids, req);
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

			result = (*res->object)->GetInterface(res->object, *SL_IID_VOLUME_PTR, &res->volume);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_PLAYBACKRATE_PTR, &res->playRate);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK
			}

			result = (*res->volume)->EnableStereoPosition(res->volume, SL_BOOLEAN_TRUE);
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
			(*source->volume)->SetStereoPosition(source->volume, SLpermille(pan * 1000.0f));
		}

		void xnAudioSourceSetLooping(xnAudioSource* source, npBool looping)
		{
			source->looped = looping;
		}

		void xnAudioSourceSetGain(xnAudioSource* source, float gain)
		{
			auto dbVolume = SLmillibel(log10f(gain) * 20 * 100);
			(*source->volume)->SetVolumeLevel(source->volume, dbVolume);
		}

		void xnAudioSourceSetPitch(xnAudioSource* source, float pitch)
		{
			//(*source->playRate)->SetRate(source->playRate, SLpermille(pitch * 1000.0f));
		}

		void xnAudioSourceSetBuffer(xnAudioSource* source, xnAudioBuffer* buffer)
		{
			if (source->streamed) return;

			source->buffersLock.Lock();

			source->streamBuffers[0] = buffer;
			(*source->queue)->Enqueue(source->queue, buffer->dataPtr, buffer->dataLength);

			source->buffersLock.Unlock();
		}

		void xnAudioSourceQueueBuffer(xnAudioSource* source, xnAudioBuffer* buffer, short* pcm, int bufferSize, bool endOfStream)
		{
			if (!source->streamed) return;

			buffer->endOfStream = endOfStream;
			buffer->dataLength = bufferSize;
			memcpy(buffer->dataPtr, pcm, bufferSize);

			source->buffersLock.Lock();

			source->streamBuffers.push_back(buffer);
			(*source->queue)->Enqueue(source->queue, buffer->dataPtr, buffer->dataLength);

			source->buffersLock.Unlock();
		}

		xnAudioBuffer* xnAudioSourceGetFreeBuffer(xnAudioSource* source)
		{
			if (!source->streamed) return NULL;

			xnAudioBuffer* freeBuffer = NULL;

			source->buffersLock.Lock();

			if(!source->freeBuffers.empty())
			{
				freeBuffer = source->freeBuffers.back();
				source->freeBuffers.pop_back();
			}

			source->buffersLock.Unlock();

			return freeBuffer;
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
			buffer->endOfStream = true;
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
