// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Skyboxes
{
    /// <summary>
    /// Defines the usage of the skybox.
    /// </summary>
    [DataContract("SkyboxUsage")]
    public enum SkyboxUsage
    {
        /// <summary>
        /// The skybox is used only for lighting.
        /// </summary>
        Lighting,

        /// <summary>
        /// The skybox is used only for specular lighting. Useful in combination with light probes.
        /// </summary>
        SpecularLighting,
    }
}