// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.ComputeEffect
{
    public class ComputeEffectShaderKeys
    {
        public static readonly PermutationParameterKey<string> ComputeShaderName = ParameterKeys.NewPermutation<string>();
        public static readonly PermutationParameterKey<Int3> ThreadNumbers = ParameterKeys.NewPermutation<Int3>();
    }
}
