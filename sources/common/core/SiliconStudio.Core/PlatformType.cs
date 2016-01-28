// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core
{
    /// <summary>
    /// Describes the platform operating system.
    /// </summary>
#if ASSEMBLY_PROCESSOR
    internal enum PlatformType
#else
    [DataContract("PlatformType")]
    public enum PlatformType
#endif

    {
        /// <summary>
        /// This is shared across platforms
        /// </summary>
        Shared,

        /// <summary>
        /// The windows desktop OS.
        /// </summary>
        Windows,

        /// <summary>
        /// The Windows Phone OS.
        /// </summary>
        WindowsPhone,

        /// <summary>
        /// The Windows Store OS.
        /// </summary>
        WindowsStore,

        /// <summary>
        /// The android OS.
        /// </summary>
        Android,

        /// <summary>
        /// The iOS.
        /// </summary>
        iOS,

        /// <summary>
        /// The Windows 10 OS.
        /// </summary>
        Windows10,
    }
}