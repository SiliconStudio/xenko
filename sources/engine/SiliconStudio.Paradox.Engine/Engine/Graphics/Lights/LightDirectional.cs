// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A directional light.
    /// </summary>
    [DataContract("LightDirectional")]
    [Display("Directional")]
    public class LightDirectional : DirectLightBase
    {
        // TODO: Add support for disk based sun
    }
}