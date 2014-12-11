// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    public static partial class MaterialKeys
    {
        static MaterialKeys()
        {
            SpecularPowerScaled = ParameterKeys.NewDynamic(ParameterDynamicValue.New<float, float>(SpecularPower, ScaleSpecularPower));
        }

        private static void ScaleSpecularPower(ref float specularPower, ref float scaledSpecularPower)
        {
            scaledSpecularPower = (float)Math.Pow(2.0f, 1.0f + specularPower * 13.0f);
        }
    }
}