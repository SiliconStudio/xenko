// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/NativePath/NativeDynamicLinking.h"

#define HAVE_STDINT_H
#include "../../../deps/Celt/include/opus_custom.h"

extern "C" {
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
			int LoopStartPoint;
			int LoopEndPoint;
			int NumberOfLoops;
			bool IsInfiniteLoop;

			int CurrentFrame;
			int TotalNumberOfFrames;

			int NumberOfChannels;
			char* AudioDataBuffer;

			bool IsEnabled2D;
			bool IsEnabled3D;

			bool PlaybackEnded;

			AudioUnit HandleChannelMixer;
			AudioUnit Handle3DMixer;

			bool ShouldBeLooped() const
			{
				return IsInfiniteLoop || NumberOfLoops > 0;
			}

			void CopyDataToBuffer(char* &outBuffer, int nbFrameToCopy, int nbOfChannels)
			{
				char* inPtr = AudioDataBuffer + sizeof(short) * nbOfChannels * CurrentFrame;
				int sizeToCopy = sizeof(short) * nbFrameToCopy * nbOfChannels;

				memcpy(outBuffer, inPtr, sizeToCopy);

				CurrentFrame += nbFrameToCopy;
				outBuffer += sizeToCopy;
			}

			int AudioDataMixerCallback(uint32_t busIndex, int totalNbOfFrameToWrite, AudioBufferList* data)
			{
				char* outPtr = (char*)data->mBuffers[0].mData;

				int remainingFramesToWrite = totalNbOfFrameToWrite;
				while (remainingFramesToWrite > 0)
				{
					int nbOfFrameToWrite = fmin(remainingFramesToWrite, (ShouldBeLooped() ? LoopEndPoint : TotalNumberOfFrames) - CurrentFrame);

					CopyDataToBuffer(outPtr, nbOfFrameToWrite, NumberOfChannels);

					remainingFramesToWrite -= nbOfFrameToWrite;

					// Check if the track have to be re-looped
					if (ShouldBeLooped() && CurrentFrame >= LoopEndPoint)
					{
						--NumberOfLoops;
						CurrentFrame = LoopStartPoint;
					}

					// Check if we reached the end of the track.
					if (CurrentFrame >= TotalNumberOfFrames)
					{
						AudioUnitSetParameterFunc(HandleChannelMixer, kMultiChannelMixerParam_Enable, kAudioUnitScope_Input, busIndex, 0, 0);
						AudioUnitSetParameterFunc(Handle3DMixer, k3DMixerParam_Enable, kAudioUnitScope_Input, busIndex, 0, 0);

						IsEnabled2D = false;
						IsEnabled3D = false;

						PlaybackEnded = true;

						// Fill the rest of the buffer with blank
						int sizeToBlank = sizeof(short) * NumberOfChannels * remainingFramesToWrite;
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
		};

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

		static AURenderCallbackStruct NullRenderCallbackStruct = { NullRenderCallback, NULL };


		int SetInputRenderCallbackToChannelMixerDefault_(AudioUnit inUnit, uint32_t element, void* userData)
		{
			AURenderCallbackStruct pCallbackData = {};
			pCallbackData.inputProc = DefaultRenderCallbackChannelMixer;
			pCallbackData.inputProcRefCon = userData;

			int status = AudioUnitSetPropertyFunc(inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &pCallbackData, sizeof(AURenderCallbackStruct));

			return status;
		}

		int SetInputRenderCallbackTo3DMixerDefault_(AudioUnit inUnit, uint32_t element, void* userData)
		{
			AURenderCallbackStruct pCallbackData = {};
			pCallbackData.inputProc = DefaultRenderCallback3DMixer;
			pCallbackData.inputProcRefCon = userData;

			int status = AudioUnitSetPropertyFunc(inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &pCallbackData, sizeof(AURenderCallbackStruct));

			return status;
		}

		int SetInputRenderCallbackToNull_(AudioUnit inUnit, uint32_t element)
		{
			return AudioUnitSetPropertyFunc(inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &NullRenderCallbackStruct, sizeof(AURenderCallbackStruct));
		}

		bool XenkoAudioUnitHelpersInit()
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

	void* XenkoCeltCreate(int sampleRate, int bufferSize, int channels, bool decoderOnly)
	{
		auto celt = new XenkoCelt(sampleRate, bufferSize, channels, decoderOnly);
		if(!celt->Init())
		{
			delete celt;
			return nullptr;
		}
		return celt;
	}

	void XenkoCeltDestroy(XenkoCelt* celt)
	{
		delete celt;
	}

	int XenkoCeltEncodeFloat(XenkoCelt* celt, float* inputSamples, int numberOfInputSamples, uint8_t* outputBuffer, int maxOutputSize)
	{
		return opus_custom_encode_float(celt->GetEncoder(), inputSamples, numberOfInputSamples, outputBuffer, maxOutputSize);
	}

	int XenkoCeltDecodeFloat(XenkoCelt* celt, uint8_t* inputBuffer, int inputBufferSize, float* outputBuffer, int numberOfOutputSamples)
	{
		return opus_custom_decode_float(celt->GetDecoder(), inputBuffer, inputBufferSize, outputBuffer, numberOfOutputSamples);
	}

	int XenkoCeltEncodeShort(XenkoCelt* celt, int16_t* inputSamples, int numberOfInputSamples, uint8_t* outputBuffer, int maxOutputSize)
	{
		return opus_custom_encode(celt->GetEncoder(), inputSamples, numberOfInputSamples, outputBuffer, maxOutputSize);
	}

	int XenkoCeltDecodeShort(XenkoCelt* celt, uint8_t* inputBuffer, int inputBufferSize, int16_t* outputBuffer, int numberOfOutputSamples)
	{
		return opus_custom_decode(celt->GetDecoder(), inputBuffer, inputBufferSize, outputBuffer, numberOfOutputSamples);
	}

}
