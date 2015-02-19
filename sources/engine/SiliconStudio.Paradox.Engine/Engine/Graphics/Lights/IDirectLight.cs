// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Base interface for all direct lights.
    /// </summary>
    public interface IDirectLight : IColorLight
    {
        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        ILightShadow Shadow { get; set; }
    }
}