// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
