using System;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native
{
    public class AudioLayer
    {
        public struct Device
        {
            public IntPtr Ptr;
        }

        public struct Listener
        {
            public IntPtr Ptr;
        }

        public struct Source
        {
            public IntPtr Ptr;
        }

        public struct Buffer
        {
            public IntPtr Ptr;
        }

        static AudioLayer()
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
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioInit", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool Init();

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioCreate", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern Device Create(string deviceName);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy(Device device);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioListenerCreate", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern Listener ListenerCreate(Device device);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioListenerDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListenerDestroy(Listener listener);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioListenerEnable", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ListenerEnable(Listener listener);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioListenerDisable", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListenerDisable(Listener listener);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern Source SourceCreate(Listener listener, int sampleRate, bool mono, bool spatialized);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceDestroy(Source source);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetPan", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceSetPan(Source source, float pan);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioBufferCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern Buffer BufferCreate(int maxBufferSizeBytes);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioBufferDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BufferDestroy(Buffer buffer);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioBufferFill", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BufferFill(Buffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceSetBuffer(Source source, Buffer buffer);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceQueueBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceQueueBuffer(Source source, Buffer buffer, IntPtr pcm, int bufferSize, bool endOfStream);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceGetFreeBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Buffer SourceGetFreeBuffer(Source source);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourcePlay", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourcePlay(Source source);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourcePause", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourcePause(Source source);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceStop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceStop(Source source);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetLooping", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceSetLooping(Source source, bool looped);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetGain", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceSetGain(Source source, float gain);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetPitch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SourceSetPitch(Source source, float pitch);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioListenerPush3D", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void ListenerPush3D(Listener listener, float* pos, float* forward, float* up, float* vel);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourcePush3D", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SourcePush3D(Source source, float* pos, float* forward, float* up, float* vel);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceIsPlaying", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SourceIsPlaying(Source source);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xnSleep(int ms);
    }
}
