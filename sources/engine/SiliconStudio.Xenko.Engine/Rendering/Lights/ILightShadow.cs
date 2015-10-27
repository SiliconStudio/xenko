// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Rendering.Shadows;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Interface for the shadow of a light.
    /// </summary>
    public interface ILightShadow
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ILightShadow"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }
    }
}