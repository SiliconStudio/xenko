// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(WINDOWS_DESKTOP) || defined(WINDOWS_UWP) || defined(WINDOWS_STORE) || defined(WINDOWS_PHONE) || !defined(__clang__)

#include "../../../deps/NativePath/NativePath.h"
#include "../../../deps/NativePath/TINYSTL/unordered_map.h"

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

	namespace XAudio2
	{
		typedef struct _GUID {
			unsigned long  Data1;
			unsigned short Data2;
			unsigned short Data3;
			unsigned char  Data4[8];
		} GUID;

		typedef GUID IID;

		typedef unsigned long       DWORD;
		typedef int                 BOOL;
		typedef unsigned char       BYTE;
		typedef unsigned short      WORD;
		typedef float               FLOAT;
		typedef FLOAT               *PFLOAT;
		typedef int                 INT;
		typedef unsigned int        UINT;
		typedef unsigned int        *PUINT;

#define REFIID const IID &
#define HRESULT long
#define UINT32 unsigned int 
#define ULONG unsigned long
#define THIS_
#define THIS void
#define PURE = 0;
#define _Outptr_
#define _In_
#define _In_opt_
#define _Out_
#define X2DEFAULT(x) =x
#define _Reserved_
#define _In_reads_bytes_(_xxx_)
#define _Out_writes_bytes_(_xxx_)
#define _Out_writes_(_xxx_)
#define _In_reads_(_xxx_)
#define STDMETHOD(method) virtual HRESULT __stdcall method
#define STDMETHOD_(type,method) virtual type __stdcall method
#define IUnknown void
#define BOOL bool
#define XAUDIO2_COMMIT_NOW              0             // Used as an OperationSet argument
#define XAUDIO2_COMMIT_ALL              0             // Used in IXAudio2::CommitChanges
#define XAUDIO2_INVALID_OPSET           (UINT32)(-1)  // Not allowed for OperationSet arguments
#define XAUDIO2_NO_LOOP_REGION          0             // Used in XAUDIO2_BUFFER.LoopCount
#define XAUDIO2_LOOP_INFINITE           255           // Used in XAUDIO2_BUFFER.LoopCount
#define XAUDIO2_DEFAULT_CHANNELS        0             // Used in CreateMasteringVoice
#define XAUDIO2_DEFAULT_SAMPLERATE      0             // Used in CreateMasteringVoice
#define BYTE char
#define UINT64 unsigned __int64 
#define _In_opt_z_
#define FAILED(hr) (((HRESULT)(hr)) < 0)

		extern long __stdcall XAudio2Create(void** ppXAudio2, UINT32 flags, UINT32 processor);
		extern long __stdcall CoInitializeEx(void* ppXAudio2, DWORD dwCoInit);

		struct IXAudio2Voice;

		typedef struct XAUDIO2_VOICE_DETAILS
		{
			UINT32 CreationFlags;               // Flags the voice was created with.
			UINT32 ActiveFlags;                 // Flags currently active.
			UINT32 InputChannels;               // Channels in the voice's input audio.
			UINT32 InputSampleRate;             // Sample rate of the voice's input audio.
		} XAUDIO2_VOICE_DETAILS;

		typedef struct XAUDIO2_SEND_DESCRIPTOR
		{
			UINT32 Flags;                       // Either 0 or XAUDIO2_SEND_USEFILTER.
			IXAudio2Voice* pOutputVoice;        // This send's destination voice.
		} XAUDIO2_SEND_DESCRIPTOR;

		typedef struct XAUDIO2_VOICE_SENDS
		{
			UINT32 SendCount;                   // Number of sends from this voice.
			XAUDIO2_SEND_DESCRIPTOR* pSends;    // Array of SendCount send descriptors.
		} XAUDIO2_VOICE_SENDS;

		typedef struct XAUDIO2_EFFECT_DESCRIPTOR
		{
			IUnknown* pEffect;                  // Pointer to the effect object's IUnknown interface.
			BOOL InitialState;                  // TRUE if the effect should begin in the enabled state.
			UINT32 OutputChannels;              // How many output channels the effect should produce.
		} XAUDIO2_EFFECT_DESCRIPTOR;

		typedef struct XAUDIO2_EFFECT_CHAIN
		{
			UINT32 EffectCount;                 // Number of effects in this voice's effect chain.
			XAUDIO2_EFFECT_DESCRIPTOR* pEffectDescriptors; // Array of effect descriptors.
		} XAUDIO2_EFFECT_CHAIN;

		typedef enum XAUDIO2_FILTER_TYPE
		{
			LowPassFilter,                      // Attenuates frequencies above the cutoff frequency (state-variable filter).
			BandPassFilter,                     // Attenuates frequencies outside a given range      (state-variable filter).
			HighPassFilter,                     // Attenuates frequencies below the cutoff frequency (state-variable filter).
			NotchFilter,                        // Attenuates frequencies inside a given range       (state-variable filter).
			LowPassOnePoleFilter,               // Attenuates frequencies above the cutoff frequency (one-pole filter, XAUDIO2_FILTER_PARAMETERS.OneOverQ has no effect)
			HighPassOnePoleFilter               // Attenuates frequencies below the cutoff frequency (one-pole filter, XAUDIO2_FILTER_PARAMETERS.OneOverQ has no effect)
		} XAUDIO2_FILTER_TYPE;

		typedef struct XAUDIO2_FILTER_PARAMETERS
		{
			XAUDIO2_FILTER_TYPE Type;           // Filter type.
			float Frequency;                    // Filter coefficient.
												//  must be >= 0 and <= XAUDIO2_MAX_FILTER_FREQUENCY
												//  See XAudio2CutoffFrequencyToRadians() for state-variable filter types and
												//  XAudio2CutoffFrequencyToOnePoleCoefficient() for one-pole filter types.
			float OneOverQ;                     // Reciprocal of the filter's quality factor Q;
												//  must be > 0 and <= XAUDIO2_MAX_FILTER_ONEOVERQ.
												//  Has no effect for one-pole filters.
		} XAUDIO2_FILTER_PARAMETERS;

		struct IXAudio2EngineCallback
		{
			// Called by XAudio2 just before an audio processing pass begins.
			STDMETHOD_(void, OnProcessingPassStart) (THIS) PURE;

			// Called just after an audio processing pass ends.
			STDMETHOD_(void, OnProcessingPassEnd) (THIS) PURE;

			// Called in the event of a critical system error which requires XAudio2
			// to be closed down and restarted.  The error code is given in Error.
			STDMETHOD_(void, OnCriticalError) (THIS_ HRESULT Error) PURE;
		};

		struct IXAudio2Voice
		{
			/* NAME: IXAudio2Voice::GetVoiceDetails
			// DESCRIPTION: Returns the basic characteristics of this voice.
			//
			// ARGUMENTS:
			//  pVoiceDetails - Returns the voice's details.
			*/
			STDMETHOD_(void, GetVoiceDetails) (THIS_ _Out_ XAUDIO2_VOICE_DETAILS* pVoiceDetails) PURE; 
			
			/* NAME: IXAudio2Voice::SetOutputVoices
			// DESCRIPTION: Replaces the set of submix/mastering voices that receive
			//              this voice's output.
			//
			// ARGUMENTS:
			//  pSendList - Optional list of voices this voice should send audio to.
			*/
			STDMETHOD(SetOutputVoices) (THIS_ _In_opt_ const XAUDIO2_VOICE_SENDS* pSendList) PURE; 
			
			/* NAME: IXAudio2Voice::SetEffectChain
			// DESCRIPTION: Replaces this voice's current effect chain with a new one.
			//
			// ARGUMENTS:
			//  pEffectChain - Structure describing the new effect chain to be used.
			*/
			STDMETHOD(SetEffectChain) (THIS_ _In_opt_ const XAUDIO2_EFFECT_CHAIN* pEffectChain) PURE; 
			
			/* NAME: IXAudio2Voice::EnableEffect
			// DESCRIPTION: Enables an effect in this voice's effect chain.
			//
			// ARGUMENTS:
			//  EffectIndex - Index of an effect within this voice's effect chain.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(EnableEffect) (THIS_ UINT32 EffectIndex, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::DisableEffect
			// DESCRIPTION: Disables an effect in this voice's effect chain.
			//
			// ARGUMENTS:
			//  EffectIndex - Index of an effect within this voice's effect chain.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(DisableEffect) (THIS_ UINT32 EffectIndex, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::GetEffectState
			// DESCRIPTION: Returns the running state of an effect.
			//
			// ARGUMENTS:
			//  EffectIndex - Index of an effect within this voice's effect chain.
			//  pEnabled - Returns the enabled/disabled state of the given effect.
			*/
			STDMETHOD_(void, GetEffectState) (THIS_ UINT32 EffectIndex, _Out_ BOOL* pEnabled) PURE; 
			
			/* NAME: IXAudio2Voice::SetEffectParameters
			// DESCRIPTION: Sets effect-specific parameters.
			//
			// REMARKS: Unlike IXAPOParameters::SetParameters, this method may
			//          be called from any thread.  XAudio2 implements
			//          appropriate synchronization to copy the parameters to the
			//          realtime audio processing thread.
			//
			// ARGUMENTS:
			//  EffectIndex - Index of an effect within this voice's effect chain.
			//  pParameters - Pointer to an effect-specific parameters block.
			//  ParametersByteSize - Size of the pParameters array  in bytes.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(SetEffectParameters) (THIS_ UINT32 EffectIndex, 
				_In_reads_bytes_(ParametersByteSize) const void* pParameters, 
				UINT32 ParametersByteSize, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::GetEffectParameters
			// DESCRIPTION: Obtains the current effect-specific parameters.
			//
			// ARGUMENTS:
			//  EffectIndex - Index of an effect within this voice's effect chain.
			//  pParameters - Returns the current values of the effect-specific parameters.
			//  ParametersByteSize - Size of the pParameters array in bytes.
			*/
			STDMETHOD(GetEffectParameters) (THIS_ UINT32 EffectIndex, 
				_Out_writes_bytes_(ParametersByteSize) void* pParameters, 
				UINT32 ParametersByteSize) PURE; 
			
			/* NAME: IXAudio2Voice::SetFilterParameters
			// DESCRIPTION: Sets this voice's filter parameters.
			//
			// ARGUMENTS:
			//  pParameters - Pointer to the filter's parameter structure.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(SetFilterParameters) (THIS_ _In_ const XAUDIO2_FILTER_PARAMETERS* pParameters, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::GetFilterParameters
			// DESCRIPTION: Returns this voice's current filter parameters.
			//
			// ARGUMENTS:
			//  pParameters - Returns the filter parameters.
			*/
			STDMETHOD_(void, GetFilterParameters) (THIS_ _Out_ XAUDIO2_FILTER_PARAMETERS* pParameters) PURE; 
			
			/* NAME: IXAudio2Voice::SetOutputFilterParameters
			// DESCRIPTION: Sets the filter parameters on one of this voice's sends.
			//
			// ARGUMENTS:
			//  pDestinationVoice - Destination voice of the send whose filter parameters will be set.
			//  pParameters - Pointer to the filter's parameter structure.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(SetOutputFilterParameters) (THIS_ _In_opt_ IXAudio2Voice* pDestinationVoice, 
				_In_ const XAUDIO2_FILTER_PARAMETERS* pParameters, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::GetOutputFilterParameters
			// DESCRIPTION: Returns the filter parameters from one of this voice's sends.
			//
			// ARGUMENTS:
			//  pDestinationVoice - Destination voice of the send whose filter parameters will be read.
			//  pParameters - Returns the filter parameters.
			*/
			STDMETHOD_(void, GetOutputFilterParameters) (THIS_ _In_opt_ IXAudio2Voice* pDestinationVoice, 
				_Out_ XAUDIO2_FILTER_PARAMETERS* pParameters) PURE; 
			
			/* NAME: IXAudio2Voice::SetVolume
			// DESCRIPTION: Sets this voice's overall volume level.
			//
			// ARGUMENTS:
			//  Volume - New overall volume level to be used, as an amplitude factor.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(SetVolume) (THIS_ float Volume, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::GetVolume
			// DESCRIPTION: Obtains this voice's current overall volume level.
			//
			// ARGUMENTS:
			//  pVolume: Returns the voice's current overall volume level.
			*/
			STDMETHOD_(void, GetVolume) (THIS_ _Out_ float* pVolume) PURE; 
			
			/* NAME: IXAudio2Voice::SetChannelVolumes
			// DESCRIPTION: Sets this voice's per-channel volume levels.
			//
			// ARGUMENTS:
			//  Channels - Used to confirm the voice's channel count.
			//  pVolumes - Array of per-channel volume levels to be used.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(SetChannelVolumes) (THIS_ UINT32 Channels, _In_reads_(Channels) const float* pVolumes, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; \
			
			/* NAME: IXAudio2Voice::GetChannelVolumes
			// DESCRIPTION: Returns this voice's current per-channel volume levels.
			//
			// ARGUMENTS:
			//  Channels - Used to confirm the voice's channel count.
			//  pVolumes - Returns an array of the current per-channel volume levels.
			*/
			STDMETHOD_(void, GetChannelVolumes) (THIS_ UINT32 Channels, _Out_writes_(Channels) float* pVolumes) PURE; 
			
			/* NAME: IXAudio2Voice::SetOutputMatrix
			// DESCRIPTION: Sets the volume levels used to mix from each channel of this
			//              voice's output audio to each channel of a given destination
			//              voice's input audio.
			//
			// ARGUMENTS:
			//  pDestinationVoice - The destination voice whose mix matrix to change.
			//  SourceChannels - Used to confirm this voice's output channel count
			//   (the number of channels produced by the last effect in the chain).
			//  DestinationChannels - Confirms the destination voice's input channels.
			//  pLevelMatrix - Array of [SourceChannels * DestinationChannels] send
			//   levels.  The level used to send from source channel S to destination
			//   channel D should be in pLevelMatrix[S + SourceChannels * D].
			//  OperationSet - Used to identify this call as part of a deferred batch.
			*/
			STDMETHOD(SetOutputMatrix) (THIS_ _In_opt_ IXAudio2Voice* pDestinationVoice, 
				UINT32 SourceChannels, UINT32 DestinationChannels, 
				_In_reads_(SourceChannels * DestinationChannels) const float* pLevelMatrix, 
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE; 
			
			/* NAME: IXAudio2Voice::GetOutputMatrix
			// DESCRIPTION: Obtains the volume levels used to send each channel of this
			//              voice's output audio to each channel of a given destination
			//              voice's input audio.
			//
			// ARGUMENTS:
			//  pDestinationVoice - The destination voice whose mix matrix to obtain.
			//  SourceChannels - Used to confirm this voice's output channel count
			//   (the number of channels produced by the last effect in the chain).
			//  DestinationChannels - Confirms the destination voice's input channels.
			//  pLevelMatrix - Array of send levels, as above.
			*/
			STDMETHOD_(void, GetOutputMatrix) (THIS_ _In_opt_ IXAudio2Voice* pDestinationVoice,
				UINT32 SourceChannels, UINT32 DestinationChannels,
				_Out_writes_(SourceChannels * DestinationChannels) float* pLevelMatrix) PURE;
			/* NAME: IXAudio2Voice::DestroyVoice
			// DESCRIPTION: Destroys this voice, stopping it if necessary and removing
			//              it from the XAudio2 graph.
			*/
			STDMETHOD_(void, DestroyVoice) (THIS) PURE;
		};

		typedef struct XAUDIO2_BUFFER
		{
			UINT32 Flags;                       // Either 0 or XAUDIO2_END_OF_STREAM.
			UINT32 AudioBytes;                  // Size of the audio data buffer in bytes.
			const BYTE* pAudioData;             // Pointer to the audio data buffer.
			UINT32 PlayBegin;                   // First sample in this buffer to be played.
			UINT32 PlayLength;                  // Length of the region to be played in samples,
												//  or 0 to play the whole buffer.
			UINT32 LoopBegin;                   // First sample of the region to be looped.
			UINT32 LoopLength;                  // Length of the desired loop region in samples,
												//  or 0 to loop the entire buffer.
			UINT32 LoopCount;                   // Number of times to repeat the loop region,
												//  or XAUDIO2_LOOP_INFINITE to loop forever.
			void* pContext;                     // Context value to be passed back in callbacks.
		} XAUDIO2_BUFFER;

		typedef struct XAUDIO2_BUFFER_WMA
		{
			const UINT32* pDecodedPacketCumulativeBytes; // Decoded packet's cumulative size array.
														 //  Each element is the number of bytes accumulated
														 //  when the corresponding XWMA packet is decoded in
														 //  order.  The array must have PacketCount elements.
			UINT32 PacketCount;                          // Number of XWMA packets submitted. Must be >= 1 and
														 //  divide evenly into XAUDIO2_BUFFER.AudioBytes.
		} XAUDIO2_BUFFER_WMA;

		typedef struct XAUDIO2_VOICE_STATE
		{
			void* pCurrentBufferContext;        // The pContext value provided in the XAUDIO2_BUFFER
												//  that is currently being processed, or NULL if
												//  there are no buffers in the queue.
			UINT32 BuffersQueued;               // Number of buffers currently queued on the voice
												//  (including the one that is being processed).
			UINT64 SamplesPlayed;               // Total number of samples produced by the voice since
												//  it began processing the current audio stream.
												//  If XAUDIO2_VOICE_NOSAMPLESPLAYED is specified
												//  in the call to IXAudio2SourceVoice::GetState,
												//  this member will not be calculated, saving CPU.
		} XAUDIO2_VOICE_STATE;

		struct IXAudio2SourceVoice : IXAudio2Voice
		{
			// NAME: IXAudio2SourceVoice::Start
			// DESCRIPTION: Makes this voice start consuming and processing audio.
			//
			// ARGUMENTS:
			//  Flags - Flags controlling how the voice should be started.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			//
			STDMETHOD(Start) (THIS_ UINT32 Flags X2DEFAULT(0), UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE;

			// NAME: IXAudio2SourceVoice::Stop
			// DESCRIPTION: Makes this voice stop consuming audio.
			//
			// ARGUMENTS:
			//  Flags - Flags controlling how the voice should be stopped.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			//
			STDMETHOD(Stop) (THIS_ UINT32 Flags X2DEFAULT(0), UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE;

			// NAME: IXAudio2SourceVoice::SubmitSourceBuffer
			// DESCRIPTION: Adds a new audio buffer to this voice's input queue.
			//
			// ARGUMENTS:
			//  pBuffer - Pointer to the buffer structure to be queued.
			//  pBufferWMA - Additional structure used only when submitting XWMA data.
			//
			STDMETHOD(SubmitSourceBuffer) (THIS_ _In_ const XAUDIO2_BUFFER* pBuffer, _In_opt_ const XAUDIO2_BUFFER_WMA* pBufferWMA X2DEFAULT(NULL)) PURE;

			// NAME: IXAudio2SourceVoice::FlushSourceBuffers
			// DESCRIPTION: Removes all pending audio buffers from this voice's queue.
			//
			STDMETHOD(FlushSourceBuffers) (THIS) PURE;

			// NAME: IXAudio2SourceVoice::Discontinuity
			// DESCRIPTION: Notifies the voice of an intentional break in the stream of
			//              audio buffers (e.g. the end of a sound), to prevent XAudio2
			//              from interpreting an empty buffer queue as a glitch.
			//
			STDMETHOD(Discontinuity) (THIS) PURE;

			// NAME: IXAudio2SourceVoice::ExitLoop
			// DESCRIPTION: Breaks out of the current loop when its end is reached.
			//
			// ARGUMENTS:
			//  OperationSet - Used to identify this call as part of a deferred batch.
			//
			STDMETHOD(ExitLoop) (THIS_ UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE;

			// NAME: IXAudio2SourceVoice::GetState
			// DESCRIPTION: Returns the number of buffers currently queued on this voice,
			//              the pContext value associated with the currently processing
			//              buffer (if any), and other voice state information.
			//
			// ARGUMENTS:
			//  pVoiceState - Returns the state information.
			//  Flags - Flags controlling what voice state is returned.
			//
			STDMETHOD_(void, GetState) (THIS_ _Out_ XAUDIO2_VOICE_STATE* pVoiceState, UINT32 Flags X2DEFAULT(0)) PURE;

			// NAME: IXAudio2SourceVoice::SetFrequencyRatio
			// DESCRIPTION: Sets this voice's frequency adjustment, i.e. its pitch.
			//
			// ARGUMENTS:
			//  Ratio - Frequency change, expressed as source frequency / target frequency.
			//  OperationSet - Used to identify this call as part of a deferred batch.
			//
			STDMETHOD(SetFrequencyRatio) (THIS_ float Ratio,
				UINT32 OperationSet X2DEFAULT(XAUDIO2_COMMIT_NOW)) PURE;

			// NAME: IXAudio2SourceVoice::GetFrequencyRatio
			// DESCRIPTION: Returns this voice's current frequency adjustment ratio.
			//
			// ARGUMENTS:
			//  pRatio - Returns the frequency adjustment.
			//
			STDMETHOD_(void, GetFrequencyRatio) (THIS_ _Out_ float* pRatio) PURE;

			// NAME: IXAudio2SourceVoice::SetSourceSampleRate
			// DESCRIPTION: Reconfigures this voice to treat its source data as being
			//              at a different sample rate than the original one specified
			//              in CreateSourceVoice's pSourceFormat argument.
			//
			// ARGUMENTS:
			//  UINT32 - The intended sample rate of further submitted source data.
			//
			STDMETHOD(SetSourceSampleRate) (THIS_ UINT32 NewSourceSampleRate) PURE;
		};

		typedef struct tWAVEFORMATEX
		{
			WORD        wFormatTag;         /* format type */
			WORD        nChannels;          /* number of channels (i.e. mono, stereo...) */
			DWORD       nSamplesPerSec;     /* sample rate */
			DWORD       nAvgBytesPerSec;    /* for buffer estimation */
			WORD        nBlockAlign;        /* block size of data */
			WORD        wBitsPerSample;     /* number of bits per sample of mono data */
			WORD        cbSize;             /* the count in bytes of the size of */
											/* extra information (after cbSize) */
		} WAVEFORMATEX;

#define XAUDIO2_MAX_BUFFER_BYTES        0x80000000    // Maximum bytes allowed in a source buffer
#define XAUDIO2_MAX_QUEUED_BUFFERS      64            // Maximum buffers allowed in a voice queue
#define XAUDIO2_MAX_BUFFERS_SYSTEM      2             // Maximum buffers allowed for system threads (Xbox 360 only)
#define XAUDIO2_MAX_AUDIO_CHANNELS      64            // Maximum channels in an audio stream
#define XAUDIO2_MIN_SAMPLE_RATE         1000          // Minimum audio sample rate supported
#define XAUDIO2_MAX_SAMPLE_RATE         200000        // Maximum audio sample rate supported
#define XAUDIO2_MAX_VOLUME_LEVEL        16777216.0f   // Maximum acceptable volume level (2^24)
#define XAUDIO2_MIN_FREQ_RATIO          (1/1024.0f)   // Minimum SetFrequencyRatio argument
#define XAUDIO2_MAX_FREQ_RATIO          1024.0f       // Maximum MaxFrequencyRatio argument
#define XAUDIO2_DEFAULT_FREQ_RATIO      2.0f          // Default MaxFrequencyRatio argument
#define XAUDIO2_MAX_FILTER_ONEOVERQ     1.5f          // Maximum XAUDIO2_FILTER_PARAMETERS.OneOverQ
#define XAUDIO2_MAX_FILTER_FREQUENCY    1.0f          // Maximum XAUDIO2_FILTER_PARAMETERS.Frequency
#define XAUDIO2_MAX_LOOP_COUNT          254           // Maximum non-infinite XAUDIO2_BUFFER.LoopCount
#define XAUDIO2_MAX_INSTANCES           8             // Maximum simultaneous XAudio2 objects on Xbox 360

		struct IXAudio2MasteringVoice : IXAudio2Voice
		{
			// NAME: IXAudio2MasteringVoice::GetChannelMask
			// DESCRIPTION: Returns the channel mask for this voice
			//
			// ARGUMENTS:
			//  pChannelMask - returns the channel mask for this voice.  This corresponds
			//                 to the dwChannelMask member of WAVEFORMATEXTENSIBLE.
			//
			STDMETHOD(GetChannelMask) (THIS_ _Out_ DWORD* pChannelmask) PURE;
		};

		struct IXAudio2VoiceCallback
		{
			// Called just before this voice's processing pass begins.
			STDMETHOD_(void, OnVoiceProcessingPassStart) (THIS_ UINT32 BytesRequired) PURE;

			// Called just after this voice's processing pass ends.
			STDMETHOD_(void, OnVoiceProcessingPassEnd) (THIS) PURE;

			// Called when this voice has just finished playing a buffer stream
			// (as marked with the XAUDIO2_END_OF_STREAM flag on the last buffer).
			STDMETHOD_(void, OnStreamEnd) (THIS) PURE;

			// Called when this voice is about to start processing a new buffer.
			STDMETHOD_(void, OnBufferStart) (THIS_ void* pBufferContext) PURE;

			// Called when this voice has just finished processing a buffer.
			// The buffer can now be reused or destroyed.
			STDMETHOD_(void, OnBufferEnd) (THIS_ void* pBufferContext) PURE;

			// Called when this voice has just reached the end position of a loop.
			STDMETHOD_(void, OnLoopEnd) (THIS_ void* pBufferContext) PURE;

			// Called in the event of a critical error during voice processing,
			// such as a failing xAPO or an error from the hardware XMA decoder.
			// The voice may have to be destroyed and re-created to recover from
			// the error.  The callback arguments report which buffer was being
			// processed when the error occurred, and its HRESULT code.
			STDMETHOD_(void, OnVoiceError) (THIS_ void* pBufferContext, HRESULT Error) PURE;
		};

		typedef enum _AUDIO_STREAM_CATEGORY
		{
			AudioCategory_Other = 0,
			AudioCategory_ForegroundOnlyMedia,
			AudioCategory_BackgroundCapableMedia,
			AudioCategory_Communications,
			AudioCategory_Alerts,
			AudioCategory_SoundEffects,
			AudioCategory_GameEffects,
			AudioCategory_GameMedia,
		} AUDIO_STREAM_CATEGORY;

		typedef struct XAUDIO2_PERFORMANCE_DATA
		{
			// CPU usage information
			UINT64 AudioCyclesSinceLastQuery;   // CPU cycles spent on audio processing since the
												//  last call to StartEngine or GetPerformanceData.
			UINT64 TotalCyclesSinceLastQuery;   // Total CPU cycles elapsed since the last call
												//  (only counts the CPU XAudio2 is running on).
			UINT32 MinimumCyclesPerQuantum;     // Fewest CPU cycles spent processing any one
												//  audio quantum since the last call.
			UINT32 MaximumCyclesPerQuantum;     // Most CPU cycles spent processing any one
												//  audio quantum since the last call.

												// Memory usage information
			UINT32 MemoryUsageInBytes;          // Total heap space currently in use.

												// Audio latency and glitching information
			UINT32 CurrentLatencyInSamples;     // Minimum delay from when a sample is read from a
												//  source buffer to when it reaches the speakers.
			UINT32 GlitchesSinceEngineStarted;  // Audio dropouts since the engine was started.

												// Data about XAudio2's current workload
			UINT32 ActiveSourceVoiceCount;      // Source voices currently playing.
			UINT32 TotalSourceVoiceCount;       // Source voices currently existing.
			UINT32 ActiveSubmixVoiceCount;      // Submix voices currently playing/existing.

			UINT32 ActiveResamplerCount;        // Resample xAPOs currently active.
			UINT32 ActiveMatrixMixCount;        // MatrixMix xAPOs currently active.

												// Usage of the hardware XMA decoder (Xbox 360 only)
			UINT32 ActiveXmaSourceVoices;       // Number of source voices decoding XMA data.
			UINT32 ActiveXmaStreams;            // A voice can use more than one XMA stream.
		} XAUDIO2_PERFORMANCE_DATA;

		typedef struct XAUDIO2_DEBUG_CONFIGURATION
		{
			UINT32 TraceMask;                   // Bitmap of enabled debug message types.
			UINT32 BreakMask;                   // Message types that will break into the debugger.
			BOOL LogThreadID;                   // Whether to log the thread ID with each message.
			BOOL LogFileline;                   // Whether to log the source file and line number.
			BOOL LogFunctionName;               // Whether to log the function name.
			BOOL LogTiming;                     // Whether to log message timestamps.
		} XAUDIO2_DEBUG_CONFIGURATION;

		struct IUnkown
		{
			// NAME: IXAudio2::QueryInterface
			// DESCRIPTION: Queries for a given COM interface on the XAudio2 object.
			//              Only IID_IUnknown and IID_IXAudio2 are supported.
			//
			// ARGUMENTS:
			//  riid - IID of the interface to be obtained.
			//  ppvInterface - Returns a pointer to the requested interface.
			//
			STDMETHOD(QueryInterface) (THIS_ REFIID riid, _Outptr_ void** ppvInterface) PURE;

			// NAME: IXAudio2::AddRef
			// DESCRIPTION: Adds a reference to the XAudio2 object.
			//
			STDMETHOD_(ULONG, AddRef) (THIS) PURE;

			// NAME: IXAudio2::Release
			// DESCRIPTION: Releases a reference to the XAudio2 object.
			//
			STDMETHOD_(ULONG, Release) (THIS) PURE;
		};

		struct IXAudio2 : IUnkown
		{
			// NAME: IXAudio2::RegisterForCallbacks
			// DESCRIPTION: Adds a new client to receive XAudio2's engine callbacks.
			//
			// ARGUMENTS:
			//  pCallback - Callback interface to be called during each processing pass.
			//
			STDMETHOD(RegisterForCallbacks) (_In_ IXAudio2EngineCallback* pCallback) PURE;

			// NAME: IXAudio2::UnregisterForCallbacks
			// DESCRIPTION: Removes an existing receiver of XAudio2 engine callbacks.
			//
			// ARGUMENTS:
			//  pCallback - Previously registered callback interface to be removed.
			//
			STDMETHOD_(void, UnregisterForCallbacks) (_In_ IXAudio2EngineCallback* pCallback) PURE;

			// NAME: IXAudio2::CreateSourceVoice
			// DESCRIPTION: Creates and configures a source voice.
			//
			// ARGUMENTS:
			//  ppSourceVoice - Returns the new object's IXAudio2SourceVoice interface.
			//  pSourceFormat - Format of the audio that will be fed to the voice.
			//  Flags - XAUDIO2_VOICE flags specifying the source voice's behavior.
			//  MaxFrequencyRatio - Maximum SetFrequencyRatio argument to be allowed.
			//  pCallback - Optional pointer to a client-provided callback interface.
			//  pSendList - Optional list of voices this voice should send audio to.
			//  pEffectChain - Optional list of effects to apply to the audio data.
			//
			STDMETHOD(CreateSourceVoice) (THIS_ _Outptr_ IXAudio2SourceVoice** ppSourceVoice,
				_In_ const WAVEFORMATEX* pSourceFormat,
				UINT32 Flags X2DEFAULT(0),
				float MaxFrequencyRatio X2DEFAULT(XAUDIO2_DEFAULT_FREQ_RATIO),
				_In_opt_ IXAudio2VoiceCallback* pCallback X2DEFAULT(NULL),
				_In_opt_ const XAUDIO2_VOICE_SENDS* pSendList X2DEFAULT(NULL),
				_In_opt_ const XAUDIO2_EFFECT_CHAIN* pEffectChain X2DEFAULT(NULL)) PURE;

			// NAME: IXAudio2::CreateSubmixVoice
			// DESCRIPTION: Creates and configures a submix voice.
			//
			// ARGUMENTS:
			//  ppSubmixVoice - Returns the new object's IXAudio2SubmixVoice interface.
			//  InputChannels - Number of channels in this voice's input audio data.
			//  InputSampleRate - Sample rate of this voice's input audio data.
			//  Flags - XAUDIO2_VOICE flags specifying the submix voice's behavior.
			//  ProcessingStage - Arbitrary number that determines the processing order.
			//  pSendList - Optional list of voices this voice should send audio to.
			//  pEffectChain - Optional list of effects to apply to the audio data.
			//
			STDMETHOD(CreateSubmixVoice) (THIS_ _Outptr_ IXAudio2Voice** ppSubmixVoice,
				UINT32 InputChannels, UINT32 InputSampleRate,
				UINT32 Flags X2DEFAULT(0), UINT32 ProcessingStage X2DEFAULT(0),
				_In_opt_ const XAUDIO2_VOICE_SENDS* pSendList X2DEFAULT(NULL),
				_In_opt_ const XAUDIO2_EFFECT_CHAIN* pEffectChain X2DEFAULT(NULL)) PURE;


			// NAME: IXAudio2::CreateMasteringVoice
			// DESCRIPTION: Creates and configures a mastering voice.
			//
			// ARGUMENTS:
			//  ppMasteringVoice - Returns the new object's IXAudio2MasteringVoice interface.
			//  InputChannels - Number of channels in this voice's input audio data.
			//  InputSampleRate - Sample rate of this voice's input audio data.
			//  Flags - XAUDIO2_VOICE flags specifying the mastering voice's behavior.
			//  szDeviceId - Identifier of the device to receive the output audio.
			//  pEffectChain - Optional list of effects to apply to the audio data.
			//  StreamCategory - The audio stream category to use for this mastering voice
			//
			STDMETHOD(CreateMasteringVoice) (THIS_ _Outptr_ IXAudio2MasteringVoice** ppMasteringVoice,
				UINT32 InputChannels X2DEFAULT(XAUDIO2_DEFAULT_CHANNELS),
				UINT32 InputSampleRate X2DEFAULT(XAUDIO2_DEFAULT_SAMPLERATE),
				UINT32 Flags X2DEFAULT(0), _In_opt_z_ void* szDeviceId X2DEFAULT(NULL),
				_In_opt_ const XAUDIO2_EFFECT_CHAIN* pEffectChain X2DEFAULT(NULL),
				_In_ AUDIO_STREAM_CATEGORY StreamCategory X2DEFAULT(AudioCategory_GameEffects)) PURE;

			// NAME: IXAudio2::StartEngine
			// DESCRIPTION: Creates and starts the audio processing thread.
			//
			STDMETHOD(StartEngine) (THIS) PURE;

			// NAME: IXAudio2::StopEngine
			// DESCRIPTION: Stops and destroys the audio processing thread.
			//
			STDMETHOD_(void, StopEngine) (THIS) PURE;

			// NAME: IXAudio2::CommitChanges
			// DESCRIPTION: Atomically applies a set of operations previously tagged
			//              with a given identifier.
			//
			// ARGUMENTS:
			//  OperationSet - Identifier of the set of operations to be applied.
			//
			STDMETHOD(CommitChanges) (THIS_ UINT32 OperationSet) PURE;

			// NAME: IXAudio2::GetPerformanceData
			// DESCRIPTION: Returns current resource usage details: memory, CPU, etc.
			//
			// ARGUMENTS:
			//  pPerfData - Returns the performance data structure.
			//
			STDMETHOD_(void, GetPerformanceData) (THIS_ _Out_ XAUDIO2_PERFORMANCE_DATA* pPerfData) PURE;

			// NAME: IXAudio2::SetDebugConfiguration
			// DESCRIPTION: Configures XAudio2's debug output (in debug builds only).
			//
			// ARGUMENTS:
			//  pDebugConfiguration - Structure describing the debug output behavior.
			//  pReserved - Optional parameter; must be NULL.
			//
			STDMETHOD_(void, SetDebugConfiguration) (THIS_ _In_opt_ const XAUDIO2_DEBUG_CONFIGURATION* pDebugConfiguration,
				_Reserved_ void* pReserved X2DEFAULT(NULL)) PURE;
		};

		IXAudio2* x_audio2_;

		bool xnAudioInit()
		{
			CoInitializeEx(NULL, 0x0);

			auto result = XAudio2Create((void**)&x_audio2_, 0, 0x00000001);
			if (FAILED(result)) return false;

			return true;
		}

		struct xnAudioDevice
		{			
			IXAudio2MasteringVoice* mastering_voice_;
		};

		struct xnAudioBuffer
		{
			XAUDIO2_BUFFER buffer_;
		};

		struct xnAudioListener
		{
			//ALCcontext* context;
			//tinystl::unordered_map<ALuint, xnAudioBuffer*> buffers;
		};

		struct xnAudioSource
		{
			IXAudio2SourceVoice* source_voice_;
			xnAudioListener* listener;
		};

		xnAudioDevice* xnAudioCreate(const char* deviceName)
		{
			auto res = new xnAudioDevice;
			auto result = x_audio2_->CreateMasteringVoice(&res->mastering_voice_);
			if (FAILED(result)) return NULL;
			return res;
		}

		void xnAudioDestroy(xnAudioDevice* device)
		{
			device->mastering_voice_->DestroyVoice();
			delete device;
		}

		xnAudioListener* xnAudioListenerCreate(xnAudioDevice* device)
		{
			auto res = new xnAudioListener;
			
			return res;
		}

		void xnAudioListenerDestroy(xnAudioListener* listener)
		{
			
		}

		bool xnAudioListenerEnable(xnAudioListener* listener)
		{
			return true;
		}

		void xnAudioListenerDisable(xnAudioListener* listener)
		{
			
		}

		xnAudioSource* xnAudioSourceCreate(xnAudioListener* listener)
		{
			auto res = new xnAudioSource;
			res->listener = listener;
			
			x_audio2_->CreateSourceVoice(&res->source_voice_, NULL);

			return res;
		}

		void xnAudioSourceDestroy(xnAudioSource* source)
		{
			source->source_voice_->DestroyVoice();
			delete source;
		}

		void xnAudioSourceSetPan(xnAudioSource* source, float pan)
		{
			
		}

		void xnAudioSourceSetLooping(xnAudioSource* source, bool looping)
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

		void xnAudioSourceQueueBuffer(xnAudioSource* source, xnAudioBuffer* buffer, short* pcm, int bufferSize, int sampleRate, bool mono)
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

		bool xnAudioSourceIsPlaying(xnAudioSource* source)
		{
			
		}

		xnAudioBuffer* xnAudioBufferCreate()
		{
			
		}

		void xnAudioBufferDestroy(xnAudioBuffer* buffer)
		{
			
		}

		void xnAudioBufferFill(xnAudioBuffer* buffer, short* pcm, int bufferSize, int sampleRate, bool mono)
		{
			
		}
	}	
}

#endif
