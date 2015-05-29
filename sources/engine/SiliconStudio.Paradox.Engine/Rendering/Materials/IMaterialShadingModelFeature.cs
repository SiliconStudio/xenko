// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// Base interface for a shading light dependent model material feature.
    /// </summary>
    public interface IMaterialShadingModelFeature : IMaterialFeature, IEquatable<IMaterialShadingModelFeature>
    {
        bool IsLightDependent { get; }
    }
}