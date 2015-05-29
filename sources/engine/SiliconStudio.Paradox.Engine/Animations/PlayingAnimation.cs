// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Animations
{
    [DataContract]
    public class PlayingAnimation
    {
        // Used internally by animation system
        // TODO: Stored in AnimationProcessor?
        internal AnimationClipEvaluator Evaluator;
        internal float[] NodeFactors;
        internal AnimationComponent AnimationComponent;

        internal PlayingAnimation(AnimationComponent animationComponent, string name)
        {
            AnimationComponent = animationComponent;
            IsPlaying = true;
            TimeFactor = 1.0f;
            BlendOperation = AnimationBlendOperation.LinearBlend;
            Name = name;
            Clip = animationComponent.Animations[name];
            RepeatMode = Clip.RepeatMode;
        }

        /// <summary>
        /// Gets or sets the blend operation.
        /// </summary>
        /// <value>
        /// The blend operation.
        /// </value>
        public AnimationBlendOperation BlendOperation { get; set; }

        /// <summary>
        /// Gets or sets the repeat mode.
        /// </summary>
        /// <value>
        /// The repeat mode.
        /// </value>
        public AnimationRepeatMode RepeatMode { get; set; }

        /// <summary>
        /// Gets or sets the animation weight.
        /// </summary>
        /// <value>
        /// The weight.
        /// </value>
        public float Weight { get; set; }

        public string Name { get; private set; }

        public AnimationClip Clip { get; private set; }

        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        /// <value>
        /// The current time.
        /// </value>
        public TimeSpan CurrentTime { get; set; }

        /// <summary>
        /// Gets or sets the playback speed factor.
        /// </summary>
        /// <value>
        /// The playback speed factor.
        /// </value>
        public float TimeFactor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether animation is playing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if animation is playing; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying { get; set; }

        // Animation
        public float WeightTarget { get; set; }
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// Filters the animation to the specified sub-trees given by <see cref="roots"/>.
        /// </summary>
        /// <param name="nodes">The node hierarchy.</param>
        /// <param name="roots">The node roots of sub-trees that should be active (others will be filtered out).</param>
        public void FilterNodes(ModelNodeDefinition[] nodes, params string[] roots)
        {
            // Initialize list of factors (matching nodes list)
            var nodeFactors = new float[nodes.Length];
            for (int index = 0; index < nodes.Length; index++)
            {
                var node = nodes[index];
                if (roots.Contains(node.Name)
                    || (node.ParentIndex != -1 && nodeFactors[node.ParentIndex] == 1.0f))
                {
                    nodeFactors[index] = 1.0f;
                }
            }

            // Update animation channel factors
            var blenderChannels = Evaluator.BlenderChannels;
            var channels = Evaluator.Channels.Items;
            for (int index = 0; index < blenderChannels.Count; index++)
            {
                var blenderChannel = blenderChannels[index];

                // Find node index
                var nodeName = MeshAnimationUpdater.GetNodeName(blenderChannel.PropertyName);
                var nodeIndex = nodes.IndexOf(x => x.Name == nodeName);

                if (nodeIndex != -1)
                {
                    // Update factor
                    channels[index].Factor *= nodeFactors[nodeIndex];
                }
            }
        }
    }
}