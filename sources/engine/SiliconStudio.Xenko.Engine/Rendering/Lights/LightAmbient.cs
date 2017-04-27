// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Lights
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
