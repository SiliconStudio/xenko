// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public static class LightShaftsEffectKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> LightGroup = ParameterKeys.NewPermutation<ShaderSource>();
        public static readonly PermutationParameterKey<int> SampleCount = ParameterKeys.NewPermutation<int>();
        public static readonly PermutationParameterKey<bool> Animated = ParameterKeys.NewPermutation<bool>();
    }
}
