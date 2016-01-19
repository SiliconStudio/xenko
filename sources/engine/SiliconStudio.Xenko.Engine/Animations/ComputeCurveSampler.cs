// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// Base interface for curve based compute value nodes.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class ComputeCurveSampler<T> where T : struct
    {
        [NotNull]
        [Display("Curve")]
        public IComputeCurve<T> Curve { get; set; }

        protected ComputeCurveSampler()
        {
            bakedArray = new T[bakedArraySize];
            BakeData();
        }

        /// <summary>
        /// Samples the compute curve at the specified location, usually in the [0 .. 1] range
        /// </summary>
        /// <param name="location">Location to sample at</param>
        /// <returns>Sampled value</returns>
        public T SampleAt(float t)
        {
            //return Curve?.SampleAt(t) ?? new T();

            var indexLocation = t * (bakedArraySize - 1);
            var index = (int)indexLocation;
            var lerpValue = indexLocation - index;

            T result;
            var thisIndex = (int) Math.Max(index, 0);
            var nextIndex = (int)Math.Min(index + 1, bakedArraySize - 1);
            Linear(ref bakedArray[thisIndex], ref bakedArray[nextIndex], lerpValue, out result);
            return result;
        }

        /// <summary>
        /// Interface for linera interpolation between two data values
        /// </summary>
        /// <param name="value1">Left value</param>
        /// <param name="value2">Right value</param>
        /// <param name="t">Lerp amount between 0 and 1</param>
        /// <param name="result">The interpolated result of linearLerp(L, R, t)</param>
        public abstract void Linear(ref T value1, ref T value2, float t, out T result);

        // TODO Maybe make it variable length/density
        private const uint bakedArraySize = 32;
        /// <summary>
        /// Data in this sampler can be baked to allow faster sampling
        /// </summary>
        [DataMemberIgnore]
        private T[] bakedArray;

        /// <summary>
        /// Bakes the sampled data in a fixed size array for faster access
        /// </summary>
        private void BakeData()
        {
            if (Curve == null)
            {
                var emptyValue = new T();
                for (var i = 0; i < bakedArraySize; i++)
                {
                    bakedArray[i] = emptyValue;
                }

                return;
            }

            for (var i = 0; i < bakedArraySize; i++)
            {
                var t = i/(float)(bakedArraySize - 1);
                bakedArray[i] = Curve.SampleAt(t);
            }
        }

        public bool UpdateChanges()
        {
            // TODO Check if it needs updating
            BakeData();
            return true;
        }
    }
}
