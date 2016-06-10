// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../deps/NativePath/TINYSTL/vector.h"

#define HAVE_STDINT_H
#include "../../../deps/Celt/include/opus_custom.h"
#include "../../../deps/OpenAL/AL/al.h"
#include "../../../deps/OpenAL/AL/alc.h"

extern "C" {
	namespace OpenAL
	{
		LPALCOPENDEVICE OpenDevice;
		LPALCCLOSEDEVICE CloseDevice;
		LPALCCREATECONTEXT CreateContext;
		LPALCDESTROYCONTEXT DestroyContext;
		LPALCMAKECONTEXTCURRENT MakeContextCurrent;
		
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

		void* OpenALLibrary = NULL;

		bool xnInitOpenAL()
		{
			if (OpenALLibrary) return true;

			OpenALLibrary = LoadDynamicLibrary("OpenAL32");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x64\\OpenAL32");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x86\\OpenAL32");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x64/OpenAL32");
			if (!OpenALLibrary) OpenALLibrary = LoadDynamicLibrary("x86/OpenAL32");
			if (!OpenALLibrary) return false;

			OpenDevice = (LPALCOPENDEVICE)GetSymbolAddress(OpenALLibrary, "alcOpenDevice");
			CloseDevice = (LPALCCLOSEDEVICE)GetSymbolAddress(OpenALLibrary, "alcCloseDevice");
			CreateContext = (LPALCCREATECONTEXT)GetSymbolAddress(OpenALLibrary, "alcCreateContext");
			DestroyContext = (LPALCDESTROYCONTEXT)GetSymbolAddress(OpenALLibrary, "alcDestroyContext");
			MakeContextCurrent = (LPALCMAKECONTEXTCURRENT)GetSymbolAddress(OpenALLibrary, "alcMakeContextCurrent");

			SourcePlay = (LPALSOURCEPLAY)GetSymbolAddress(OpenALLibrary, "alSourcePlay");
			SourcePause = (LPALSOURCEPAUSE)GetSymbolAddress(OpenALLibrary, "alSourcePause");
			SourceStop = (LPALSOURCESTOP)GetSymbolAddress(OpenALLibrary, "alSourceStop");
			SourceF = (LPALSOURCEF)GetSymbolAddress(OpenALLibrary, "alSourcef");
			DeleteSources = (LPALDELETESOURCES)GetSymbolAddress(OpenALLibrary, "alDeleteSources");
			DeleteBuffers = (LPALDELETEBUFFERS)GetSymbolAddress(OpenALLibrary, "alDeleteBuffers");
			GenSources = (LPALGENSOURCES)GetSymbolAddress(OpenALLibrary, "alGenSources");
			GenBuffers = (LPALGENBUFFERS)GetSymbolAddress(OpenALLibrary, "alGenBuffers");
			Source3I = (LPALSOURCE3I)GetSymbolAddress(OpenALLibrary, "alSource3i");
			SourceI = (LPALSOURCEI)GetSymbolAddress(OpenALLibrary, "alSourcei"); 
			BufferData = (LPALBUFFERDATA)GetSymbolAddress(OpenALLibrary, "alBufferData");
			SourceQueueBuffers = (LPALSOURCEQUEUEBUFFERS)GetSymbolAddress(OpenALLibrary, "alSourceQueueBuffers"); 
			SourceUnqueueBuffers = (LPALSOURCEUNQUEUEBUFFERS)GetSymbolAddress(OpenALLibrary, "alSourceUnqueueBuffers");
			GetSourceI = (LPALGETSOURCEI)GetSymbolAddress(OpenALLibrary, "alGetSourcei");

			return true;
		}

		struct xnAudioDevice
		{
			ALCdevice* device;
			ALCcontext* context;
		};

		xnAudioDevice* xnAudioCreate(const char* deviceName)
		{
			auto o = new xnAudioDevice;
			o->device = OpenDevice(NULL);
			if(!o->device)
			{
				delete o;
				return NULL;
			}
			o->context = CreateContext(o->device, NULL);
			MakeContextCurrent(o->context);
			return o;
		}

		void xnAudioDestroy(xnAudioDevice* device)
		{
			MakeContextCurrent(NULL);
			DestroyContext(device->context);
			CloseDevice(device->device);
			delete device;
		}

		uint32_t xnAudioCreateVoice()
		{
			ALuint voice;
			GenSources(1, &voice);

			//this sets the voice as a normal stereo voice basically
			Source3I(voice, AL_POSITION, 0, 0, -1);
			SourceI(voice, AL_SOURCE_RELATIVE, AL_TRUE);

			return voice;
		}

		void xnAudioDestroyVoice(uint32_t voice)
		{
			DeleteSources(1, &voice);
		}

		uint32_t xnAudioCreateBuffer()
		{
			ALuint buffer;
			GenBuffers(1, &buffer);
			return buffer;
		}

		void xnAudioDestroyBuffer(uint32_t buffer)
		{
			DeleteBuffers(1, &buffer);
		}

		void xnAudioFillBuffer(uint32_t buffer, short* pcm, int bufferSize, int sampleRate, bool mono)
		{
			BufferData(buffer, mono ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, pcm, bufferSize, sampleRate);
		}

		void xnAudioSetVoiceBuffer(uint32_t voice, uint32_t buffer)
		{
			SourceI(voice, AL_BUFFER, buffer);
		}

		void xnAudioVoiceQueueBuffer(uint32_t voice, uint32_t buffer)
		{
			SourceQueueBuffers(voice, 1, &buffer);
		}

		uint32_t xnAudioVoiceGetFreeBuffer(uint32_t voice)
		{
			ALint processed;
			GetSourceI(voice, AL_BUFFERS_PROCESSED, &processed);
			if(processed > 0)
			{
				ALuint buffer;
				SourceUnqueueBuffers(voice, 1, &buffer);
				return buffer;
			}
			return 0;
		}

		void xnAudioPlay(uint32_t voice)
		{
			SourcePlay(voice);
		}

		void xnAudioPause(uint32_t voice)
		{
			SourcePause(voice);
		}

		void xnAudioStop(uint32_t voice)
		{
			SourceStop(voice);
		}
	}

	namespace iOS_Helpers
	{
		//all these types are just copy pasted from https://developer.apple.com/library/ios/documentation/AudioUnit/Reference/AudioUnitPropertiesReference/

		typedef int OSStatus;
		typedef void* AudioUnit;
		typedef int AudioUnitParameterID;
		typedef int AudioUnitScope;
		typedef int AudioUnitElement;
		typedef float AudioUnitParameterValue;
		typedef int AudioUnitPropertyID;

		enum {
			kAudioUnitRenderAction_PreRender = (1 << 2),
			kAudioUnitRenderAction_PostRender = (1 << 3),
			kAudioUnitRenderAction_OutputIsSilence = (1 << 4),
			kAudioOfflineUnitRenderAction_Preflight = (1 << 5),
			kAudioOfflineUnitRenderAction_Render = (1 << 6),
			kAudioOfflineUnitRenderAction_Complete = (1 << 7),
			kAudioUnitRenderAction_PostRenderError = (1 << 8),
			kAudioUnitRenderAction_DoNotCheckRenderArgs = (1 << 9)
		};
		typedef uint32_t AudioUnitRenderActionFlags;

		enum {
			kMultiChannelMixerParam_Volume = 0,
			kMultiChannelMixerParam_Enable = 1,
			kMultiChannelMixerParam_Pan = 2,
			kMultiChannelMixerParam_PreAveragePower = 1000,
			kMultiChannelMixerParam_PrePeakHoldLevel = 2000,
			kMultiChannelMixerParam_PostAveragePower = 3000,
			kMultiChannelMixerParam_PostPeakHoldLevel = 4000
		};

		enum {
			k3DMixerParam_Azimuth = 0,
			k3DMixerParam_Elevation = 1,
			k3DMixerParam_Distance = 2,
			k3DMixerParam_Gain = 3,
			k3DMixerParam_PlaybackRate = 4,
			k3DMixerParam_Enable = 5,
			k3DMixerParam_MinGain = 6,
			k3DMixerParam_MaxGain = 7,
			k3DMixerParam_ReverbBlend = 8,
			k3DMixerParam_GlobalReverbGain = 9,
			k3DMixerParam_OcclusionAttenuation = 10,
			k3DMixerParam_ObstructionAttenuation = 11
		};

		enum {
			kAudioUnitScope_Global = 0,
			kAudioUnitScope_Input = 1,
			kAudioUnitScope_Output = 2,
			kAudioUnitScope_Group = 3,
			kAudioUnitScope_Part = 4,
			kAudioUnitScope_Note = 5
		};

		enum {
			kAudioUnitProperty_ClassInfo = 0,
			kAudioUnitProperty_MakeConnection = 1,
			kAudioUnitProperty_SampleRate = 2,
			kAudioUnitProperty_ParameterList = 3,
			kAudioUnitProperty_ParameterInfo = 4,
			kAudioUnitProperty_StreamFormat = 8,
			kAudioUnitProperty_ElementCount = 11,
			kAudioUnitProperty_Latency = 12,
			kAudioUnitProperty_SupportedNumChannels = 13,
			kAudioUnitProperty_MaximumFramesPerSlice = 14,
			kAudioUnitProperty_AudioChannelLayout = 19,
			kAudioUnitProperty_TailTime = 20,
			kAudioUnitProperty_BypassEffect = 21,
			kAudioUnitProperty_LastRenderError = 22,
			kAudioUnitProperty_SetRenderCallback = 23,
			kAudioUnitProperty_FactoryPresets = 24,
			kAudioUnitProperty_RenderQuality = 26,
			kAudioUnitProperty_InPlaceProcessing = 29,
			kAudioUnitProperty_ElementName = 30,
			kAudioUnitProperty_SupportedChannelLayoutTags = 32,
			kAudioUnitProperty_PresentPreset = 36,
			kAudioUnitProperty_ShouldAllocateBuffer = 51,
			kAudioUnitProperty_ParameterHistoryInfo = 53,

			kAudioUnitProperty_CPULoad = 6,
			kAudioUnitProperty_ParameterValueStrings = 16,
			kAudioUnitProperty_ContextName = 25,
			kAudioUnitProperty_HostCallbacks = 27,
			kAudioUnitProperty_ParameterStringFromValue = 33,
			kAudioUnitProperty_ParameterIDName = 34,
			kAudioUnitProperty_ParameterClumpName = 35,
			kAudioUnitProperty_OfflineRender = 37,
			kAudioUnitProperty_ParameterValueFromString = 38,
			kAudioUnitProperty_PresentationLatency = 40,
			kAudioUnitProperty_DependentParameters = 45,
			kAudioUnitProperty_InputSamplesInOutput = 49,
			kAudioUnitProperty_ClassInfoFromDocument = 50,
			kAudioUnitProperty_FrequencyResponse = 52
		};

		struct AudioBuffer { uint32_t mNumberChannels; uint32_t mDataByteSize; void *mData; };
		typedef struct AudioBuffer AudioBuffer;

		struct AudioBufferList { uint32_t mNumberBuffers; AudioBuffer mBuffers[1]; }; 
		typedef struct AudioBufferList AudioBufferList;

		struct SMPTETime { int16_t mSubframes; int16_t mSubframeDivisor; uint32_t mCounter; uint32_t mType; uint32_t mFlags; int16_t mHours; int16_t mMinutes; int16_t mSeconds; int16_t mFrames; };
		typedef struct SMPTETime SMPTETime;

		struct AudioTimeStamp { double mSampleTime; uint64_t mHostTime; double mRateScalar; uint64_t mWordClockTime; SMPTETime mSMPTETime; uint32_t mFlags; uint32_t mReserved; };
		typedef struct AudioTimeStamp AudioTimeStamp;

		typedef OSStatus (*AudioUnitSetParameterPtr)(AudioUnit inUnit, AudioUnitParameterID inID, AudioUnitScope inScope, AudioUnitElement inElement, AudioUnitParameterValue inValue, int inBufferOffsetInFrames);
		AudioUnitSetParameterPtr AudioUnitSetParameterFunc;

		typedef OSStatus (*AudioUnitSetPropertyPtr)(AudioUnit inUnit, AudioUnitPropertyID inID, AudioUnitScope inScope, AudioUnitElement inElement, const void *inData, uint32_t inDataSize);
		AudioUnitSetPropertyPtr AudioUnitSetPropertyFunc;

		typedef OSStatus (*AURenderCallback)(void *inRefCon, AudioUnitRenderActionFlags *ioActionFlags, const AudioTimeStamp *inTimeStamp, uint32_t inBusNumber, uint32_t inNumberFrames, AudioBufferList *ioData);
		typedef struct AURenderCallbackStruct { AURenderCallback inputProc; void *inputProcRefCon; } AURenderCallbackStruct;

		struct AudioDataRenderer
		{
			struct xnAudioBuffer
			{
				short* data;
				int frames;
				int currentFrame;
				int channels;
			};

			//careful this is mirrored in the c# struct
			int LoopStartPoint;
			int LoopEndPoint;
			int NumberOfLoops;
			bool IsInfiniteLoop;

			bool IsEnabled2D;
			bool IsEnabled3D;

			bool PlaybackEnded;

			AudioUnit HandleChannelMixer;
			AudioUnit Handle3DMixer;
			//end of c# struct

			int bufferIndex = 0;

			static OSStatus NullRenderCallback(void                        *inRefCon,
				AudioUnitRenderActionFlags  *ioActionFlags,
				const AudioTimeStamp        *inTimeStamp,
				uint32_t                      inBusNumber,
				uint32_t                      inNumberFrames,
				AudioBufferList             *ioData)
			{
				memset(ioData->mBuffers[0].mData, 0x0, ioData->mBuffers[0].mDataByteSize);

				return 0;
			}

			static OSStatus DefaultRenderCallbackChannelMixer(void                        *inRefCon,
				AudioUnitRenderActionFlags  *ioActionFlags,
				const AudioTimeStamp        *inTimeStamp,
				uint32_t                      inBusNumber,
				uint32_t                      inNumberFrames,
				AudioBufferList             *ioData)
			{
				return ((AudioDataRenderer*)inRefCon)->RendererCallbackChannelMixer(inBusNumber, inNumberFrames, ioData);
			}

			static OSStatus DefaultRenderCallback3DMixer(void                        *inRefCon,
				AudioUnitRenderActionFlags  *ioActionFlags,
				const AudioTimeStamp        *inTimeStamp,
				uint32_t                      inBusNumber,
				uint32_t                      inNumberFrames,
				AudioBufferList             *ioData)
			{
				return ((AudioDataRenderer*)inRefCon)->RendererCallback3DMixer(inBusNumber, inNumberFrames, ioData);
			}

			tinystl::vector<xnAudioBuffer> AudioDataBuffers;

			bool ShouldBeLooped() const
			{
				return IsInfiniteLoop || NumberOfLoops > 0;
			}

			int AudioDataMixerCallback(uint32_t busIndex, int totalNbOfFrameToWrite, AudioBufferList* data)
			{
				char* outPtr = (char*)data->mBuffers[0].mData;
				xnAudioBuffer& currentBuffer = AudioDataBuffers[bufferIndex];

				int remainingFramesToWrite = totalNbOfFrameToWrite;
				while (remainingFramesToWrite > 0)
				{
					int nbOfFrameToWrite = fmin(remainingFramesToWrite, (ShouldBeLooped() ? LoopEndPoint : currentBuffer.frames) - currentBuffer.currentFrame);

					short* inPtr = currentBuffer.data + currentBuffer.channels * currentBuffer.currentFrame;
					int sizeToCopy = sizeof(short) * nbOfFrameToWrite * currentBuffer.channels;

					memcpy(outPtr, inPtr, sizeToCopy);

					currentBuffer.currentFrame += nbOfFrameToWrite;
					outPtr += sizeToCopy;

					remainingFramesToWrite -= nbOfFrameToWrite;

					// Check if the track have to be re-looped
					if (ShouldBeLooped() && currentBuffer.currentFrame >= LoopEndPoint)
					{
						--NumberOfLoops;
						currentBuffer.currentFrame = LoopStartPoint;
					}

					// Check if we reached the end of the track.
					if (currentBuffer.currentFrame >= currentBuffer.frames)
					{
						AudioUnitSetParameterFunc(HandleChannelMixer, kMultiChannelMixerParam_Enable, kAudioUnitScope_Input, busIndex, 0, 0);
						AudioUnitSetParameterFunc(Handle3DMixer, k3DMixerParam_Enable, kAudioUnitScope_Input, busIndex, 0, 0);

						IsEnabled2D = false;
						IsEnabled3D = false;

						PlaybackEnded = true;

						// Fill the rest of the buffer with blank
						int sizeToBlank = sizeof(short) * currentBuffer.channels * remainingFramesToWrite;
						memset(outPtr, 0x0 , sizeToBlank);

						return 0;
					}
				}

				return 0;
			}

			OSStatus RendererCallbackChannelMixer(uint32_t busNumber, uint32_t numberFrames, AudioBufferList* data)
			{
				if (!IsEnabled2D)
					return 0;

				OSStatus ret = AudioDataMixerCallback(busNumber, (int)numberFrames, data);

				return ret;
			}

			OSStatus RendererCallback3DMixer(uint32_t busNumber, uint32_t numberFrames, AudioBufferList* data)
			{
				if (!IsEnabled3D)
					return 0;

				return AudioDataMixerCallback(busNumber, (int)numberFrames, data);
			}

			void AddBuffer(short* audioBuffer, int channels, int nframes)
			{
				xnAudioBuffer buffer = {};
				buffer.data = audioBuffer;
				buffer.frames = nframes;
				buffer.channels = channels;
				buffer.currentFrame = 0;
				AudioDataBuffers.push_back(buffer);
			}
		};

		static AURenderCallbackStruct NullRenderCallbackStruct = { AudioDataRenderer::NullRenderCallback, NULL };

		AudioDataRenderer* xnCreateAudioDataRenderer()
		{
			return new AudioDataRenderer();
		}

		void xnDestroyAudioDataRenderer(AudioDataRenderer* ptr)
		{
			delete ptr;
		}

		void xnAddAudioBuffer(AudioDataRenderer* renderer, short* buffer, int channels, int nframes)
		{
			renderer->AddBuffer(buffer, channels, nframes);
		}

		void xnSetAudioBufferFrame(AudioDataRenderer* renderer, int bufferIndex, int frame)
		{
			if (bufferIndex >= renderer->AudioDataBuffers.size()) return;
			renderer->AudioDataBuffers[bufferIndex].currentFrame = frame;
		}

		int xnSetInputRenderCallbackToChannelMixerDefault(AudioUnit inUnit, uint32_t element, void* userData)
		{
			AURenderCallbackStruct pCallbackData = {};
			pCallbackData.inputProc = AudioDataRenderer::DefaultRenderCallbackChannelMixer;
			pCallbackData.inputProcRefCon = userData;

			int status = AudioUnitSetPropertyFunc(inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &pCallbackData, sizeof(AURenderCallbackStruct));

			return status;
		}

		int xnSetInputRenderCallbackTo3DMixerDefault(AudioUnit inUnit, uint32_t element, void* userData)
		{
			AURenderCallbackStruct pCallbackData = {};
			pCallbackData.inputProc = AudioDataRenderer::DefaultRenderCallback3DMixer;
			pCallbackData.inputProcRefCon = userData;

			int status = AudioUnitSetPropertyFunc(inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &pCallbackData, sizeof(AURenderCallbackStruct));

			return status;
		}

		int xnSetInputRenderCallbackToNull(AudioUnit inUnit, uint32_t element)
		{
			return AudioUnitSetPropertyFunc(inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &NullRenderCallbackStruct, sizeof(AURenderCallbackStruct));
		}

		bool xnAudioUnitHelpersInit()
		{
			auto exe = LoadDynamicLibrary(NULL);
			if (!exe) return false;

			AudioUnitSetParameterFunc = AudioUnitSetParameterPtr(GetSymbolAddress(exe, "AudioUnitSetParameter"));
			if (!AudioUnitSetParameterFunc) return false;

			AudioUnitSetPropertyFunc = AudioUnitSetPropertyPtr(GetSymbolAddress(exe, "AudioUnitSetProperty"));
			if (!AudioUnitSetPropertyFunc) return false;

			return true;
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
