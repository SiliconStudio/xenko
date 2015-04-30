// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Skyboxes
{
    public static class SkyboxKeys
    {
        public static readonly ParameterKey<float> Intensity = ParameterKeys.New(1.0f);

        public static readonly ParameterKey<float> Rotation = ParameterKeys.New(0.0f);

        public static readonly ParameterKey<Matrix> SkyMatrix = ParameterKeys.NewDynamic(ParameterDynamicValue.New<Matrix, float>(Rotation, UpdateSkyMatrix));

        public static readonly ParameterKey<ShaderSource> Shader = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> DiffuseLighting = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> SpecularLighting = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> CubeMap = ParameterKeys.New<Texture>();

        private static void UpdateSkyMatrix(ref float angle, ref Matrix output)
        {
            Matrix.RotationY(MathUtil.DegreesToRadians(angle), out output);
        }
    }
}