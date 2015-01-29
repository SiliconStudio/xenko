// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a <see cref="Sprite"/> to an <see cref="Entity"/>. It could be an animated sprite.
    /// </summary>
    [DataContract("SpriteComponent")]
    [Display(100, "Sprite")]
    public sealed class SpriteComponent : EntityComponent
    {
        public static PropertyKey<SpriteComponent> Key = new PropertyKey<SpriteComponent>("Key", typeof(SpriteComponent));

        /// <summary>
        /// The sprites to play.
        /// </summary>
        internal SpriteGroup SpriteGroupInternal;

        /// <summary>
        /// The color to apply on the sprite.
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// The effect to apply on the sprite.
        /// </summary>
        public SpriteEffects SpriteEffect;

        /// <summary>
        /// The effect to use to render the sprite.
        /// </summary>
        [DataMemberCustomSerializer]
        public Effect Effect;

        [DataMemberIgnore]
        internal double ElapsedTime;

        private int currentFrame;

        /// <summary>
        /// The group of sprites associated to the component.
        /// </summary>
        public SpriteGroup SpriteGroup
        {
            get { return SpriteGroupInternal; }
            set
            {
                if(SpriteGroupInternal == value)
                    return;

                SpriteGroupInternal = value;
                CurrentFrame = 0;
            }
        }

        /// <summary>
        /// Gets or sets the current frame of the animation.
        /// </summary>
        [DataMemberIgnore]
        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                if (SpriteGroupInternal == null || SpriteGroupInternal.Images == null || SpriteGroupInternal.Images.Count == 0)
                {
                    currentFrame = 0;
                    return;
                }

                currentFrame = Math.Max(0, value % SpriteGroupInternal.Images.Count);
            }
        }

        /// <summary>
        /// Gets the current sprite.
        /// </summary>
        [DataMemberIgnore]
        public Sprite CurrentSprite
        {
            get
            {
                if (SpriteGroupInternal == null || SpriteGroupInternal.Images == null)
                    return null;

                return SpriteGroupInternal.Images[Math.Min(currentFrame, SpriteGroupInternal.Images.Count)];
            }
        }

        private readonly static Queue<List<int>> SpriteIndicesPool = new Queue<List<int>>();

        [DataMemberIgnore]
        internal double AnimationTime;

        [DataMemberIgnore]
        internal int CurrentIndexIndex;

        [DataMemberIgnore]
        internal bool IsPaused;

        internal struct AnimationInfo
        {
            public float FramePerSeconds;

            public bool ShouldLoop;

            public List<int> SpriteIndices;
        }

        internal Queue<AnimationInfo> Animations = new Queue<AnimationInfo>();

        internal static List<int> GetNewSpriteIndicesList()
        {
            lock (SpriteIndicesPool)
            {
                return SpriteIndicesPool.Count > 0 ? SpriteIndicesPool.Dequeue() : new List<int>();
            }
        }

        internal static void RecycleSpriteIndicesList(List<int> indicesList)
        {
            lock (SpriteIndicesPool)
            {
                indicesList.Clear();
                SpriteIndicesPool.Enqueue(indicesList);
            }
        }

        internal void ClearAnimations()
        {
            lock (SpriteIndicesPool)
            {
                while (Animations.Count>0)
                    RecycleSpriteIndicesList(Animations.Dequeue().SpriteIndices);
            }
        }

        internal void RecycleFirstAnimation()
        {
            CurrentIndexIndex = 0;
            if (Animations.Count > 0)
            {
                var info = Animations.Dequeue();
                RecycleSpriteIndicesList(info.SpriteIndices);
            }
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}