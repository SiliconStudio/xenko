using System.Collections.Generic;

namespace SiliconStudio.Paradox.Rendering
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