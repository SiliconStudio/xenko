// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Linq;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Keys used by <see cref="FlareArtifact"/> and FlareArtifactEffect xkfx.
    /// </summary>
    internal static class FlareArtifactKeys
    {
        public static readonly PermutationParameterKey<int> Count = ParameterKeys.NewPermutation<int>();
    }
}
