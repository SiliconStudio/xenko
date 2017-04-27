// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public static class SkyboxKeys
    {
        public static readonly ValueParameterKey<float> Intensity = ParameterKeys.NewValue(1.0f);

        public static readonly ValueParameterKey<Matrix> SkyMatrix = ParameterKeys.NewValue(Matrix.Identity);

        public static readonly PermutationParameterKey<ShaderSource> Shader = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly PermutationParameterKey<ShaderSource> DiffuseLighting = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly PermutationParameterKey<ShaderSource> SpecularLighting = ParameterKeys.NewPermutation<ShaderSource>();

        public static readonly ObjectParameterKey<Texture> CubeMap = ParameterKeys.NewObject<Texture>();
    }
}
