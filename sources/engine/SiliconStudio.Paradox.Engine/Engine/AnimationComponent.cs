// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add animation capabilities to an <see cref="Entity"/>. It will usually apply to <see cref="ModelComponent.ModelViewHierarchy"/>
    /// </summary>
    /// <remarks>
    /// Data is stored as in http://altdevblogaday.com/2011/10/23/low-level-animation-part-2/.
    /// </remarks>
    [DataContract("AnimationComponent")]
    [Display(20, "Animation")]
    [DefaultEntityComponentProcessor(typeof(AnimationProcessor))]
    public sealed class AnimationComponent : EntityComponent
    {
        private readonly Dictionary<string, AnimationClip> animations;
        private readonly TrackingCollection<PlayingAnimation> playingAnimations;

        [DataMemberIgnore]
        internal AnimationBlender Blender = new AnimationBlender();

        public static PropertyKey<AnimationComponent> Key = new PropertyKey<AnimationComponent>("Key", typeof(AnimationComponent));

        public AnimationComponent()
        {
            animations = new Dictionary<string, AnimationClip>();
            playingAnimations = new TrackingCollection<PlayingAnimation>();
            playingAnimations.CollectionChanged += playingAnimations_CollectionChanged;
        }

        void playingAnimations_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = (PlayingAnimation)e.Item;
                var evaluator = item.Evaluator;
                if (evaluator != null)
                {
                    Blender.ReleaseEvaluator(evaluator);
                    item.Evaluator = null;
                }
            }
        }

        public Dictionary<string, AnimationClip> Animations
        {
            get { return animations; }
        }

        /// <summary>
        /// Plays right away the animation with the specified name, instantly removing all other blended animations.
        /// </summary>
        /// <param name="name">The animation name.</param>
        public void Play(string name)
        {
            PlayingAnimations.Clear();
            PlayingAnimations.Add(new PlayingAnimation(this, name) { CurrentTime = TimeSpan.Zero, Weight = 1.0f });
        }

        /// <summary>
        /// Crossfades to a new animation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fadeTimeSpan">The fade time span.</param>
        /// <exception cref="ArgumentException">name</exception>
        public void Crossfade(string name, TimeSpan fadeTimeSpan)
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
            Blend(name, 1.0f, fadeTimeSpan);
        }

        /// <summary>
        /// Blends progressively a new animation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="desiredWeight">The desired weight.</param>
        /// <param name="fadeTimeSpan">The fade time span.</param>
        /// <exception cref="ArgumentException">name</exception>
        public void Blend(string name, float desiredWeight, TimeSpan fadeTimeSpan)
        {
            if (!Animations.ContainsKey(name))
                throw new ArgumentException("name");

            var playingAnimation = new PlayingAnimation(this, name) { CurrentTime = TimeSpan.Zero, Weight = 0.0f };
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
        }

        public PlayingAnimation NewPlayingAnimation(string name)
        {
            return new PlayingAnimation(this, name);
        }

        [DataMemberIgnore]
        public TrackingCollection<PlayingAnimation> PlayingAnimations
        {
            get { return playingAnimations; }
        }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}