// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Filtering type used for a Shadow map.
    /// </summary>
    [DataContract("LightShadowMapFilterType")]
    public enum LightShadowMapFilterType
    {
        Nearest = 0,
        PercentageCloserFiltering = 1,
        Variance = 2,
    }
}