// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public static class LightShaftsEffectKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> ShadowGroup = ParameterKeys.NewPermutation<ShaderSource>();
        public static readonly PermutationParameterKey<ShaderSource> AttenuationModel = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
