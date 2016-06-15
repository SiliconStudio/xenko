// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/NativePath/NativeDynamicLinking.h"

#define HAVE_STDINT_H
#include "../../../deps/Celt/include/opus_custom.h"
#include "../../../deps/OpenAL/AL/al.h"
#include "../../../deps/OpenAL/AL/alc.h"

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

		bool xnInitOpenAL()
		{
			if (OpenALLibrary) return true;

			OpenALLibrary = LoadDynamicLibrary("OpenAL");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x64\\OpenAL");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x86\\OpenAL");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x64/OpenAL");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x86/OpenAL");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("/System/Library/Frameworks/OpenAL.framework/OpenAL"); //iOS Apple OpenAL
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

		struct xnAudioDevice
		{
			ALCdevice* device;
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

		struct xnAudioListener
		{
			ALCcontext* context;
		};

		xnAudioListener* xnAudioListenerCreate(xnAudioDevice* device)
		{
			auto res = new xnAudioListener;
			res->context = CreateContext(device->device, NULL);
			ALC_ERROR(device->device);
			if (!MakeContextCurrent(res->context))
				debugtrap();
			ProcessContext(res->context);
			ALC_ERROR(device->device);
			return res;
		}

		void xnAudioListenerDestroy(xnAudioListener* listener)
		{
			DestroyContext(listener->context);
		}

		bool xnAudioListenerEnable(xnAudioListener* listener)
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

		uint32_t xnAudioSourceCreate(xnAudioListener* listener)
		{
			ContextState lock(listener->context);

			ALuint source;
			GenSources(1, &source);
			AL_ERROR;
			SourceF(source, AL_REFERENCE_DISTANCE, 1.0f);
			AL_ERROR;
						
			return source;
		}

		void xnAudioSourceDestroy(xnAudioListener* listener, uint32_t source)
		{
			ContextState lock(listener->context);

			DeleteSources(1, &source);
			AL_ERROR;
		}

		void xnAudioSourceSetPan(xnAudioListener* listener, uint32_t source, float pan)
		{
			ContextState lock(listener->context);

			//make sure we are able to pan
			SourceI(source, AL_SOURCE_RELATIVE, AL_TRUE);

			auto clampedPan = pan > 1.0f ? 1.0f : pan < -1.0f ? -1.0f : pan;
			ALfloat alpan[3];
			alpan[0] = clampedPan; // from -1 (left) to +1 (right) 
			alpan[1] = sqrt(1.0f - clampedPan*clampedPan);
			alpan[2] = 0.0f;
			SourceFV(source, AL_POSITION, alpan);
		}

		void xnAudioSourceSetLooping(xnAudioListener* listener, uint32_t source, bool looping)
		{
			ContextState lock(listener->context);

			SourceI(source, AL_LOOPING, looping ? AL_TRUE : AL_FALSE);
		}

		void xnAudioSourceSetGain(xnAudioListener* listener, uint32_t source, float gain)
		{
			ContextState lock(listener->context);

			SourceF(source, AL_GAIN, gain);
		}

		void xnAudioSourceSetPitch(xnAudioListener* listener, uint32_t source, float pitch)
		{
			ContextState lock(listener->context);

			SourceF(source, AL_PITCH, pitch);
		}

		void xnAudioSourceSetBuffer(xnAudioListener* listener, uint32_t source, uint32_t buffer)
		{
			ContextState lock(listener->context);

			SourceI(source, AL_BUFFER, buffer);
		}

		void xnAudioSourceQueueBuffer(xnAudioListener* listener, uint32_t source, uint32_t buffer, short* pcm, int bufferSize, int sampleRate, bool mono)
		{
			ContextState lock(listener->context);

			BufferData(buffer, mono ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, pcm, bufferSize, sampleRate);
			SourceQueueBuffers(source, 1, &buffer);
		}

		uint32_t xnAudioSourceGetFreeBuffer(xnAudioListener* listener, uint32_t source)
		{
			ContextState lock(listener->context);

			ALint processed = 0;
			GetSourceI(source, AL_BUFFERS_PROCESSED, &processed);
			if(processed > 0)
			{
				ALuint buffer;
				SourceUnqueueBuffers(source, 1, &buffer);
				return buffer;
			}
			return 0;
		}

		void xnAudioSourcePlay(xnAudioListener* listener, uint32_t source)
		{
			ContextState lock(listener->context);

			SourcePlay(source);
		}

		void xnAudioSourcePause(xnAudioListener* listener, uint32_t source)
		{
			ContextState lock(listener->context);

			SourcePause(source);
		}

		void xnAudioSourceStop(xnAudioListener* listener, uint32_t source)
		{
			ContextState lock(listener->context);

			SourceStop(source);
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

		void xnAudioSourcePush3D(xnAudioListener* listener, uint32_t source, float* pos, float* forward, float* up, float* vel)
		{
			ContextState lock(listener->context);

			//make sure we are able to 3D
			SourceI(source, AL_SOURCE_RELATIVE, AL_FALSE);

			if (forward && up)
			{
				float ori[6];
				ori[0] = forward[0];
				ori[1] = forward[1];
				ori[2] = -forward[2];
				ori[3] = up[0];
				ori[4] = up[1];
				ori[5] = -up[2];
				SourceFV(source, AL_ORIENTATION, ori);
			}

			if (pos)
			{
				float pos2[3];
				pos2[0] = pos[0];
				pos2[1] = pos[1];
				pos2[2] = -pos[2];
				SourceFV(source, AL_POSITION, pos2);
			}

			if (vel)
			{
				float vel2[3];
				vel2[0] = vel[0];
				vel2[1] = vel[1];
				vel2[2] = -vel[2];
				SourceFV(source, AL_VELOCITY, vel2);
			}
		}

		bool xnAudioSourceIsPlaying(xnAudioListener* listener, uint32_t source)
		{
			ContextState lock(listener->context);

			ALint value;
			GetSourceI(source, AL_SOURCE_STATE, &value);
			return value == AL_PLAYING;
		}

		uint32_t xnAudioBufferCreate()
		{
			ALuint buffer;
			GenBuffers(1, &buffer);
			return buffer;
		}

		void xnAudioBufferDestroy(uint32_t buffer)
		{
			DeleteBuffers(1, &buffer);
		}

		void xnAudioBufferFill(uint32_t buffer, short* pcm, int bufferSize, int sampleRate, bool mono)
		{
			BufferData(buffer, mono ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, pcm, bufferSize, sampleRate);
		}
	}

	class XenkoCelt
	{
	public:
		XenkoCelt(int sampleRate, int bufferSize, int channels, bool decoderOnly): mode_(nullptr), decoder_(nullptr), encoder_(nullptr), sample_rate_(sampleRate), buffer_size_(bufferSize), channels_(channels), decoder_only_(decoderOnly)
		{
		}

		~XenkoCelt()
		{
			if (encoder_) opus_custom_encoder_destroy(encoder_);
			encoder_ = nullptr;
			if (decoder_) opus_custom_decoder_destroy(decoder_);
			decoder_ = nullptr;
			if (mode_) opus_custom_mode_destroy(mode_);
			mode_ = nullptr;
		}

		bool Init()
		{
			mode_ = opus_custom_mode_create(sample_rate_, buffer_size_, nullptr);
			if (!mode_) return false;

			decoder_ = opus_custom_decoder_create(mode_, channels_, nullptr);
			if (!decoder_) return false;

			if(!decoder_only_)
			{
				encoder_ = opus_custom_encoder_create(mode_, channels_, nullptr);
				if (!encoder_) return false;
			}

			return true;
		}

		OpusCustomEncoder* GetEncoder() const
		{
			return encoder_;
		}

		OpusCustomDecoder* GetDecoder() const
		{
			return decoder_;
		}

	private:
		OpusCustomMode* mode_;
		OpusCustomDecoder* decoder_;
		OpusCustomEncoder* encoder_;
		int sample_rate_;
		int buffer_size_;
		int channels_;
		bool decoder_only_;
	};

	void* xnCeltCreate(int sampleRate, int bufferSize, int channels, bool decoderOnly)
	{
		auto celt = new XenkoCelt(sampleRate, bufferSize, channels, decoderOnly);
		if(!celt->Init())
		{
			delete celt;
			return nullptr;
		}
		return celt;
	}

	void xnCeltDestroy(XenkoCelt* celt)
	{
		delete celt;
	}

	int xnCeltEncodeFloat(XenkoCelt* celt, float* inputSamples, int numberOfInputSamples, uint8_t* outputBuffer, int maxOutputSize)
	{
		return opus_custom_encode_float(celt->GetEncoder(), inputSamples, numberOfInputSamples, outputBuffer, maxOutputSize);
	}

	int xnCeltDecodeFloat(XenkoCelt* celt, uint8_t* inputBuffer, int inputBufferSize, float* outputBuffer, int numberOfOutputSamples)
	{
		return opus_custom_decode_float(celt->GetDecoder(), inputBuffer, inputBufferSize, outputBuffer, numberOfOutputSamples);
	}

	int xnCeltEncodeShort(XenkoCelt* celt, int16_t* inputSamples, int numberOfInputSamples, uint8_t* outputBuffer, int maxOutputSize)
	{
		return opus_custom_encode(celt->GetEncoder(), inputSamples, numberOfInputSamples, outputBuffer, maxOutputSize);
	}

	int xnCeltDecodeShort(XenkoCelt* celt, uint8_t* inputBuffer, int inputBufferSize, int16_t* outputBuffer, int numberOfOutputSamples)
	{
		return opus_custom_decode(celt->GetDecoder(), inputBuffer, inputBufferSize, outputBuffer, numberOfOutputSamples);
	}

}
