// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Platform specific queries and functions.
    /// </summary>
    public static class Platform
    {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.Windows;
#elif SILICONSTUDIO_PLATFORM_WINDOWS_PHONE
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.WindowsPhone;
#elif SILICONSTUDIO_PLATFORM_WINDOWS_STORE
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.WindowsStore;
#elif SILICONSTUDIO_PLATFORM_WINDOWS_10
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.Windows10;
#elif SILICONSTUDIO_PLATFORM_ANDROID
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.Android;
#elif SILICONSTUDIO_PLATFORM_IOS
        /// <summary>
        /// The current running <see cref="PlatformType"/>.
        /// </summary>
        public static readonly PlatformType Type = PlatformType.iOS;
#endif

        /// <summary>
        /// Gets a value indicating whether the running platform is windows desktop.
        /// </summary>
        /// <value><c>true</c> if this instance is windows desktop; otherwise, <c>false</c>.</value>
        public static readonly bool IsWindowsDesktop = Type == PlatformType.Windows;

        /// <summary>
        /// Gets a value indicating whether the running assembly is a debug assembly.
        /// </summary>
        public static readonly bool IsRunningDebugAssembly = GetIsRunningDebugAssembly();

        private static bool GetIsRunningDebugAssembly()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            return false;
#else
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var debuggableAttribute = entryAssembly.GetCustomAttributes(typeof(DebuggableAttribute)).OfType<DebuggableAttribute>().FirstOrDefault();
                if (debuggableAttribute != null)
                {
                    return (debuggableAttribute.DebuggingFlags & DebuggableAttribute.DebuggingModes.DisableOptimizations) != 0;
                }
            }
            return false;
#endif
        }
    }
}