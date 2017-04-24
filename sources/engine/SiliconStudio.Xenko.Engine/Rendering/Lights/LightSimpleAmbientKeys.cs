// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public static partial class LightSimpleAmbientKeys
    {
        static LightSimpleAmbientKeys()
        {
            AmbientLight = ParameterKeys.NewValue(new Color3(1.0f, 1.0f, 1.0f));
        }
    }
}
