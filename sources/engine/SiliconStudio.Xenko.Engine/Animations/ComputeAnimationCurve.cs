// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public T Value { get { return val; } set { val = value; HasChanged = true; } }

        private float key;
        public float Key { get { return key; } set { key = value; HasChanged = true; } }

        // TODO Interpolation technique

        [DataMemberIgnore]
        public bool HasChanged = true;
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
        // TODO This class will hold an AnimationCurve<T> later
        //[DataMemberIgnore]
        //public AnimationCurve<T> Animation { get; set; } = new AnimationCurve<T>();

        public TrackingCollection<AnimationKeyFrame<T>> KeyFrames { get; set; } = new TrackingCollection<AnimationKeyFrame<T>>();

        // TODO This list will become AnimationCurve<T>
        private FastList<AnimationKeyFrame<T>> sortedKeys = new FastList<AnimationKeyFrame<T>>(); 

        private int framesCount = 0;
        private bool HasChanged()
        {
            if (framesCount != KeyFrames.Count)
                return true;

            for (var i = 0; i < framesCount; i++)
                if (KeyFrames[i].HasChanged)
                    return true;

            return false;
        }

        /// <inheritdoc/>
        public bool UpdateChanges()
        {
            if (!HasChanged())
                return false;

            sortedKeys.Clear();
            sortedKeys.AddRange(KeyFrames.ToArray());
            sortedKeys.Sort(this);

            framesCount = KeyFrames.Count;
            for (var i = 0; i < framesCount; i++)
                KeyFrames[i].HasChanged = false;
            return true;
        }

        /// <inheritdoc/>
        public override int Compare(AnimationKeyFrame<T> x, AnimationKeyFrame<T> y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return  1;

            return (x.Key < y.Key) ? -1 : (x.Key > y.Key) ? 1 : 0;
        }

        public abstract void Cubic(ref T value1, ref T value2, ref T value3, ref T value4, float t, out T result);

        public abstract void Linear(ref T value1, ref T value2, float t, out T result);

        /// <summary>
        /// Unoptimized sampler which searches all the keyframes in order. Intended to be used for baking purposes only
        /// </summary>
        /// <param name="t">Location t to sample at, between 0 and 1</param>
        /// <returns>Sampled and interpolated data value</returns>
        protected T SampleRaw(float t)
        {
            if (sortedKeys.Count <= 0)
                return new T();

            var thisIndex = 0;
            while ((thisIndex < sortedKeys.Count - 1) && (sortedKeys[thisIndex + 1].Key <= t))
                thisIndex++;

            if ((thisIndex >= sortedKeys.Count - 1) || (sortedKeys[thisIndex].Key >= t))
                return sortedKeys[thisIndex].Value;

            var nextIndex = thisIndex + 1;
            if (sortedKeys[thisIndex].Key >= sortedKeys[nextIndex].Key)
                return sortedKeys[thisIndex].Value;

            // Lerp between the two values
            var lerpValue = (t - sortedKeys[thisIndex].Key) / (sortedKeys[nextIndex].Key - sortedKeys[thisIndex].Key);
            T result;

            var leftValue = sortedKeys[thisIndex].Value;
            var rightValue = sortedKeys[nextIndex].Value;

            // TODO Lerp methods other than linear
            Linear(ref leftValue, ref rightValue, lerpValue, out result);
            return result;
        }

        /// <inheritdoc/>
        public T SampleAt(float location)
        {
            return SampleRaw(location);
        }
    }
}
