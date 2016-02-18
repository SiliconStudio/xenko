// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Rendering.Images
{
    internal static class ToneMapKeys
    {
        public static readonly PermutationParameterKey<bool> AutoExposure = ParameterKeys.NewPermutation(false);

        public static readonly PermutationParameterKey<bool> AutoKey = ParameterKeys.NewPermutation(false);

        public static readonly PermutationParameterKey<ToneMapOperator> Operator = ParameterKeys.NewPermutation<ToneMapOperator>();
    }
}