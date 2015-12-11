// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public partial class ParticleBaseKeys
    {
        static ParticleBaseKeys()
        {
            MatrixTransform = ParameterKeys.New(Matrix.Identity);
        //    ViewMatrix = ParameterKeys.New(Matrix.Identity);
        //    ProjectionMatrix = ParameterKeys.New(Matrix.Identity);
        }

        public static readonly ParameterKey<bool> ColorIsSRgb = ParameterKeys.New(false);

        public static readonly ParameterKey<bool> HasTexture = ParameterKeys.New(true);

        //public static readonly ParameterKey<ShaderSource> ComputeColor0 = ParameterKeys.New<ShaderSource>();

        //public static readonly ParameterKey<ShaderSource> ComputeColor1 = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> BaseColor     = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> BaseIntensity = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> EmissiveMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Color4>  EmissiveValue = ParameterKeys.New<Color4>();

        public static readonly ParameterKey<Texture> IntensityMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float>   IntensityValue = ParameterKeys.New<float>();

    }
}