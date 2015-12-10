// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
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

        public static readonly ParameterKey<ShaderSource> BaseColor  = ParameterKeys.New<ShaderSource>();
    }
}