// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
                Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, InputSH.Order);
                Parameters.Set(SphericalHarmonicsRendererKeys.SHCoefficients, InputSH.Coefficients);
            }
            else
            {
                Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, 1);
                Parameters.Set(SphericalHarmonicsRendererKeys.SHCoefficients, new []{ new Color3() });
            }
        }
    }
}
