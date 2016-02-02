// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Images.SphericalHarmonics
{
    public class SphericalHarmonicsRendererEffect :ImageEffectShader
    {
        /// <summary>
        /// Gets or sets the harmonic order to use during the filtering.
        /// </summary>
        public Core.Mathematics.SphericalHarmonics InputSH { get; set; }

        public SphericalHarmonicsRendererEffect()
        {
            EffectName = "SphericalHarmonicsRendererEffect";
        }

        protected override void UpdateParameters()
        {
            base.UpdateParameters();

            if (InputSH != null)
            {
                Parameters.SetValueSlow(SphericalHarmonicsParameters.HarmonicsOrder, InputSH.Order);
                Parameters.SetValueSlow(SphericalHarmonicsRendererKeys.SHCoefficients, InputSH.Coefficients);
            }
            else
            {
                Parameters.SetValueSlow(SphericalHarmonicsParameters.HarmonicsOrder, 1);
                Parameters.SetValueSlow(SphericalHarmonicsRendererKeys.SHCoefficients, new []{ new Color3() });
            }
        }
    }
}