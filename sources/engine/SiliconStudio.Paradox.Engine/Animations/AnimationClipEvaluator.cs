// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Animations
{
    /// <summary>
    /// Evaluates <see cref="AnimationClip"/> to a <see cref="AnimationClipResult"/> at a given time.
    /// </summary>
    public sealed class AnimationClipEvaluator
    {
        private AnimationClip clip;
        internal List<AnimationBlender.Channel> BlenderChannels;

        private AnimationCurveEvaluatorDirectFloatGroup curveEvaluatorFloat = new AnimationCurveEvaluatorDirectFloatGroup();
        private AnimationCurveEvaluatorDirectVector3Group curveEvaluatorVector3 = new AnimationCurveEvaluatorDirectVector3Group();
        private AnimationCurveEvaluatorDirectQuaternionGroup curveEvaluatorQuaternion = new AnimationCurveEvaluatorDirectQuaternionGroup();

        private AnimationCurveEvaluatorOptimizedFloatGroup curveEvaluatorOptimizedFloat;
        private AnimationCurveEvaluatorOptimizedVector3Group curveEvaluatorOptimizedVector3;
        private AnimationCurveEvaluatorOptimizedQuaternionGroup curveEvaluatorOptimizedQuaternion;

        // Temporarily exposed for MeshAnimationUpdater
        internal FastListStruct<EvaluatorChannel> Channels = new FastListStruct<EvaluatorChannel>(4);

        public AnimationClip Clip
        {
            get { return clip; }
        }

        internal AnimationClipEvaluator()
        {
            
        }

        internal void Initialize(AnimationClip clip, List<AnimationBlender.Channel> channels)
        {
            this.BlenderChannels = channels;
            this.clip = clip;
            clip.Freeze();

            // If there are optimized curve data, instantiate appropriate evaluators
            if (clip.OptimizedCurvesFloat != null)
            {
                if (curveEvaluatorOptimizedFloat == null)
                    curveEvaluatorOptimizedFloat = new AnimationCurveEvaluatorOptimizedFloatGroup();
                curveEvaluatorOptimizedFloat.Initialize(clip.OptimizedCurvesFloat);
            }

            if (clip.OptimizedCurvesVector3 != null)
            {
                if (curveEvaluatorOptimizedVector3 == null)
                    curveEvaluatorOptimizedVector3 = new AnimationCurveEvaluatorOptimizedVector3Group();
                curveEvaluatorOptimizedVector3.Initialize(clip.OptimizedCurvesVector3);
            }

            if (clip.OptimizedCurvesQuaternion != null)
            {
                if (curveEvaluatorOptimizedQuaternion == null)
                    curveEvaluatorOptimizedQuaternion = new AnimationCurveEvaluatorOptimizedQuaternionGroup();
                curveEvaluatorOptimizedQuaternion.Initialize(clip.OptimizedCurvesQuaternion);
            }

            // Add already existing channels
            for (int index = 0; index < channels.Count; index++)
            {
                var channel = channels[index];
                AddChannel(ref channel);
            }
        }

        internal void Cleanup()
        {
            if (curveEvaluatorOptimizedVector3 != null)
                curveEvaluatorOptimizedVector3.Cleanup();
            if (curveEvaluatorOptimizedQuaternion != null)
                curveEvaluatorOptimizedQuaternion.Cleanup();

            Channels.Clear();
            BlenderChannels = null;
            clip = null;
        }

        public unsafe void Compute(CompressedTimeSpan newTime, AnimationClipResult result)
        {
            fixed (byte* structures = result.Data)
            {
                // Update factors
                for (int index = 0; index < Channels.Count; index++)
                {
                    // For now, objects are not supported, so treat everything as a blittable struct.
                    var channel = Channels.Items[index];

                    var structureStart = (float*)(structures + channel.Offset);

                    // Write a float specifying channel factor (1 if exists, 0 if doesn't exist)
                    *structureStart = channel.Factor;
                }

                if (curveEvaluatorOptimizedVector3 != null)
                {
                    curveEvaluatorOptimizedVector3.Evaluate(newTime, (IntPtr)structures);
                }

                if (curveEvaluatorOptimizedQuaternion != null)
                {
                    curveEvaluatorOptimizedQuaternion.Evaluate(newTime, (IntPtr)structures);
                }

                // Write interpolated data
                curveEvaluatorFloat.Evaluate(newTime, (IntPtr)structures);
                curveEvaluatorVector3.Evaluate(newTime, (IntPtr)structures);
                curveEvaluatorQuaternion.Evaluate(newTime, (IntPtr)structures);
            }
        }

        public unsafe void AddCurveValues(CompressedTimeSpan newTime, AnimationClipResult result)
        {
            fixed (byte* structures = result.Data)
            {
                for (int index = 0; index < Channels.Count; index++)
                {
                    var channel = Channels.Items[index];

                    // For now, objects are not supported, so treat everything as a blittable struct.
                    channel.Curve.AddValue(newTime, (IntPtr)(structures + channel.Offset));
                }
            }
        }

        internal void AddChannel(ref AnimationBlender.Channel channel)
        {
            AnimationClip.Channel clipChannel;
            AnimationCurve curve = null;

            // Try to find curve and create evaluator
            // (if curve doesn't exist, Evaluator will be null).
            bool itemFound = clip.Channels.TryGetValue(channel.PropertyName, out clipChannel);

            if (itemFound)
            {
                if (clipChannel.CurveIndex != -1)
                {
                    curve = clip.Curves[clipChannel.CurveIndex];
                    if (clipChannel.ElementType == typeof(Vector3))
                        curveEvaluatorVector3.AddChannel(curve, channel.Offset + sizeof(float));
                    else if (clipChannel.ElementType == typeof(Quaternion))
                        curveEvaluatorQuaternion.AddChannel(curve, channel.Offset + sizeof(float));
                    else
                        throw new NotImplementedException("Can't create evaluator with this type of curve channel.");
                }
                else
                {
                    if (clipChannel.ElementType == typeof(Vector3))
                        curveEvaluatorOptimizedVector3.AddChannel(channel.PropertyName, channel.Offset + sizeof(float));
                    else if (clipChannel.ElementType == typeof(Quaternion))
                        curveEvaluatorOptimizedQuaternion.AddChannel(channel.PropertyName, channel.Offset + sizeof(float));
                }
            }

            Channels.Add(new EvaluatorChannel { Offset = channel.Offset, Curve = curve, Factor = itemFound ? 1.0f : 0.0f });
        }

        internal struct EvaluatorChannel
        {
            public int Offset;
            public AnimationCurve Curve;
            public float Factor;
        }
    }
}