// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

//namespace SiliconStudio.Xenko.Rendering
namespace SiliconStudio.Xenko.Rendering
{
    public partial class ParticleCustomShaderKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> BaseColor = ParameterKeys.NewPermutation<ShaderSource>();
        public static readonly ObjectParameterKey<Texture> EmissiveMap = ParameterKeys.NewObject<Texture>();
        public static readonly ValueParameterKey<Color4> EmissiveValue = ParameterKeys.NewValue<Color4>();

        public static readonly PermutationParameterKey<ShaderSource> BaseIntensity = ParameterKeys.NewPermutation<ShaderSource>();
        public static readonly ObjectParameterKey<Texture> IntensityMap = ParameterKeys.NewObject<Texture>();
        public static readonly ValueParameterKey<float> IntensityValue = ParameterKeys.NewValue<float>();
    }
}
