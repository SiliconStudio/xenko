// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        // TODO This class will hold an AnimationCurve<T> later
        //[DataMemberIgnore]
        //public AnimationCurve<T> Animation { get; set; } = new AnimationCurve<T>();

        public TrackingCollection<AnimationKeyFrame<T>> KeyFrames { get; set; } = new TrackingCollection<AnimationKeyFrame<T>>();

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

        public abstract void Cubic(ref T value1, ref T value2, ref T value3, ref T value4, float t, out T result);

        public abstract void Linear(ref T value1, ref T value2, float t, out T result);

        /// <summary>
        /// Unoptimized sampler which searches all the keyframes in order. Intended to be used for baking purposes only
        /// </summary>
        /// <param name="t">Location t to sample at, between 0 and 1</param>
        /// <returns>Sampled and interpolated data value</returns>
        protected T SampleRaw(float t)
        {
            if (KeyFrames.Count <= 0)
                return new T();

            if (Dirty)
            {
                KeyFrames.Sort(this);
                Dirty = false;
            }

            var thisIndex = 0;
            while ((thisIndex < KeyFrames.Count - 1) && (KeyFrames[thisIndex + 1].Key <= t))
                thisIndex++;

            if ((thisIndex >= KeyFrames.Count - 1) || (KeyFrames[thisIndex].Key >= t))
                return KeyFrames[thisIndex].Value;

            var nextIndex = thisIndex + 1;
            if (KeyFrames[thisIndex].Key >= KeyFrames[nextIndex].Key)
                return KeyFrames[thisIndex].Value;

            // Lerp between the two values
            var lerpValue = (t - KeyFrames[thisIndex].Key) / (KeyFrames[nextIndex].Key - KeyFrames[thisIndex].Key);
            T result;

            var leftValue = KeyFrames[thisIndex].Value;
            var rightValue = KeyFrames[nextIndex].Value;

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
