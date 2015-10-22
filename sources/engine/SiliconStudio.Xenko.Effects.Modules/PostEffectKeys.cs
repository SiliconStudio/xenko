// Copyright (c) 2011 Silicon Studio

namespace Xenko.Effects.Modules
{
    public class PostEffectKeys
    {
        /// <summary>
        /// Blur coefficients from [0] to [4].
        /// </summary>
        public static readonly ParameterArrayValueKey<float> MixCoefficients = ParameterKeys.ArrayValue(new[] { 0.5f, 0.5f });
    }
}
