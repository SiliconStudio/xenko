// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Animations
{
    /// <summary>
    /// An aggregation of <see cref="AnimationCurve"/> with their channel names.
    /// </summary>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<AnimationClip>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<AnimationClip>), Profile = "Asset")]
    public sealed class AnimationClip
    {
        // If there is an evaluator, animation clip can't be changed anymore.
        internal bool Frozen;

        /// <summary>
        /// Gets or sets the duration of this clip.
        /// </summary>
        /// <value>
        /// The duration of this clip.
        /// </value>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the repeat mode of the <see cref="AnimationClip"/>.
        /// </summary>
        public AnimationRepeatMode RepeatMode { get; set; }

        /// <summary>
        /// Gets the channels of this clip.
        /// </summary>
        /// <value>
        /// The channels of this clip.
        /// </value>
        public Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();

        // TODO: The curve stored inside should be internal/private (it is public now to avoid implementing custom serialization before first release).
        public List<AnimationCurve> Curves = new List<AnimationCurve>();

        public AnimationData<float> OptimizedCurvesFloat;
        public AnimationData<Vector3> OptimizedCurvesVector3;
        public AnimationData<Quaternion> OptimizedCurvesQuaternion;

        /// <summary>
        /// Adds a named curve.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="curve">The curve.</param>
        public void AddCurve(string propertyName, AnimationCurve curve)
        {
            if (Frozen)
                throw new InvalidOperationException("This AnimationClip is frozen");

            // Add channel
            Channels.Add(propertyName, new Channel
            {
                NodeName = MeshAnimationUpdater.GetNodeName(propertyName),
                Type = MeshAnimationUpdater.GetType(propertyName),
                CurveIndex = Curves.Count,
                ElementType = curve.ElementType,
                ElementSize = curve.ElementSize,
            });
            Curves.Add(curve);
        }

        /// <summary>
        /// Optimizes data from multiple curves to a single linear data stream.
        /// </summary>
        public void Optimize()
        {
            Freeze();

            OptimizedCurvesFloat = CreateOptimizedData<float>();
            OptimizedCurvesVector3 = CreateOptimizedData<Vector3>();
            OptimizedCurvesQuaternion = CreateOptimizedData<Quaternion>();
        }

        private AnimationData<T> CreateOptimizedData<T>()
        {
            // Find Vector3 channels
            var curves = Channels
                .Where(x => x.Value.CurveIndex != -1 && x.Value.ElementType == typeof(T))
                .ToDictionary(x => x.Key, x => (AnimationCurve<T>)Curves[x.Value.CurveIndex]);

            // Update channels
            foreach (var curve in curves)
            {
                var channel = Channels[curve.Key];

                // CurveIndex -1 means there is optimized data
                if (channel.CurveIndex != -1)
                {
                    Curves[channel.CurveIndex] = null;
                    channel.CurveIndex = -1;
                }

                Channels[curve.Key] = channel;
            }

            return AnimationData<T>.FromAnimationChannels(curves);
        }

        internal void Freeze()
        {
            Frozen = true;
        }

        [DataContract]
        public struct Channel
        {
            public string NodeName;
            public MeshAnimationUpdater.ChannelType Type;

            public int CurveIndex;
            public Type ElementType;
            public int ElementSize;
        }
    }
}