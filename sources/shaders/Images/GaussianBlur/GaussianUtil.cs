// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Images
{
    internal class GaussianMacros
    {
        public GaussianMacros(Vector2[] defines)
        {
            this.defines = defines;
            Count = defines.Length.ToString(CultureInfo.InvariantCulture);
            Dump(defines, out Offsets, out Weights);
        }

        private readonly Vector2[] defines;

        public readonly string Count;

        public readonly string Offsets;

        public readonly string Weights;

        private static void Dump(Vector2[] defines, out string offsets, out string weights)
        {
            var offsetsBuffer = new StringBuilder(defines.Length * 10);
            var weightsBuffer = new StringBuilder(defines.Length * 10);
            for (int i = 0; i < defines.Length; i++)
            {
                if (i > 0)
                {
                    offsetsBuffer.Append(',');
                    weightsBuffer.Append(',');
                }
                offsetsBuffer.Append(defines[i].X.ToString(CultureInfo.InvariantCulture));
                weightsBuffer.Append(defines[i].Y.ToString(CultureInfo.InvariantCulture));
            }
            offsets = offsetsBuffer.ToString();
            weights = weightsBuffer.ToString();
        }
    }


    /// <summary>
    /// Utility class to calculate 1D Gaussian filter used for separable 2D Gaussian filters.
    /// </summary>
    internal class GaussianUtil
    {
        private readonly static Dictionary<GaussianKey, GaussianMacros> GaussianMacros = new Dictionary<GaussianKey,GaussianMacros>();

        /// <summary>
        /// Gets the blur macros defined for the specified radius and LDR boolean.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="isLDR">if set to <c>true</c> the <c>sigma = radius / 2.0f</c>; otherwise <c>sigma = radius = 3.0f;</c>.</param>
        /// <returns>A GaussianMacros for effect compilation.</returns>
        public static GaussianMacros GetBlurMacros(int radius, bool isLDR)
        {
            GaussianMacros result;
            lock (GaussianMacros)
            {
                var key = new GaussianKey(radius, isLDR);
                if (!GaussianMacros.TryGetValue(key, out result))
                {
                    result = new GaussianMacros(Calculate1D(radius, isLDR));
                    GaussianMacros.Add(key, result);
                }
            }
            return result;
        }


        /// <summary>
        /// Calculate a 1D gaussian filter.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="isLDR">if set to <c>true</c> the <c>sigma = radius / 2.0f</c>; otherwise <c>sigma = radius = 3.0f;</c>.</param>
        /// <returns>An array of offsets (<see cref="Vector2.X"/>) and weights (<see cref="Vector2.Y"/>).</returns>
        public static Vector2[] Calculate1D(int radius, bool isLDR)
        {
            if (radius < 1)
                radius = 1;

            var sigma = radius / (isLDR ? 2.0f : 3.0f);
            return Calculate1D(radius, sigma);
        }

        /// <summary>
        /// Calculate a 1D gaussian filter.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="sigma">The sigma.</param>
        /// <param name="disableBilinear">if set to <c>true</c> to disable bilinear offsets/weights.</param>
        /// <returns>An array of offsets (<see cref="Vector2.X" />) and weights (<see cref="Vector2.Y" />).</returns>
        public static Vector2[] Calculate1D(int radius, float sigma, bool disableBilinear = false)
        {
            if (radius < 1)
                radius = 1;

            // Precalculate a default sigma if not specified
            if (MathUtil.IsZero(sigma))
            {
                sigma = radius / 3.0f;
            }

            var sigma2 = sigma * sigma;
            var sigmaDivInner = 1.0 / (2.0 * sigma2);

            double total = 1.0;

            Vector2[] offsetsWeights;
            double[] localWeights;

            if (disableBilinear)
            {
                int count = radius + 1;
                offsetsWeights = new Vector2[count];
                localWeights = new double[count];

                for (int i = 1; i <= radius; i++)
                {
                    var weight = Math.Exp(-Math.Pow(i, 2) * sigmaDivInner);
                    offsetsWeights[i].X = i;
                    localWeights[i] = weight;
                    total += 2.0 * weight;  // weight*2 because offsets/weights are mirrored
                }
            }
            else
            {
                // Calculate offsets and weights with LinearSampling
                int count = radius / 2 + 1;
                offsetsWeights = new Vector2[count];
                localWeights = new double[count];
                int index = 1;

                for (int i = 1; i <= radius; i += 2)
                {
                    double w0 = Math.Exp(-Math.Pow(i, 2) * sigmaDivInner);
                    double offset;
                    double weight;

                    if (i == radius)
                    {
                        // edge case
                        offset = i;
                        weight = w0;
                    }
                    else
                    {
                        // Calculate offsets & weights with bilinear filtering
                        double w1 = Math.Exp(-Math.Pow(i + 1, 2) * sigmaDivInner);
                        offset = (i + w1 / (w0 + w1));
                        weight = (w0 + w1);
                    }
                    offsetsWeights[index].X = (float)offset;
                    localWeights[index] = weight;
                    total += 2.0 * weight;  // weight*2 because offsets/weights are mirrored
                    index++;
                }
            }

            // Mormalize weights so that sum is 1.0 (normal distribution, energy conservative)
            localWeights[0] = 1.0;
            for (int i = 0; i < localWeights.Length; i++)
                offsetsWeights[i].Y = (float)(localWeights[i] / total);

            return offsetsWeights;
        }

        private struct GaussianKey : IEquatable<GaussianKey>
        {
            public GaussianKey(int radius, bool isLDR)
            {
                Radius = radius;
                IsLDR = isLDR;
            }

            public readonly int Radius;

            public readonly bool IsLDR;

            public bool Equals(GaussianKey other)
            {
                return Radius == other.Radius && IsLDR.Equals(other.IsLDR);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is GaussianKey && Equals((GaussianKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Radius * 397) ^ IsLDR.GetHashCode();
                }
            }
        }
    }
}