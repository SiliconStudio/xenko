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
        }

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInputRenderCallbackToChannelMixerDefault_(IntPtr inUnit, uint element, IntPtr userData);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInputRenderCallbackTo3DMixerDefault_(IntPtr inUnit, uint element, IntPtr userData);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInputRenderCallbackToNull_(IntPtr inUnit, uint element);

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XenkoAudioUnitHelpersInit();
    }
}
