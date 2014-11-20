// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Effects
{
    public class PostEffectKeys
    {
        /// <summary>
        /// Blur coefficients from [0] to [4].
        /// </summary>
        public static readonly ParameterKey<float[]> MixCoefficients = ParameterKeys.New(new[] { 0.5f, 0.5f });
    }
}
