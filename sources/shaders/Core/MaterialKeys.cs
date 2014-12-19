// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
{
    public partial class MaterialKeys
    {
        public static readonly ParameterKey<ShaderSource> Material = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> Texture = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<SamplerState> Sampler = ParameterKeys.New<SamplerState>();

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