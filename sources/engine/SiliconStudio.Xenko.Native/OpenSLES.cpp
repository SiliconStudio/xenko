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

			return res;
		}

		void xnAudioSourceDestroy(xnAudioSource* source)
		{
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
			auto dbVolume = SLmillibel(log10f(gain) * 20 * 100);
			(*source->volume)->SetVolumeLevel(source->volume, dbVolume);
		}

		void xnAudioSourceSetPitch(xnAudioSource* source, float pitch)
		{
			if (!source->canRateChange) return;

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

		float ComputeDopplerFactor(xnAudioListener* listener, float4 pos, float4 forward, float4 up, float4 vel)
		{
#ifdef __clang__ //resharper does not know about opencl vectors

			// To evaluate the Doppler effect we calculate the distance to the listener from one wave to the next one and divide it by the sound speed
			// we use 343m/s for the sound speed which correspond to the sound speed in the air.
			// we use 600Hz for the sound frequency which correspond to the middle of the human hearable sounds frequencies.

			const float SoundSpeed = 343.0f;
			const float SoundFreq = 600.0f;
			const float SoundPeriod = 1 / SoundFreq;
			const float ZeroTolerance = 1e-6f;
			float MaxValue = 3.402823E+38f;

			// avoid useless calculations.
			if(vel.x == 0 && vel.y == 0 && vel.z == 0 && listener->velocity.x == 0 && listener->velocity.y == 0 && listener->velocity.z == 0)
			{
				return 1.0f;
			}

			float4 vecListEmit = pos - listener->pos;
			auto distListEmit = sqrtf(vecListEmit[0] * vecListEmit[0] + vecListEmit[1] * vecListEmit[1] + vecListEmit[2] * vecListEmit[2]);

			float4 vecListEmitNorm;
			if(distListEmit > ZeroTolerance)
			{
				float inv = 1.0f / distListEmit;
				vecListEmitNorm *= inv;
			}
			else
			{
				memcpy(&vecListEmitNorm, &vecListEmit, sizeof(float) * 3);
			}

			float4 vecListEmitSpeed = vel - listener->velocity;
			auto speedDot = (vecListEmitSpeed[0] * vecListEmitNorm[0]) + (vecListEmitSpeed[1] * vecListEmitNorm[1]) + (vecListEmitSpeed[2] * vecListEmitNorm[2]);
			if (speedDot < -SoundSpeed) // emitter and listener are getting closer more quickly than the speed of the sound.
			{
				return MaxValue; //positive infinity
			}

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
			return apparentFrequency / SoundFreq;
#else

			return 0;
#endif
		}

		void xnAudioSourcePush3D(xnAudioSource* source, float* pos, float* forward, float* up, float* vel)
		{
			float4 vpos;
			memcpy(&vpos, pos, sizeof(float) * 3);
			float4 vforward;
			memcpy(&vforward, forward, sizeof(float) * 3);
			float4 vup;
			memcpy(&vup, up, sizeof(float) * 3);
			float4 vvel;
			memcpy(&vvel, vel, sizeof(float) * 3);

			auto doppler = ComputeDopplerFactor(source->listener, vpos, vforward, vup, vvel);
			
			xnAudioSourceSetPitch(source, doppler);

			/*

			// Since android has no function available to perform sound 3D localization by default, here we try to mimic the behaviour of XAudio2

			// After an analysis of the XAudio2 left/right stereo balance with respect to 3D world position, 
			// it could be found the volume repartition is symmetric to the Up/Down and Front/Back planes.
			// Moreover the left/right repartition seems to follow a third degree polynomial function:
			// Volume_left(a) = 2(c-1)*a^3 - 3(c-1)*a^2 + c*a , where c is a constant close to c = 1.45f and a is the angle normalized bwt [0,1]
			// Volume_right(a) = 1-Volume_left(a)

			// As for signal attenuation wrt distance the model follows a simple inverse square law function as explained in XAudio2 documentation 
			// ( http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.x3daudio.x3daudio_emitter(v=vs.85).aspx )
			// Volume(d) = 1                    , if d <= ScaleDistance where d is the distance to the listener
			// Volume(d) = ScaleDistance / d    , if d >= ScaleDistance where d is the distance to the listener

			// 1. Attenuation due to distance.
			var vecListEmit = emitter.Position - listener.Position;
			var distListEmit = vecListEmit.Length();
			var attenuationFactor = distListEmit <= emitter.DistanceScale ? 1f : emitter.DistanceScale / distListEmit;

			// 2. Left/Right balance.
			var repartRight = 0.5f;
			var worldToList = Matrix.Identity;
			var rightVec = Vector3.Cross(listener.Forward, listener.Up);
			worldToList.Column1 = new Vector4(rightVec, 0);
			worldToList.Column2 = new Vector4(listener.Forward, 0);
			worldToList.Column3 = new Vector4(listener.Up, 0);
			var vecListEmitListBase = Vector3.TransformNormal(vecListEmit, worldToList);
			var vecListEmitListBase2 = (Vector2)vecListEmitListBase;
			if (vecListEmitListBase2.Length() > 0)
			{
				const float c = 1.45f;
				var absAlpha = Math.Abs(Math.Atan2(vecListEmitListBase2.Y, vecListEmitListBase2.X));
				var normAlpha = (float)(absAlpha / (Math.PI / 2));
				if (absAlpha > Math.PI / 2) normAlpha = 2 - normAlpha;
				repartRight = 0.5f * (2 * (c - 1) * normAlpha * normAlpha * normAlpha - 3 * (c - 1) * normAlpha * normAlpha * normAlpha + c * normAlpha);
				if (absAlpha > Math.PI / 2) repartRight = 1 - repartRight;
			}

			// Set the volumes.
			localizationChannelVolumes = new[] { attenuationFactor * (1f - repartRight), attenuationFactor * repartRight };
			UpdateStereoVolumes();

			// 3. Calculation of the Doppler effect
			ComputeDopplerFactor(listener, emitter);
			UpdatePitch();
			*/
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
