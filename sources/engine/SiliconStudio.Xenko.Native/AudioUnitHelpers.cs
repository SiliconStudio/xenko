using System;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native
{
    public class AudioUnitHelpers
    {
        static AudioUnitHelpers()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".dll");
#else
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".so");
#endif

            if (!AudioUnitHelpersInit())
            {
                throw new Exception("Could not load AudioUnitHelpers");
            }

            Console.WriteLine(@"AudioUnitHelpers loaded");
        }

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnCreateAudioDataRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateAudioDataRenderer();

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnDestroyAudioDataRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyAudioDataRenderer(IntPtr audioRendererPtr);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAddAudioBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AddAudioBuffer(IntPtr renderer, IntPtr buffer, int channels, int nframes);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnSetAudioBufferFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAudioBufferFrame(IntPtr renderer, int bufferIndex, int frame);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnSetInputRenderCallbackToChannelMixerDefault", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInputRenderCallbackToChannelMixerDefault(IntPtr inUnit, uint element, IntPtr userData);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnSetInputRenderCallbackTo3DMixerDefault", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInputRenderCallbackTo3DMixerDefault(IntPtr inUnit, uint element, IntPtr userData);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnSetInputRenderCallbackToNull", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInputRenderCallbackToNull(IntPtr inUnit, uint element);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioUnitHelpersInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool AudioUnitHelpersInit();
    }
}
