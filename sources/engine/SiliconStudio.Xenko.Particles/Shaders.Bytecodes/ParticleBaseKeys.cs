// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

// The namespace has to stay SiliconStudio.Xenko.Rendering to match the generated shader code
namespace SiliconStudio.Xenko.Rendering
{
    public partial class ParticleBaseKeys
    {
        static ParticleBaseKeys()
        {
            //MatrixTransform = ParameterKeys.New(Matrix.Identity);
        }

        //public static readonly ParameterKey<bool> ColorIsSRgb = ParameterKeys.New(false);

        //public static readonly ParameterKey<ShaderSource> ParticleColor = ParameterKeys.New<ShaderSource>();

        public static readonly PermutationParameterKey<ShaderSource> BaseColor = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly PermutationParameterKey<uint> UsesSoftEdge = ParameterKeys.NewPermutation<uint>(0);

        //public static readonly ParameterKey<ShaderSource> BaseIntensity = ParameterKeys.New<ShaderSource>();

        public static readonly ObjectParameterKey<Texture> EmissiveMap = ParameterKeys.NewObject<Texture>();
        public static readonly ValueParameterKey<Color4> EmissiveValue = ParameterKeys.NewValue<Color4>();

        //public static readonly ParameterKey<Texture> IntensityMap = ParameterKeys.New<Texture>();
        //public static readonly ParameterKey<float>   IntensityValue = ParameterKeys.New<float>();

    }
}