// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    public class ShadowMapPermutationArray : PermutationArray
    {
        public static ParameterKey<ShadowMapPermutationArray> Key = ParameterKeys.Resource(new ShadowMapPermutationArray());

        public ShadowMapPermutationArray()
        {
            ShadowMaps = new List<ShadowMapPermutation>();
        }

        public IList<ShadowMapPermutation> ShadowMaps { get; set; }

        public override IEnumerable<Permutation> GetValues()
        {
            return ShadowMaps;
        }
    }
}
