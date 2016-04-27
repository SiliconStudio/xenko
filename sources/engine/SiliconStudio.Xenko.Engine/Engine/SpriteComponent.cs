// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Add a <see cref="Sprite"/> to an <see cref="Entity"/>. It could be an animated sprite.
    /// </summary>
    [DataContract("SpriteComponent")]
    [Display("Sprite", Expand = ExpandRule.Once)]
    [DefaultEntityComponentRenderer(typeof(SpriteRenderProcessor))]
    [ComponentOrder(10000)]
    public sealed class SpriteComponent : ActivableEntityComponent
    {
        /// <summary>
        /// The group of sprites associated to the component.
        /// </summary>
        /// <userdoc>The source of the sprite data.</userdoc>
        [DataMember(5)]
        [Display("Source")]
        [NotNull]
        public ISpriteProvider SpriteProvider { get; set; }

        /// <summary>
        /// The type of the sprite.
        /// </summary>
        /// <userdoc>The type of the sprite. A 3D sprite in the scene or billboard perpendicular to camera view.</userdoc>
        [DataMember(10)]
        [DefaultValue(SpriteType.Sprite)]
        [Display("Type")]
        public SpriteType SpriteType;

        /// <summary>
        /// The color to apply on the sprite.
        /// </summary>
        /// <userdoc>The color to apply to the sprite.</userdoc>
        [DataMember(40)]
        [Display("Color")]
        public Color4 Color = Color4.White;

        /// <summary>
        /// Gets or sets a value indicating whether the sprite is a pre-multiplied alpha (default is true).
        /// </summary>
        /// <value><c>true</c> if the texture is pre-multiplied by alpha; otherwise, <c>false</c>.</value>
        [DataMember(50)]
        [DefaultValue(true)]
        public bool PremultipliedAlpha { get; set; }

        /// <summary>
        /// Ignore the depth of other elements of the scene when rendering the sprite by disabling the depth test.
        /// </summary>
        /// <userdoc>Ignore the depth of other elements of the scene when rendering the sprite. When checked, the sprite is always put on top of previous elements.</userdoc>
        [DataMember(60)]
        [DefaultValue(false)]
        [Display("Ignore Depth")]
        public bool IgnoreDepth;

        [DataMemberIgnore]
        internal double ElapsedTime;

        /// <summary>
        /// Creates a new instance of <see cref="SpriteComponent"/>
        /// </summary>
        public SpriteComponent()
        {
            SpriteProvider = new SpriteFromSheet();
            PremultipliedAlpha = true;
        }

        /// <summary>
        /// Gets the current frame of the animation.
        /// </summary>
        [DataMemberIgnore]
        public int CurrentFrame => (SpriteProvider as SpriteFromSheet)?.CurrentFrame ?? 0;

        /// <summary>
        /// Gets the current sprite.
        /// </summary>
        [DataMemberIgnore]
        public Sprite CurrentSprite => SpriteProvider?.GetSprite();

        private static readonly Queue<List<int>> SpriteIndicesPool = new Queue<List<int>>();

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
    }
}
