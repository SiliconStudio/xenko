// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.ComputeEffect.LambertianPrefiltering
{
    /// <summary>
    /// Performs Lambertian pre-filtering in the form of Spherical Harmonics.
    /// </summary>
    public class LambertianPrefilteringSH : ComputeEffect
    {
        private int harmonicalOrder;

        private readonly ComputeEffectShader firstPassEffect;
        private readonly ComputeEffectShader secondPassEffect;

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

                firstPassEffect.Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, harmonicalOrder);
                secondPassEffect.Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, harmonicalOrder);
            }
        }

        /// <summary>
        /// Gets the computed spherical harmonics corresponding to the pre-filtered lambertian.
        /// </summary>
        public SphericalHarmonics PrefilteredLambertianSH { get { return prefilteredLambertianSH; } }

        /// <summary>
        /// Gets or sets the input radiance map to pre-filter.
        /// </summary>
        public Texture RadianceMap { get; set; }

        public LambertianPrefilteringSH(DrawEffectContext context)
            : base(context, "LambertianPrefilteringSH")
        {
            firstPassEffect = new ComputeEffectShader(context) { ShaderSourceName = "LambertianPrefilteringSHEffectPass1" };
            secondPassEffect = new ComputeEffectShader(context) { ShaderSourceName = "LambertianPrefilteringSHEffectPass2" };

            HarmonicOrder = 3;
        }

        protected override void DrawCore()
        {
            base.DrawCore();

            var inputTexture = RadianceMap;
            if (inputTexture == null)
                return;
            
            // Gets and checks the input texture
            if (inputTexture.Dimension != TextureDimension.TextureCube)
                throw new NotSupportedException("Only texture cube are currently supported as input of 'LambertianPrefilteringSH' effect.");

            var faceCount = inputTexture.Dimension == TextureDimension.TextureCube ? 6 : 1;
            var inputSize = inputTexture.Width; // (Note: for cube maps width = height)
            var coefficientsCount = harmonicalOrder * harmonicalOrder;

            var partialSumCount = harmonicalOrder * inputSize * faceCount;
            var partialSumBuffer = NewScopedTypedBuffer(partialSumCount, PixelFormat.R32G32B32A32_Float, true);
            var finalCoeffsBuffer = NewScopedTypedBuffer(coefficientsCount, PixelFormat.R32G32B32A32_Float, true);

            // Project the radiance on the SH basis and sum up the results along the row
            firstPassEffect.ThreadNumbers = new Int3(inputSize, 1, 1);
            firstPassEffect.ThreadGroupCounts = new Int3(1, inputSize, faceCount);
            firstPassEffect.Parameters.Set(LambertianPrefilteringSHParameters.ImageSize, inputSize);
            firstPassEffect.Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, harmonicalOrder);
            firstPassEffect.Parameters.Set(LambertianPrefilteringSHPass1Keys.RadianceMap, inputTexture);
            firstPassEffect.Parameters.Set(LambertianPrefilteringSHPass1Keys.OutputBuffer, partialSumBuffer);
            firstPassEffect.Draw();

            // Complete the partial sums until obtaining finals coefficients
            secondPassEffect.ThreadNumbers = new Int3(1, coefficientsCount, 1);
            secondPassEffect.ThreadGroupCounts = new Int3(inputSize, 1, faceCount);
            secondPassEffect.Parameters.Set(LambertianPrefilteringSHParameters.ImageSize, inputSize);
            secondPassEffect.Parameters.Set(LambertianPrefilteringSHParameters.FaceCount, faceCount);
            secondPassEffect.Parameters.Set(SphericalHarmonicsParameters.HarmonicsOrder, harmonicalOrder);
            secondPassEffect.Parameters.Set(LambertianPrefilteringSHPass2Keys.InputBuffer, partialSumBuffer);
            secondPassEffect.Parameters.Set(LambertianPrefilteringSHPass2Keys.OutputBuffer, finalCoeffsBuffer);
            secondPassEffect.Draw();

            // copy the output coefficients to the output SphericalHarmonics
            prefilteredLambertianSH = new SphericalHarmonics(HarmonicOrder);
            var coefficients = finalCoeffsBuffer.GetData<Vector4>();
            for (int i = 0; i < coefficientsCount; i++)
            {
                var coefficient = coefficients[i];
                prefilteredLambertianSH.Coefficients[i] = new Color3(coefficient.X, coefficient.Y, coefficient.Z);
            }
        }

    }
}