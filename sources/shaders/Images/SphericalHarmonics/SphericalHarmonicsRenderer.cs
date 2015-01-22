// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Images.SphericalHarmonics
{
    public class SphericalHarmonicsRenderer :ImageEffectShader
    {
        /// <summary>
        /// Gets or sets the harmonic order to use during the filtering.
        /// </summary>
        public Core.Mathematics.SphericalHarmonics InputSH { get; set; }

        public SphericalHarmonicsRenderer(DrawEffectContext context)
            : base(context, "SphericalHarmonicsRenderer")
        {
        }

        protected override void UpdateParameters()
        {
            base.UpdateParameters();

            if (InputSH == null)
                return;

            Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, InputSH.Order);
            Parameters.Set(SphericalHarmonicsRendererKeys.SHCoefficients, InputSH.Coefficients);
        }
    }
}