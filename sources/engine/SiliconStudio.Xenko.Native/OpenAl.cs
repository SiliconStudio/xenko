using System;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native
{
    public class OpenAl
    {
        static OpenAl()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".dll");
#else
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".so");
#endif
        }

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnInitOpenAL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool InitOpenAL();

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioCreate", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr AudioCreate(string deviceName);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioDestroy(IntPtr device);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioCreateVoice", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint AudioCreateVoice();

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioDestroyVoice", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioDestroyVoice(uint voice);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioCreateBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint AudioCreateBuffer();

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioDestroyBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioDestroyBuffer(uint voice);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioFillBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioFillBuffer(uint buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSetVoiceBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioSetVoiceBuffer(uint voice, uint buffer);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioVoiceQueueBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioVoiceQueueBuffer(uint voice, uint buffer);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioVoiceGetFreeBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint AudioVoiceGetFreeBuffer(uint voice);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioPlay", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioPlay(uint voice);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioPause", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioPause(uint voice);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioStop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AudioStop(uint voice);
    }
}
