// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public static class SkyboxKeys
    {
        public static readonly ParameterKey<float> Intensity = ParameterKeys.New(1.0f);

        public static readonly ParameterKey<float> Rotation = ParameterKeys.New(0.0f);

        public static readonly ParameterKey<Matrix> SkyMatrix = ParameterKeys.New(Matrix.Identity);

        public static readonly ParameterKey<ShaderSource> Shader = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> DiffuseLighting = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> SpecularLighting = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> CubeMap = ParameterKeys.New<Texture>();
    }
}