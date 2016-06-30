// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(LINUX) || defined(IOS) || !defined(__clang__)

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../../deps/NativePath/NativeThreading.h"
#include "../../../../deps/NativePath/TINYSTL/unordered_map.h"
#include "../../../../deps/NativePath/TINYSTL/unordered_set.h"


#define HAVE_STDINT_H
#include "../../../../deps/OpenAL/AL/al.h"
#include "../../../../deps/OpenAL/AL/alc.h"

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

	namespace OpenAL
	{
		LPALCOPENDEVICE OpenDevice;
		LPALCCLOSEDEVICE CloseDevice;
		LPALCCREATECONTEXT CreateContext;
		LPALCDESTROYCONTEXT DestroyContext;
		LPALCMAKECONTEXTCURRENT MakeContextCurrent;
		LPALCGETCURRENTCONTEXT GetCurrentContext;
		LPALCPROCESSCONTEXT ProcessContext;
		LPALCGETERROR GetErrorALC;
		LPALCSUSPENDCONTEXT SuspendContext;
		
		LPALSOURCEPLAY SourcePlay;
		LPALSOURCEPAUSE SourcePause;
		LPALSOURCESTOP SourceStop;
		LPALSOURCEF SourceF;
		LPALDELETESOURCES DeleteSources;
		LPALDELETEBUFFERS DeleteBuffers;
		LPALGENSOURCES GenSources;
		LPALGENBUFFERS GenBuffers;
		LPALSOURCE3I Source3I;
		LPALSOURCEI SourceI;
		LPALBUFFERDATA BufferData;
		LPALSOURCEQUEUEBUFFERS SourceQueueBuffers;
		LPALSOURCEUNQUEUEBUFFERS SourceUnqueueBuffers;
		LPALGETSOURCEI GetSourceI;
		LPALSOURCEFV SourceFV;
		LPALLISTENERFV ListenerFV;
		LPALGETERROR GetErrorAL;

		void* OpenALLibrary = NULL;

		class ContextState
		{
		public:
			ContextState(ALCcontext* context)
			{
				sOpenAlLock.Lock();

				mOldContext = GetCurrentContext();
				if (context != mOldContext)
				{
					MakeContextCurrent(context);
					swap = true;
				}
				else
				{
					swap = false;
				}
			}

			~ContextState()
			{
				if (swap)
				{
					MakeContextCurrent(mOldContext);
				}
				
				sOpenAlLock.Unlock();
			}

		private:
			bool swap;
			ALCcontext* mOldContext;
			static SpinLock sOpenAlLock;
		};

		SpinLock ContextState::sOpenAlLock;

		npBool xnAudioInit()
		{
			if (OpenALLibrary) return true;

			//Generic
			OpenALLibrary = LoadDynamicLibrary("OpenAL");
			OpenALLibrary = LoadDynamicLibrary("openal");
			
			//PC - Windows
			if(sizeof(intptr_t) == 4)
			{
				if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x86\\OpenAL");
				if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x86/OpenAL");
			}
			else
			{
				if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x64\\OpenAL");
				if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x64/OpenAL");
			}
			
			//iOS
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("/System/Library/Frameworks/OpenAL.framework/OpenAL"); //iOS Apple OpenAL

			//Linux
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("libopenal.so.1");
			
			if (!OpenALLibrary) return false;

			OpenDevice = (LPALCOPENDEVICE)GetSymbolAddress(OpenALLibrary, "alcOpenDevice");
			if (!OpenDevice) return false;
			CloseDevice = (LPALCCLOSEDEVICE)GetSymbolAddress(OpenALLibrary, "alcCloseDevice");
			if (!CloseDevice) return false;
			CreateContext = (LPALCCREATECONTEXT)GetSymbolAddress(OpenALLibrary, "alcCreateContext");
			if (!CreateContext) return false;
			DestroyContext = (LPALCDESTROYCONTEXT)GetSymbolAddress(OpenALLibrary, "alcDestroyContext");
			if (!DestroyContext) return false;
			MakeContextCurrent = (LPALCMAKECONTEXTCURRENT)GetSymbolAddress(OpenALLibrary, "alcMakeContextCurrent");
			if (!MakeContextCurrent) return false;
			GetCurrentContext = (LPALCGETCURRENTCONTEXT)GetSymbolAddress(OpenALLibrary, "alcGetCurrentContext");
			if (!GetCurrentContext) return false;
			ProcessContext = (LPALCPROCESSCONTEXT)GetSymbolAddress(OpenALLibrary, "alcProcessContext");
			if (!ProcessContext) return false;
			GetErrorALC = (LPALCGETERROR)GetSymbolAddress(OpenALLibrary, "alcGetError");
			if (!GetErrorALC) return false;
			SuspendContext = (LPALCSUSPENDCONTEXT)GetSymbolAddress(OpenALLibrary, "alcSuspendContext");
			if (!SuspendContext) return false;

			SourcePlay = (LPALSOURCEPLAY)GetSymbolAddress(OpenALLibrary, "alSourcePlay");
			if (!SourcePlay) return false;
			SourcePause = (LPALSOURCEPAUSE)GetSymbolAddress(OpenALLibrary, "alSourcePause");
			if (!SourcePause) return false;
			SourceStop = (LPALSOURCESTOP)GetSymbolAddress(OpenALLibrary, "alSourceStop");
			if (!SourceStop) return false;
			SourceF = (LPALSOURCEF)GetSymbolAddress(OpenALLibrary, "alSourcef");
			if (!SourceF) return false;
			DeleteSources = (LPALDELETESOURCES)GetSymbolAddress(OpenALLibrary, "alDeleteSources");
			if (!DeleteSources) return false;
			DeleteBuffers = (LPALDELETEBUFFERS)GetSymbolAddress(OpenALLibrary, "alDeleteBuffers");
			if (!DeleteBuffers) return false;
			GenSources = (LPALGENSOURCES)GetSymbolAddress(OpenALLibrary, "alGenSources");
			if (!GenSources) return false;
			GenBuffers = (LPALGENBUFFERS)GetSymbolAddress(OpenALLibrary, "alGenBuffers");
			if (!GenBuffers) return false;
			Source3I = (LPALSOURCE3I)GetSymbolAddress(OpenALLibrary, "alSource3i");
			if (!Source3I) return false;
			SourceI = (LPALSOURCEI)GetSymbolAddress(OpenALLibrary, "alSourcei"); 
			if (!SourceI) return false;
			BufferData = (LPALBUFFERDATA)GetSymbolAddress(OpenALLibrary, "alBufferData");
			if (!BufferData) return false;
			SourceQueueBuffers = (LPALSOURCEQUEUEBUFFERS)GetSymbolAddress(OpenALLibrary, "alSourceQueueBuffers"); 
			if (!SourceQueueBuffers) return false;
			SourceUnqueueBuffers = (LPALSOURCEUNQUEUEBUFFERS)GetSymbolAddress(OpenALLibrary, "alSourceUnqueueBuffers");
			if (!SourceUnqueueBuffers) return false;
			GetSourceI = (LPALGETSOURCEI)GetSymbolAddress(OpenALLibrary, "alGetSourcei");
			if (!GetSourceI) return false;
			SourceFV = (LPALSOURCEFV)GetSymbolAddress(OpenALLibrary, "alSourcefv");
			if (!SourceFV) return false;
			ListenerFV = (LPALLISTENERFV)GetSymbolAddress(OpenALLibrary, "alListenerfv");
			if (!ListenerFV) return false;
			GetErrorAL = (LPALGETERROR)GetSymbolAddress(OpenALLibrary, "alGetError");
			if (!GetErrorAL) return false;

			return true;
		}

		#define AL_ERROR //if (auto err = GetErrorAL() != AL_NO_ERROR) debugtrap()
		#define ALC_ERROR(__device__) //if (auto err = GetErrorALC(__device__) != ALC_NO_ERROR) debugtrap()

		struct xnAudioListener;

		struct xnAudioDevice
		{
			ALCdevice* device;
			SpinLock deviceLock;
			tinystl::unordered_set<xnAudioListener*> listeners;
		};

		struct xnAudioBuffer
		{
			ALuint buffer;
		};

		struct xnAudioListener
		{
			xnAudioDevice* device;
			ALCcontext* context;
			tinystl::unordered_map<ALuint, xnAudioBuffer*> buffers;
		};

		struct xnAudioSource
		{
			ALuint source;
			int sampleRate;
			bool mono;
			xnAudioListener* listener;
		};

		xnAudioDevice* xnAudioCreate(const char* deviceName)
		{
			auto res = new xnAudioDevice;
			res->device = OpenDevice(deviceName);
			ALC_ERROR(res->device);
			if (!res->device)
			{
				delete res;
				return NULL;
			}
			return res;
		}

		void xnAudioDestroy(xnAudioDevice* device)
		{
			CloseDevice(device->device);
			ALC_ERROR(device->device);
			delete device;
		}

		xnAudioListener* xnAudioListenerCreate(xnAudioDevice* device)
		{
			auto res = new xnAudioListener;
			res->device = device;

			res->context = CreateContext(device->device, NULL);
			ALC_ERROR(device->device);
			MakeContextCurrent(res->context);
			ALC_ERROR(device->device);
			ProcessContext(res->context);
			ALC_ERROR(device->device);

			device->deviceLock.Lock();

			device->listeners.insert(res);

			device->deviceLock.Unlock();

			return res;
		}

		void xnAudioListenerDestroy(xnAudioListener* listener)
		{
			listener->device->deviceLock.Lock();

			listener->device->listeners.erase(listener);

			listener->device->deviceLock.Unlock();

			DestroyContext(listener->context);

			delete listener;
		}

		void xnAudioSetMasterVolume(xnAudioDevice* device, float volume)
		{
			device->deviceLock.Lock();
			for(auto listener : device->listeners)
			{
				ContextState lock(listener->context);
				alListenerf(AL_GAIN, volume);
			}
			device->deviceLock.Unlock();
		}

		npBool xnAudioListenerEnable(xnAudioListener* listener)
		{
			bool res = MakeContextCurrent(listener->context);
			ProcessContext(listener->context);
			return res;
		}

		void xnAudioListenerDisable(xnAudioListener* listener)
		{
			SuspendContext(listener->context);
			MakeContextCurrent(NULL);
		}

		xnAudioSource* xnAudioSourceCreate(xnAudioListener* listener, int sampleRate, int maxNBuffers, npBool mono, npBool spatialized, npBool streamed)
		{
			(void)spatialized;
			(void)streamed;

			auto res = new xnAudioSource;
			res->listener = listener;
			res->sampleRate = sampleRate;
			res->mono = mono;

			ContextState lock(listener->context);

			GenSources(1, &res->source);
			AL_ERROR;
			SourceF(res->source, AL_REFERENCE_DISTANCE, 1.0f);
			AL_ERROR;

			if(spatialized)
			{
				//make sure we are able to 3D
				SourceI(res->source, AL_SOURCE_RELATIVE, AL_FALSE);
			}
			else
			{
				//make sure we are able to pan
				SourceI(res->source, AL_SOURCE_RELATIVE, AL_TRUE);
			}
			
			return res;
		}

		void xnAudioSourceDestroy(xnAudioSource* source)
		{
			ContextState lock(source->listener->context);

			DeleteSources(1, &source->source);
			AL_ERROR;

			delete source;
		}

		void xnAudioSourceSetPan(xnAudioSource* source, float pan)
		{
			auto clampedPan = pan > 1.0f ? 1.0f : pan < -1.0f ? -1.0f : pan;
			ALfloat alpan[3];
			alpan[0] = clampedPan; // from -1 (left) to +1 (right) 
			alpan[1] = sqrt(1.0f - clampedPan*clampedPan);
			alpan[2] = 0.0f;

			ContextState lock(source->listener->context);

			SourceFV(source->source, AL_POSITION, alpan);
		}

		void xnAudioSourceSetLooping(xnAudioSource* source, npBool looping)
		{
			ContextState lock(source->listener->context);

			SourceI(source->source, AL_LOOPING, looping ? AL_TRUE : AL_FALSE);
		}

		void xnAudioSourceSetGain(xnAudioSource* source, float gain)
		{
			ContextState lock(source->listener->context);

			SourceF(source->source, AL_GAIN, gain);
		}

		void xnAudioSourceSetPitch(xnAudioSource* source, float pitch)
		{
			ContextState lock(source->listener->context);

			SourceF(source->source, AL_PITCH, pitch);
		}

		void xnAudioSourceSetBuffer(xnAudioSource* source, xnAudioBuffer* buffer)
		{
			ContextState lock(source->listener->context);

			SourceI(source->source, AL_BUFFER, buffer->buffer);
		}

		void xnAudioSourceQueueBuffer(xnAudioSource* source, xnAudioBuffer* buffer, short* pcm, int bufferSize, bool endOfStream)
		{
			(void)endOfStream;

			ContextState lock(source->listener->context);

			BufferData(buffer->buffer, source->mono ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, pcm, bufferSize, source->sampleRate);
			SourceQueueBuffers(source->source, 1, &buffer->buffer);
			source->listener->buffers[buffer->buffer] = buffer;
		}

		xnAudioBuffer* xnAudioSourceGetFreeBuffer(xnAudioSource* source)
		{
			ALuint buffer;
			//Context Lock
			{				
				ContextState lock(source->listener->context);
				ALint processed = 0;
				GetSourceI(source->source, AL_BUFFERS_PROCESSED, &processed);
				if (processed > 0)
				{				
					SourceUnqueueBuffers(source->source, 1, &buffer);					
				}
				else
				{
					return NULL;
				}
			}
			auto found = source->listener->buffers.find(buffer);
			if (found == source->listener->buffers.end()) return NULL;
			return found->second;
		}

		void xnAudioSourcePlay(xnAudioSource* source)
		{
			ContextState lock(source->listener->context);

			SourcePlay(source->source);
		}

		void xnAudioSourcePause(xnAudioSource* source)
		{
			ContextState lock(source->listener->context);

			SourcePause(source->source);
		}

		void xnAudioSourceStop(xnAudioSource* source)
		{
			ContextState lock(source->listener->context);

			SourceStop(source->source);
		}

		void xnAudioListenerPush3D(xnAudioListener* listener, float* pos, float* forward, float* up, float* vel)
		{
			ContextState lock(listener->context);

			if (forward && up)
			{
				float ori[6];
				ori[0] = forward[0];
				ori[1] = forward[1];
				ori[2] = -forward[2];
				ori[3] = up[0];
				ori[4] = up[1];
				ori[5] = -up[2];
				ListenerFV(AL_ORIENTATION, ori);
			}

			if (pos)
			{
				float pos2[3];
				pos2[0] = pos[0];
				pos2[1] = pos[1];
				pos2[2] = -pos[2];
				ListenerFV(AL_POSITION, pos2);
			}

			if (vel)
			{
				float vel2[3];
				vel2[0] = vel[0];
				vel2[1] = vel[1];
				vel2[2] = -vel[2];
				ListenerFV(AL_VELOCITY, vel2);
			}
		}

		void xnAudioSourcePush3D(xnAudioSource* source, float* pos, float* forward, float* up, float* vel)
		{
			ContextState lock(source->listener->context);

			if (forward && up)
			{
				float ori[6];
				ori[0] = forward[0];
				ori[1] = forward[1];
				ori[2] = -forward[2];
				ori[3] = up[0];
				ori[4] = up[1];
				ori[5] = -up[2];
				SourceFV(source->source, AL_ORIENTATION, ori);
			}

			if (pos)
			{
				float pos2[3];
				pos2[0] = pos[0];
				pos2[1] = pos[1];
				pos2[2] = -pos[2];
				SourceFV(source->source, AL_POSITION, pos2);
			}

			if (vel)
			{
				float vel2[3];
				vel2[0] = vel[0];
				vel2[1] = vel[1];
				vel2[2] = -vel[2];
				SourceFV(source->source, AL_VELOCITY, vel2);
			}
		}

		npBool xnAudioSourceIsPlaying(xnAudioSource* source)
		{
			ContextState lock(source->listener->context);

			ALint value;
			GetSourceI(source->source, AL_SOURCE_STATE, &value);
			return value == AL_PLAYING;
		}

		xnAudioBuffer* xnAudioBufferCreate(int maxBufferSize)
		{
			auto res = new xnAudioBuffer;
			GenBuffers(1, &res->buffer);
			return res;
		}

		void xnAudioBufferDestroy(xnAudioBuffer* buffer)
		{
			DeleteBuffers(1, &buffer->buffer);
			delete buffer;
		}

		void xnAudioBufferFill(xnAudioBuffer* buffer, short* pcm, int bufferSize, int sampleRate, npBool mono)
		{
			BufferData(buffer->buffer, mono ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, pcm, bufferSize, sampleRate);
		}

		void xnSleep(int milliseconds)
		{
			npThreadSleep(milliseconds);
		}
	}
}

#endif
