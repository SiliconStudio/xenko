// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// An ambient light.
    /// </summary>
    [DataContract("LightAmbient")]
    [Display("Ambient")]
    public class LightAmbient : ColorLightBase
    {
        public override bool Update(LightComponent lightComponent)
        {
            return true;
        }
    }
}