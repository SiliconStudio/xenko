// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Animations
{
    [DataContract]
    public class PlayingAnimation
    {
        private static readonly object Lock = new object();

        // Used internally by animation system
        // TODO: Stored in AnimationProcessor?
        internal AnimationClipEvaluator Evaluator;
        internal float[] NodeFactors;
        internal TaskCompletionSource<bool> endedTCS;
        internal bool attached; // Is it part of a AnimationComponent.PlayingAnimations collection?

        internal PlayingAnimation(string name, AnimationClip clip) : this()
        {
            Name = name;
            Clip = clip;
            RepeatMode = Clip.RepeatMode;
        }

        public PlayingAnimation()
        {
            Enabled = true;
            TimeFactor = 1.0f;
            Weight = 1.0f;
            BlendOperation = AnimationBlendOperation.LinearBlend;
            RepeatMode = AnimationRepeatMode.LoopInfinite;
        }

        /// <summary>
        /// Gets or sets a value indicating whether animation is playing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if animation is playing; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        [DataMember(10)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of this playing animation (optional).
        /// </summary>
        /// <userdoc>
        /// The name of this playing animation (optional).
        /// </userdoc>
        [DataMember(20)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the animation clip to run
        /// </summary>
        /// <userdoc>
        /// The clip being played.
        /// </userdoc>
        [DataMember(30)]
        public AnimationClip Clip { get; set; }

        /// <summary>
        /// Gets or sets the repeat mode.
        /// </summary>
        /// <value>
        /// The repeat mode.
        /// </value>
        [DataMember(40)]
        public AnimationRepeatMode RepeatMode { get; set; }

        /// <summary>
        /// Gets or sets the blend operation.
        /// </summary>
        /// <value>
        /// The blend operation.
        /// </value>
        [DataMember(50)]
        public AnimationBlendOperation BlendOperation { get; set; }

        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        /// <userdoc>
        /// The current time when playing the animation.
        /// </userdoc>
        [DataMember(60)]
        public TimeSpan CurrentTime { get; set; }

        /// <summary>
        /// Gets or sets the playback speed factor.
        /// </summary>
        /// <userdoc>
        /// The playback speed factor.
        /// </userdoc>
        [DataMember(70)]
        [DefaultValue(1.0f)]
        public float TimeFactor { get; set; }

        /// <summary>
        /// Gets or sets the animation weight.
        /// </summary>
        /// <value>
        /// The animation weight.
        /// </value>
        [DataMember(80)]
        [DefaultValue(1.0f)]
        public float Weight { get; set; }

        // Animation (not exposed until stabilized)
        [DataMemberIgnore]
        public float WeightTarget { get; set; }
        [DataMemberIgnore]
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// Returns an awaitable object that will be completed when the animation is removed from the PlayingAnimation list.
        /// This happens when:
        /// - RepeatMode is PlayOnce and animation reached end
        /// - Animation faded out completely (due to blend to 0.0 or crossfade out)
        /// - Animation was manually removed from AnimationComponent.PlayingAnimations
        /// </summary>
        /// <returns></returns>
        public Task Ended()
        {
            if (!attached)
                throw new InvalidOperationException("Trying to await end of an animation which is not playing");

            lock (Lock)
            {
                if (endedTCS == null)
                    endedTCS = new TaskCompletionSource<bool>();
            }

            return endedTCS.Task;
        }
    }
}