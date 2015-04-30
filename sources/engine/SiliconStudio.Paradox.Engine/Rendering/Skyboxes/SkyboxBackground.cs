// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Skyboxes
{
    /// <summary>
    /// Defines how the background parameters used for this skybox.
    /// </summary>
    [DataContract("SkyboxBackground")]
    public enum SkyboxBackground
    {
        /// <summary>
        /// Display the color of the skybox.
        /// </summary>
        Color,

        /// <summary>
        /// Display the irrandiance of the skybox
        /// </summary>
        Irradiance
    }
}