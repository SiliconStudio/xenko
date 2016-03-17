// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public static class SkyboxKeys
    {
        public static readonly ValueParameterKey<float> Intensity = ParameterKeys.NewValue(1.0f);

        [Obsolete]
        public static readonly ValueParameterKey<float> Rotation = ParameterKeys.NewValue(0.0f);

        public static readonly ValueParameterKey<Matrix> SkyMatrix = ParameterKeys.NewValue(Matrix.Identity);

        public static readonly PermutationParameterKey<ShaderSource> Shader = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly PermutationParameterKey<ShaderSource> DiffuseLighting = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly PermutationParameterKey<ShaderSource> SpecularLighting = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly ObjectParameterKey<Texture> CubeMap = ParameterKeys.NewObject<Texture>();
    }
}