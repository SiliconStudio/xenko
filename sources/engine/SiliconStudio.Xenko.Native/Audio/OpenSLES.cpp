// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(ANDROID) || !defined(__clang__)

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../../deps/NativePath/NativeThreading.h"
#include "../../../../deps/NativePath/NativeMath.h"
#include "../../../../deps/NativePath/TINYSTL/vector.h"
#include "../../../../deps/NativePath/TINYSTL/unordered_set.h"
#include "../../../../deps/OpenSLES/OpenSLES.h"
#include "../../../../deps/OpenSLES/OpenSLES_Android.h"

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

		struct xnAudioSource;

		struct xnAudioDevice
		{
			SLObjectItf device; 
			SLEngineItf engine;
			SLObjectItf outputMix;
			SpinLock deviceLock;
			tinystl::unordered_set<xnAudioSource*> sources;
			volatile float masterVolume = 1.0f;
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
			float4 pos;
			float4 forward;
			float4 up;
			float4 velocity;
		};

		struct xnAudioSource
		{
			int sampleRate;
			bool mono;
			bool streamed;
			bool looped;
			volatile bool endOfStream;
			bool canRateChange;
			SLpermille minRate;
			SLpermille maxRate;
			volatile float gain = 1.0f;
			float localizationGain = 1.0f;
			volatile float pitch = 1.0f;
			volatile float doppler_pitch = 1.0f;

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

#define DEBUG_BREAK debugtrap()

		xnAudioDevice* xnAudioCreate(const char* deviceName)
		{
			auto res = new xnAudioDevice;
			
			SLEngineOption options[] = { { SL_ENGINEOPTION_THREADSAFE, SL_BOOLEAN_TRUE } };

			SLresult result;
			result = slCreateEngineFunc(&res->device, 1, options, 0, NULL, NULL);
			if(SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->device)->Realize(res->device, SL_BOOLEAN_FALSE);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->device)->GetInterface(res->device, *SL_IID_ENGINE_PTR, &res->engine);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->engine)->CreateOutputMix(res->engine, &res->outputMix, 0, NULL, NULL);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->outputMix)->Realize(res->outputMix, SL_BOOLEAN_FALSE);
			if (SL_RESULT_SUCCESS != result)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			return res;
		}

		void xnAudioDestroy(xnAudioDevice* device)
		{
			(*device->outputMix)->Destroy(device->outputMix);
			(*device->device)->Destroy(device->device);
			delete device;
		}

		void xnAudioSetMasterVolume(xnAudioDevice* device, float volume)
		{
			device->masterVolume = volume;

			device->deviceLock.Lock();
			
			for (xnAudioSource* source : device->sources)
			{
				auto dbVolume = SLmillibel(20 * log10(volume * source->gain * source->localizationGain) * 100);
				if (dbVolume < SL_MILLIBEL_MIN) dbVolume = SL_MILLIBEL_MIN;
				(*source->volume)->SetVolumeLevel(source->volume, dbVolume);
			}
			
			device->deviceLock.Unlock();
		}

		xnAudioListener* xnAudioListenerCreate(xnAudioDevice* device)
		{
			auto res = new xnAudioListener;
			memset(res, 0x0, sizeof(xnAudioListener));
			res->audioDevice = device;
			return res;
		}

		void xnAudioListenerDestroy(xnAudioListener* listener)
		{
			delete listener;
		}

		npBool xnAudioListenerEnable(xnAudioListener* listener)
		{
			(void)listener;
			return true;
		}

		void xnAudioListenerDisable(xnAudioListener* listener)
		{
			(void)listener;
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
			const SLInterfaceID ids[3] = { *SL_IID_BUFFERQUEUE_PTR, *SL_IID_VOLUME_PTR, *SL_IID_PLAYBACKRATE_PTR };
			const SLboolean req[3] = { SL_BOOLEAN_TRUE, SL_BOOLEAN_TRUE, SL_BOOLEAN_TRUE };
			auto result = (*listener->audioDevice->engine)->CreateAudioPlayer(listener->audioDevice->engine, &res->object, &audioSrc, &sink, 3, ids, req);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			res->canRateChange = true;
			result = (*res->object)->Realize(res->object, SL_BOOLEAN_FALSE);
			if (result != SL_RESULT_SUCCESS)
			{
				res->canRateChange = false;
				result = (*listener->audioDevice->engine)->CreateAudioPlayer(listener->audioDevice->engine, &res->object, &audioSrc, &sink, 2, ids, req);
				if (result != SL_RESULT_SUCCESS)
				{
					DEBUG_BREAK;
					delete res;
					return NULL;
				}
				result = (*res->object)->Realize(res->object, SL_BOOLEAN_FALSE);
				if (result != SL_RESULT_SUCCESS)
				{
					DEBUG_BREAK;
					delete res;
					return NULL;
				}
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_PLAY_PTR, &res->player);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_BUFFERQUEUE_PTR, &res->queue);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->object)->GetInterface(res->object, *SL_IID_VOLUME_PTR, &res->volume);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			if (res->canRateChange)
			{
				//For some reason this was not working in Android N...
				result = (*res->object)->GetInterface(res->object, *SL_IID_PLAYBACKRATE_PTR, &res->playRate);
				if (result != SL_RESULT_SUCCESS)
				{
					DEBUG_BREAK;
					delete res;
					return NULL;
				}
			}

			result = (*res->volume)->EnableStereoPosition(res->volume, SL_BOOLEAN_TRUE);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			result = (*res->queue)->RegisterCallback(res->queue, PlayerCallback, res);
			if (result != SL_RESULT_SUCCESS)
			{
				DEBUG_BREAK;
				delete res;
				return NULL;
			}

			listener->audioDevice->deviceLock.Lock();

			listener->audioDevice->sources.insert(res);
			
			listener->audioDevice->deviceLock.Unlock();

			return res;
		}

		void xnAudioSourceDestroy(xnAudioSource* source)
		{
			source->listener->audioDevice->deviceLock.Lock();

			source->listener->audioDevice->sources.erase(source);

			source->listener->audioDevice->deviceLock.Unlock();
		
			(*source->object)->Destroy(source->object);
			
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
			source->gain = gain;
			gain *= source->localizationGain * source->listener->audioDevice->masterVolume;
			auto dbVolume = SLmillibel(20 * log10(gain) * 100);
			if (dbVolume < SL_MILLIBEL_MIN) dbVolume = SL_MILLIBEL_MIN;
			(*source->volume)->SetVolumeLevel(source->volume, dbVolume);
		}

		void xnAudioSourceSetPitch(xnAudioSource* source, float pitch)
		{
			if (!source->canRateChange) return;

			source->pitch = pitch;

			pitch *= source->doppler_pitch;
			pitch = pitch > 4.0f ? 4.0f : pitch < -4.0f ? -4.0f : pitch;
			(*source->playRate)->SetRate(source->playRate, SLpermille(pitch * 1000.0f));
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
			memcpy(&listener->pos, pos, sizeof(float) * 3);
			memcpy(&listener->forward, forward, sizeof(float) * 3);
			memcpy(&listener->up, up, sizeof(float) * 3);
			memcpy(&listener->velocity, vel, sizeof(float) * 3);
		}

		const float SoundSpeed = 343.0f;
		const float SoundFreq = 600.0f;
		const float SoundPeriod = 1 / SoundFreq;
		const float ZeroTolerance = 1e-6f;
		const float MaxValue = 3.402823E+38f;
#define E_PI 3.1415926535897932384626433832795028841971693993751058209749445923078164062

		void xnAudioSourcePush3D(xnAudioSource* source, float* ppos, float* pforward, float* pup, float* pvel)
		{
			float4 pos;
			memcpy(&pos, ppos, sizeof(float) * 3);
			float4 forward;
			memcpy(&forward, pforward, sizeof(float) * 3);
			float4 up;
			memcpy(&up, pup, sizeof(float) * 3);
			float4 vel;
			memcpy(&vel, pvel, sizeof(float) * 3);

#ifdef __clang__ //resharper does not know about opencl vectors

			// To evaluate the Doppler effect we calculate the distance to the listener from one wave to the next one and divide it by the sound speed
			// we use 343m/s for the sound speed which correspond to the sound speed in the air.
			// we use 600Hz for the sound frequency which correspond to the middle of the human hearable sounds frequencies.

			auto dopplerShift = 1.0f;

			auto vecListEmit = pos - source->listener->pos;
			auto distListEmit = npLengthF4(vecListEmit);

			// avoid useless calculations.
			if (!(vel.x == 0 && vel.y == 0 && vel.z == 0 && source->listener->velocity.x == 0 && source->listener->velocity.y == 0 && source->listener->velocity.z == 0))
			{
				auto vecListEmitNorm = vecListEmit;
				if (distListEmit > ZeroTolerance)
				{
					auto inv = 1.0f / distListEmit;
					vecListEmitNorm *= inv;
				}

				auto vecListEmitSpeed = vel - source->listener->velocity;
				auto speedDot = vecListEmitSpeed[0] * vecListEmitNorm[0] + vecListEmitSpeed[1] * vecListEmitNorm[1] + vecListEmitSpeed[2] * vecListEmitNorm[2];
				if (speedDot < -SoundSpeed) // emitter and listener are getting closer more quickly than the speed of the sound.
				{
					dopplerShift = MaxValue; //positive infinity
				}
				else
				{
					auto timeSinceLastWaveArrived = 0.0f; // time elapsed since the previous wave arrived to the listener.
					auto lastWaveDistToListener = 0.0f; // the distance that the last wave still have to travel to arrive to the listener.
					const auto DistLastWave = SoundPeriod * SoundSpeed; // distance traveled by the previous wave.
					if (DistLastWave > distListEmit)
						timeSinceLastWaveArrived = (DistLastWave - distListEmit) / SoundSpeed;
					else
						lastWaveDistToListener = distListEmit - DistLastWave;

					auto nextVecListEmit = vecListEmit + SoundPeriod * vecListEmitSpeed;
					auto nextWaveDistToListener = sqrtf(nextVecListEmit[0] * nextVecListEmit[0] + nextVecListEmit[1] * nextVecListEmit[1] + nextVecListEmit[2] * nextVecListEmit[2]);
					auto timeBetweenTwoWaves = timeSinceLastWaveArrived + (nextWaveDistToListener - lastWaveDistToListener) / SoundSpeed;
					auto apparentFrequency = 1 / timeBetweenTwoWaves;
					dopplerShift = apparentFrequency / SoundFreq;
				}
			}

			source->doppler_pitch = dopplerShift;
			auto pitch = source->pitch * dopplerShift;
			pitch = pitch > 4.0f ? 4.0f : pitch < -4.0f ? -4.0f : pitch;
			(*source->playRate)->SetRate(source->playRate, SLpermille(pitch * 1000.0f));

			// After an analysis of the XAudio2 left/right stereo balance with respect to 3D world position, 
			// it could be found the volume repartition is symmetric to the Up/Down and Front/Back planes.
			// Moreover the left/right repartition seems to follow a third degree polynomial function:
			// Volume_left(a) = 2(c-1)*a^3 - 3(c-1)*a^2 + c*a , where c is a constant close to c = 1.45f and a is the angle normalized bwt [0,1]
			// Volume_right(a) = 1-Volume_left(a)

			// As for signal attenuation wrt distance the model follows a simple inverse square law function as explained in XAudio2 documentation 
			// ( http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.x3daudio.x3daudio_emitter(v=vs.85).aspx )
			// Volume(d) = 1                    , if d <= ScaleDistance where d is the distance to the listener
			// Volume(d) = ScaleDistance / d    , if d >= ScaleDistance where d is the distance to the listener

			auto attenuationFactor = distListEmit <= 1.0f ? 1.0f : 1.0f / distListEmit;

			// 2. Left/Right balance.
			auto repartRight = 0.5f;
			float4 rightVec = npCrossProductF4(source->listener->forward, source->listener->up);

			float4 worldToList[4];
			npMatrixIdentityF4(worldToList);
			worldToList[0].x = rightVec.x;
			worldToList[0].y = source->listener->forward.x;
			worldToList[0].z = source->listener->up.x;
			worldToList[1].x = rightVec.y;
			worldToList[1].y = source->listener->forward.y;
			worldToList[1].z = source->listener->up.y;
			worldToList[2].x = rightVec.z;
			worldToList[2].y = source->listener->forward.z;
			worldToList[2].z = source->listener->up.z;

			auto vecListEmitListBase = npTransformNormalF4(vecListEmit, worldToList);
			auto vecListEmitListBaseLen = npLengthF4(vecListEmitListBase);
			if(vecListEmitListBaseLen > 0.0f)
			{
				const auto c = 1.45f;
				auto absAlpha = fabsf(atan2f(vecListEmitListBase.y, vecListEmitListBase.x));
				auto normAlpha = absAlpha / (E_PI / 2.0f);
				if (absAlpha > E_PI / 2.0f) normAlpha = 2.0f - normAlpha;
				repartRight = 0.5f * (2 * (c - 1) * normAlpha * normAlpha * normAlpha - 3 * (c - 1) * normAlpha * normAlpha * normAlpha + c * normAlpha);
				if (absAlpha > E_PI / 2.0f) repartRight = 1 - repartRight;
			}

			xnAudioSourceSetPan(source, repartRight - 0.5);
			source->localizationGain = attenuationFactor;
			xnAudioSourceSetGain(source, source->gain);
#endif
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
