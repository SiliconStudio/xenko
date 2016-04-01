// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Base interface for a shading light dependent model material feature.
    /// </summary>
    public interface IMaterialShadingModelFeature : IMaterialFeature, IEquatable<IMaterialShadingModelFeature>
    {
        /// <summary>
        /// A boolean indicating whether this material depends on the lighting.
        /// </summary>
        bool IsLightDependent { get; }
    }
}