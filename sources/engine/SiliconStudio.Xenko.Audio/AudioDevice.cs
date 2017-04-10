// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Audio
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            // Make sure that this assembly is registered
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
        }
    }

    /// <summary>
    /// Reprensent an Audio Hardware Device.
    /// Can be used when creating an <see cref="AudioEngine"/> to specify the device on which to play the sound.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Returns the name of the current device.
        /// </summary>
        public string Name { get; set; }

        public AudioDevice()
        {
            Name = "default";
        }
    }
}
