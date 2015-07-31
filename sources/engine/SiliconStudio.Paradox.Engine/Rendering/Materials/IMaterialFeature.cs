// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// Base interface for a material feature.
    /// </summary>
    public interface IMaterialFeature : IMaterialShaderGenerator
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphicsRenderer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }
    }
}