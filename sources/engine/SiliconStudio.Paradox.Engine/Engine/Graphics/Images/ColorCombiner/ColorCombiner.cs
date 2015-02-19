// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A texture combiner allows to mix up to 10 textures with different weights.
    /// </summary>
    /// <remarks> This effects takes from 1 to 10 textures in input and combine them to a single output.
    /// Before using this class, it is recommended to clear the inputs by using <see cref="ImageEffect.Reset"/>.
    /// </remarks>
    public class ColorCombiner : ImageEffectShader
    {
        internal static readonly ParameterKey<int> FactorCount = ParameterKeys.New(0);

        private readonly float[] factors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorCombiner"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="colorCombinerShaderName">Name of the color combiner shader.</param>
        public ColorCombiner(string colorCombinerShaderName = "ColorCombinerEffect")
        {
            EffectName = colorCombinerShaderName;
            factors = new float[TexturingKeys.DefaultTextures.Count];
        }

        /// <summary>
        /// Gets the factors used to multiply the colors.
        /// </summary>
        /// <value>The factors.</value>
        public float[] Factors
        {
            get
            {
                return factors;
            }
        }

        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);
            Parameters.Set(FactorCount, InputCount);
            Parameters.Set(ColorCombinerShaderKeys.Factors, factors);
        }
    }
}