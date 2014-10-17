#ifdef PLATFORM_IOS

#include <AudioUnit/AudioUnit.h>
#include <algorithm>
#include <map>
#include <new>

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
    
    void* HandleChannelMixer;
    void* Handle3DMixer;
    
    bool ShouldBeLooped()
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
    
    int AudioDataMixerCallback(uint busIndex, int totalNbOfFrameToWrite,  AudioBufferList* data)
    {
        char* outPtr = (char*)data->mBuffers[0].mData;
        
        int remainingFramesToWrite = totalNbOfFrameToWrite;
        while (remainingFramesToWrite > 0)
        {
            int nbOfFrameToWrite = std::min(remainingFramesToWrite, (ShouldBeLooped() ? LoopEndPoint : TotalNumberOfFrames) - CurrentFrame);
                
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
                AudioUnitSetParameter((AudioUnit)HandleChannelMixer, kMultiChannelMixerParam_Enable, kAudioUnitScope_Input, busIndex, 0, 0);
                AudioUnitSetParameter((AudioUnit)Handle3DMixer, k3DMixerParam_Enable, kAudioUnitScope_Input, busIndex, 0, 0);
            
                IsEnabled2D = false;
                IsEnabled3D = false;
            
                PlaybackEnded = true;
            
                // Fill the rest of the buffer with blank
                int sizeToBlank = sizeof(short) * NumberOfChannels * remainingFramesToWrite;
                memset(outPtr, 0, sizeToBlank);
            
                return 0;
            }
        }
        
        return 0;
    }
    
    OSStatus RendererCallbackChannelMixer(uint busNumber, uint numberFrames, AudioBufferList* data)
    {
        if (!IsEnabled2D)
            return 0;
        
        OSStatus ret = AudioDataMixerCallback(busNumber, (int)numberFrames, data);
        
        return ret;
    }
    
    OSStatus RendererCallback3DMixer(uint busNumber, uint numberFrames, AudioBufferList* data)
    {
        if (!IsEnabled3D)
            return 0;
        
        return AudioDataMixerCallback(busNumber, (int)numberFrames, data);
    }
};

static OSStatus NullRenderCallback (void                        *inRefCon,
                                    AudioUnitRenderActionFlags  *ioActionFlags,
                                    const AudioTimeStamp        *inTimeStamp,
                                    UInt32                      inBusNumber,
                                    UInt32                      inNumberFrames,
                                    AudioBufferList             *ioData)
{
    memset(ioData->mBuffers[0].mData, 0, ioData->mBuffers[0].mDataByteSize);
    
    return 0;
}

static OSStatus DefaultRenderCallbackChannelMixer (	void                        *inRefCon,
													AudioUnitRenderActionFlags  *ioActionFlags,
													const AudioTimeStamp        *inTimeStamp,
													UInt32                      inBusNumber,
													UInt32                      inNumberFrames,
													AudioBufferList             *ioData)
{
    return ((AudioDataRenderer*)inRefCon)->RendererCallbackChannelMixer(inBusNumber, inNumberFrames, ioData);
}

static OSStatus DefaultRenderCallback3DMixer (	void                        *inRefCon,
												AudioUnitRenderActionFlags  *ioActionFlags,
												const AudioTimeStamp        *inTimeStamp,
												UInt32                      inBusNumber,
												UInt32                      inNumberFrames,
												AudioBufferList             *ioData)
{
    return ((AudioDataRenderer*)inRefCon)->RendererCallback3DMixer(inBusNumber, inNumberFrames, ioData);
}

static AURenderCallbackStruct NullRenderCallbackStruct = { NullRenderCallback, NULL };

static std::map<uint, AURenderCallbackStruct*> BusIndexToChannelMixerCallbackStructures;
static std::map<uint, AURenderCallbackStruct*> BusIndexTo3DMixerCallbackStructures;

extern "C" int SetInputRenderCallbackToChannelMixerDefault(void* inUnit, uint element, void* userData)
{
    AURenderCallbackStruct* pCallbackData = new AURenderCallbackStruct;
    pCallbackData->inputProc = DefaultRenderCallbackChannelMixer;
    pCallbackData->inputProcRefCon = userData;
    
    int status = AudioUnitSetProperty((AudioUnit)inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, pCallbackData, sizeof(AURenderCallbackStruct));
    
    // update BusIndexToChannelMixerCallbackStructures map with last valid data
    if(BusIndexToChannelMixerCallbackStructures.find(element) != BusIndexToChannelMixerCallbackStructures.end())
    {
        if(BusIndexToChannelMixerCallbackStructures[element] != NULL)
        {
            delete BusIndexToChannelMixerCallbackStructures[element];
            BusIndexToChannelMixerCallbackStructures[element] = NULL;
        }
    }
    BusIndexToChannelMixerCallbackStructures[element] = pCallbackData;
    
    return status;
}

extern "C" int SetInputRenderCallbackTo3DMixerDefault(void* inUnit, uint element, void* userData)
{
    AURenderCallbackStruct* pCallbackData = new AURenderCallbackStruct;
    pCallbackData->inputProc = DefaultRenderCallback3DMixer;
    pCallbackData->inputProcRefCon = userData;
    
    int status = AudioUnitSetProperty((AudioUnit)inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, pCallbackData, sizeof(AURenderCallbackStruct));
    
    // update BusIndexTo3DMixerCallbackStructures map with last valid data
    if(BusIndexTo3DMixerCallbackStructures.find(element) != BusIndexTo3DMixerCallbackStructures.end())
    {
        if(BusIndexTo3DMixerCallbackStructures[element] != NULL)
        {
            delete BusIndexTo3DMixerCallbackStructures[element];
            BusIndexTo3DMixerCallbackStructures[element] = NULL;
        }
    }
    BusIndexTo3DMixerCallbackStructures[element] = pCallbackData;
    
    return status;
}

extern "C" int SetInputRenderCallbackToNull(void* inUnit, uint element)
{
    return AudioUnitSetProperty((AudioUnit)inUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Input, element, &NullRenderCallbackStruct, sizeof(AURenderCallbackStruct));
}

#endif
