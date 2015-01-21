// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images.Cubemap
{
    /// <summary>
    /// Performs Lambertian pre-filtering in the form of Spherical Harmonics.
    /// </summary>
    public class LambertianPrefilteringSH : ImageEffect
    {
        private int harmonicalOrder;

        private readonly ImageEffect firstPassEffect;
        private readonly ImageEffect secondPassEffect;

        private SphericalHarmonics prefilteredLambertianSH;

        /// <summary>
        /// Gets or sets the level of precision desired when calculating the spherical harmonics.
        /// </summary>
        public int HarmonicOrder
        {
            get { return harmonicalOrder; }
            set
            {
                harmonicalOrder = Math.Max(1, Math.Min(5, value));

                firstPassEffect.Parameters.Set(SphericalHarmonicsBaseKeys.HarmonicsOrder, harmonicalOrder);
                secondPassEffect.Parameters.Set(SphericalHarmonicsBaseKeys.HarmonicsOrder, harmonicalOrder);
            }
        }

        /// <summary>
        /// Gets the computed spherical harmonics corresponding to the pre-filtered lambertian.
        /// </summary>
        public SphericalHarmonics PrefilteredLambertianSH { get { return prefilteredLambertianSH; } }

        public LambertianPrefilteringSH(ImageEffectContext context)
            : base(context, "LambertianPrefilteringSH")
        {
            firstPassEffect = new ImageEffectShader(context, "LambertianPrefilteringSHPass1");
            secondPassEffect = new ImageEffectShader(context, "LambertianPrefilteringSHPass2");

            HarmonicOrder = 3;
        }

        protected override void DrawCore()
        {
            base.DrawCore();

            // Gets and checks the input texture
            var inputTexture = GetSafeInput(0);
            if (inputTexture.Dimension != TextureDimension.TextureCube)
                throw new InvalidOperationException("Only textures of type 'TextureCube' are valid as input of 'LambertianPrefilteringSH' effect.");
            
            // Gets and checks the output texture
            var outputTexture = GetSafeOutput(0);
            if (outputTexture.Dimension != TextureDimension.TextureCube)
                throw new InvalidOperationException("Only textures of type 'TextureCube' are valid as output of 'LambertianPrefilteringSH' effect.");

            var inputSize = inputTexture.Width; // (Note: for cube maps width = height)
            var sumIterationsPass1 = FindLowerPowerOf2(Math.Sqrt(inputSize));
            var sumIterationsPass2 = inputSize / sumIterationsPass1;

            var pass1TextureSize = sumIterationsPass2 * harmonicalOrder;
            var partialCoefficientTexture = NewScopedRenderTarget2D(pass1TextureSize, pass1TextureSize, PixelFormat.R16G16B16A16_Float);
            var coefficientTexture = NewScopedRenderTarget2D(harmonicalOrder, harmonicalOrder, PixelFormat.R16G16B16A16_Float);

            firstPassEffect.Parameters.Set(LambertianPrefilteringSHPass1Keys.ImageSize, inputSize);
            firstPassEffect.Parameters.Set(LambertianPrefilteringSHPass1Keys.SumIterations, sumIterationsPass1);
            firstPassEffect.SetInput(inputTexture);
            firstPassEffect.SetOutput(partialCoefficientTexture);
            firstPassEffect.Draw("LambertianPrefilteringSHPass1");

            secondPassEffect.Parameters.Set(LambertianPrefilteringSHPass2Keys.SamplesCounts, new Vector2(sumIterationsPass2));
            secondPassEffect.SetInput(partialCoefficientTexture);
            secondPassEffect.SetOutput(coefficientTexture);
            secondPassEffect.Draw("LambertianPrefilteringSHPass2");

            prefilteredLambertianSH = new SphericalHarmonics(HarmonicOrder);
            // todo copy coef from texture to harmonic
        }

        private static int FindLowerPowerOf2(double x)
        {
            return 1 << (int)(Math.Floor(Math.Log(x) / Math.Log(2)));
        }
    }
}