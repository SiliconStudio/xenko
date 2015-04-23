namespace SiliconStudio.Paradox.Rendering
{
    public class ShadowMapPermutation : Permutation
    {
        public ShadowMapPermutation(ShadowMap shadowMap)
        {
            ShadowMap = shadowMap;
        }

        public static implicit operator ShadowMapPermutation(ShadowMap shadowMap)
        {
            return new ShadowMapPermutation(shadowMap);
        }

        public override object GenerateKey()
        {
            return ShadowMap;
        }

        public ShadowMap ShadowMap { get; private set; }
    }
}