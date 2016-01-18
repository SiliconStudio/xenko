// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Animations
{
    [DataContract]
    [Display("KeyFrame")]
    public class AnimationKeyFrame<T> where T : struct
    {
        // TODO structs are not copied properly when edited .. ?

        private T val;
        public T Value { get { return val; } set { val = value; parentCurve?.SetDirty(); } }

        private float key;
        public float Key { get { return key; } set { key = value; parentCurve?.SetDirty(); } }

        // TODO Interpolation technique

        [DataMemberIgnore]
        private ComputeAnimationCurve<T> parentCurve;

        public void SetParent(ComputeAnimationCurve<T> parentCurve)
        {
            this.parentCurve = parentCurve;
        }

    }

    /// <summary>
    /// A node which describes a binary operation between two compute curves
    /// </summary>
    /// <typeparam name="T">Sampled data's type</typeparam>
    [DataContract(Inherited = true)]
    [Display("Animation")]
    [InlineProperty]
    public abstract class ComputeAnimationCurve<T> : Comparer<AnimationKeyFrame<T>>, IComputeCurve<T>  where T : struct
    {
        //[DataMemberIgnore]
        //public AnimationCurve<T> Animation { get; set; } = new AnimationCurve<T>(); // Do we need one?

        public TrackingCollection<AnimationKeyFrame<T>> KeyFrames { get; set; } = new TrackingCollection<AnimationKeyFrame<T>>();

        private const uint bakedArraySize = 32;
        [DataMemberIgnore]
        private T[] bakedArray = new T[bakedArraySize];

        [DataMemberIgnore]
        public bool Dirty { get; set; } = true;
        public void SetDirty()
        {
            Dirty = true;
        }

        /// <summary>
        /// Default constructor. Adds an event for rebaking the curve data when keyframe points change.
        /// </summary>
        protected ComputeAnimationCurve()
        {
            KeyFrames.CollectionChanged += KeyFramesChanged;
        }

        /// <inheritdoc/>
        public override int Compare(AnimationKeyFrame<T> x, AnimationKeyFrame<T> y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return  1;

            return (x.Key < y.Key) ? -1 : (x.Key > y.Key) ? 1 : 0;
        }

        /// <summary>
        /// Called when the keyframes' tracking collection has changed
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void KeyFramesChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            Dirty = true;

            for (var i = 0; i < KeyFrames.Count; i++)
            {
                KeyFrames[i].SetParent(this);
            }
        }

        /// <summary>
        /// Bakes the curve in a fixed-length array for faster access
        /// </summary>
        internal void BakeData()
        {
            if (!Dirty)
                return;

            KeyFrames.Sort(this);

            if (KeyFrames.Count <= 0)
            {
                var emptyValue = new T();
                for (var i = 0; i < bakedArraySize; i++)
                {
                    bakedArray[i] = emptyValue;
                }

                return;
            }

            // By this point we know that (KeyFrames.Count > 0)

            // Bake keyframes into fixed-size array for fast sampling
            var firstIndex = 0;
            var firstKey = KeyFrames[firstIndex].Key;
            var nextIndex = Math.Min(KeyFrames.Count - 1, firstIndex + 1);
            var nextKey = KeyFrames[nextIndex].Key;

            for (var i = 0; i < bakedArraySize; i++)
            {
                var sampleKey = (i/(float)(bakedArraySize - 1));

                while ((sampleKey >= nextKey) && (firstIndex < KeyFrames.Count - 1))
                {
                    firstIndex++;
                    firstKey = KeyFrames[firstIndex].Key;
                    nextIndex = Math.Min(KeyFrames.Count - 1, firstIndex + 1);
                    nextKey = KeyFrames[nextIndex].Key;
                }

                if ((firstIndex == nextIndex) || (firstKey >= sampleKey) || (firstKey >= nextKey) || (Math.Abs(firstKey - nextKey) <= MathUtil.ZeroTolerance))
                {
                    bakedArray[i] = KeyFrames[firstIndex].Value;
                    continue;
                }

                // By this point we know that (firstIndex < nextIndex) and (firstKey < nextKey)

                // TODO Support interpolation methods other than linear
                var lerpValue = (sampleKey - firstKey)/(nextKey - firstKey);
                bakedArray[i] = GetInterpolatedValue(KeyFrames[firstIndex].Value, 1 - lerpValue, KeyFrames[nextIndex].Value, lerpValue);
            }

            Dirty = false;
        }

        public abstract T GetInterpolatedValue(T value1, float weight1, T value2, float weight2);

        /// <inheritdoc/>
        public T SampleAt(float location)
        {
            if (Dirty)
            {
                BakeData();                
            }

            var indexLocation = location * (bakedArraySize - 1);
            var index = (int) indexLocation;
            var lerpValue = indexLocation - index;

            return GetInterpolatedValue(bakedArray[index], 1f - lerpValue, bakedArray[index + 1], lerpValue);
        }
    }
}
