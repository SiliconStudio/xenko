// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Keys used by the DepthAwareDirectionalBlurEffect
    /// </summary>
    public static class DepthAwareDirectionalBlurKeys
    {
        public static readonly PermutationParameterKey<int> Count = ParameterKeys.NewPermutation<int>();

        public static readonly PermutationParameterKey<int> TotalTap = ParameterKeys.NewPermutation<int>();
    }
}