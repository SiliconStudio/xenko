// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
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
        /// <userdoc>Display to color of the skybox</userdoc>
        Color,

        /// <summary>
        /// Display the irradiance of the skybox
        /// </summary>
        /// <userdoc>Display the irradiance generated from the skybox</userdoc>
        Irradiance
    }
}