// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Add animation capabilities to an <see cref="Entity"/>. It will usually apply to <see cref="ModelComponent.Skeleton"/>
    /// </summary>
    /// <remarks>
    /// Data is stored as in http://altdevblogaday.com/2011/10/23/low-level-animation-part-2/.
    /// </remarks>
    [DataContract("AnimationComponent")]
    [DefaultEntityComponentProcessor(typeof(AnimationProcessor), ExecutionMode = ExecutionMode.Runtime | ExecutionMode.Thumbnail | ExecutionMode.Preview)]
    [Display("Animations", Expand = ExpandRule.Once)]
    [ComponentOrder(2000)]
    public sealed class AnimationComponent : EntityComponent
    {
        private readonly Dictionary<string, AnimationClip> animations;
        private readonly TrackingCollection<PlayingAnimation> playingAnimations;

        [DataMemberIgnore]
        internal AnimationBlender Blender = new AnimationBlender();

        //Please note that this will be gone most likely when we renew the animation system.
        //But for now it's the only way to allow user code to read animation results
        [DataMemberIgnore]
        public AnimationClipResult CurrentFrameResult;

        public AnimationComponent()
        {
            animations = new Dictionary<string, AnimationClip>();
            playingAnimations = new TrackingCollection<PlayingAnimation>();
            playingAnimations.CollectionChanged += playingAnimations_CollectionChanged;
        }

        void playingAnimations_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var item = (PlayingAnimation)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    item.attached = true;
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    var evaluator = item.Evaluator;
                    if (evaluator != null)
                    {
                        Blender.ReleaseEvaluator(evaluator);
                        item.Evaluator = null;
                    }

                    item.endedTCS?.TrySetResult(true);
                    item.endedTCS = null;
                    item.attached = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the animations associated to the component.
        /// </summary>
        /// <userdoc>The list of the animation associated to the entity.</userdoc>
        public Dictionary<string, AnimationClip> Animations
        {
            get { return animations; }
        }

        /// <summary>
        /// Plays right away the animation with the specified name, instantly removing all other blended animations.
        /// </summary>
        /// <param name="name">The animation name.</param>
        public PlayingAnimation Play(string name)
        {
            PlayingAnimations.Clear();
            var playingAnimation = new PlayingAnimation(name, Animations[name]) { CurrentTime = TimeSpan.Zero, Weight = 1.0f };
            PlayingAnimations.Add(playingAnimation);
            return playingAnimation;
        }

        /// <summary>
        /// Crossfades to a new animation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fadeTimeSpan">The fade time span.</param>
        /// <exception cref="ArgumentException">name</exception>
        public PlayingAnimation Crossfade(string name, TimeSpan fadeTimeSpan)
        {
            if (!Animations.ContainsKey(name))
                throw new ArgumentException("name");

            // Fade all animations
            foreach (var otherPlayingAnimation in PlayingAnimations)
            {
                otherPlayingAnimation.WeightTarget = 0.0f;
                otherPlayingAnimation.RemainingTime = fadeTimeSpan;
            }

            // Blend to new animation
            return Blend(name, 1.0f, fadeTimeSpan);
        }

        /// <summary>
        /// Blends progressively a new animation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="desiredWeight">The desired weight.</param>
        /// <param name="fadeTimeSpan">The fade time span.</param>
        /// <exception cref="ArgumentException">name</exception>
        public PlayingAnimation Blend(string name, float desiredWeight, TimeSpan fadeTimeSpan)
        {
            if (!Animations.ContainsKey(name))
                throw new ArgumentException("name");

            var playingAnimation = new PlayingAnimation(name, Animations[name]) { CurrentTime = TimeSpan.Zero, Weight = 0.0f };
            PlayingAnimations.Add(playingAnimation);

            if (fadeTimeSpan > TimeSpan.Zero)
            {
                playingAnimation.WeightTarget = desiredWeight;
                playingAnimation.RemainingTime = fadeTimeSpan;
            }
            else
            {
                playingAnimation.Weight = desiredWeight;
            }

            return playingAnimation;
        }

        public PlayingAnimation NewPlayingAnimation(string name)
        {
            return new PlayingAnimation(name, Animations[name]);
        }

        /// <summary>
        /// Gets list of active animations. Use this to customize startup animations.
        /// </summary>
        /// <userdoc>
        /// Active animations. Use this to customize startup animations.
        /// </userdoc>
        [MemberCollection(CanReorderItems = true)]
        [NotNullItems]
        public TrackingCollection<PlayingAnimation> PlayingAnimations => playingAnimations;
    }
}