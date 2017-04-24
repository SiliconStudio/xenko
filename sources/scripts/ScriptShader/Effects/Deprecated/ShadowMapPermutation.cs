// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
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
