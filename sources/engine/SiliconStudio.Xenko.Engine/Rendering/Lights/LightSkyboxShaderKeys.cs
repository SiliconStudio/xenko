// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public partial class LightSkyboxShaderKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> LightDiffuseColor = ParameterKeys.NewPermutation<ShaderSource>();
        public static readonly PermutationParameterKey<ShaderSource> LightSpecularColor = ParameterKeys.NewPermutation<ShaderSource>();
    }
}