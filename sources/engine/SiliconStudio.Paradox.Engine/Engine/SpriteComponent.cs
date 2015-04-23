// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a <see cref="Sprite"/> to an <see cref="Entity"/>. It could be an animated sprite.
    /// </summary>
    [DataContract("SpriteComponent")]
    [Display(100, "Sprite")]
    [DefaultEntityComponentRenderer(typeof(SpriteComponentRenderer))]
    [DefaultEntityComponentProcessor(typeof(SpriteProcessor))]
    public sealed class SpriteComponent : EntityComponent
    {
        public static PropertyKey<SpriteComponent> Key = new PropertyKey<SpriteComponent>("Key", typeof(SpriteComponent));
        
        /// <summary>
        /// The sprites to play.
        /// </summary>
        internal ISpriteProvider SpriteProviderInternal;

        /// <summary>
        /// The group of sprites associated to the component.
        /// </summary>
        /// <userdoc>The source of the sprite data.</userdoc>
        [DataMember(5)]
        [Display("Source")]
        [NotNull]
        public ISpriteProvider SpriteProvider
        {
            get { return SpriteProviderInternal; }
            set
            {
                if(SpriteProviderInternal == value)
                    return;

                SpriteProviderInternal = value;
                CurrentFrame = 0;
            }
        }

        /// <summary>
        /// The type of the sprite.
        /// </summary>
        /// <userdoc>The type of the sprite. A 3D sprite in the scene or billboard perpendicular to camera view.</userdoc>
        [DataMember(10)]
        [DefaultValue(SpriteType.Sprite)]
        [Display("Type")]
        public SpriteType SpriteType;

        /// <summary>
        /// Gets or set the sprite extrusion method.
        /// </summary>
        [DataMember(15)]
        [DefaultValue(SpriteExtrusionMethod.UnitHeightSpriteRatio)]
        [Display("Extrusion")]
        public SpriteExtrusionMethod ExtrusionMethod { get; set; }

        /// <summary>
        /// The color to apply on the sprite.
        /// </summary>
        /// <userdoc>The color to apply to the sprite.</userdoc>
        [DataMember(20)]
        [Display("Color")]
        public Color Color = Color.White;

        /// <summary>
        /// Gets or sets a value indicating whether the sprite is a premultiplied alpha (default is true).
        /// </summary>
        /// <value><c>true</c> if the texture is premultiplied by alpha; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(true)]
        public bool PremultipliedAlpha { get; set; }

        [DataMemberIgnore]
        internal double ElapsedTime;

        /// <summary>
        /// Creates a new instance of <see cref="SpriteComponent"/>
        /// </summary>
        public SpriteComponent()
        {
            SpriteProviderInternal = new SpriteFromSpriteGroup();
            ExtrusionMethod = SpriteExtrusionMethod.UnitHeightSpriteRatio;
            PremultipliedAlpha = true;
        }

        /// <summary>
        /// Gets or sets the current frame of the animation.
        /// </summary>
        [DataMemberIgnore]
        public int CurrentFrame { get; set; }

        /// <summary>
        /// Gets the current sprite.
        /// </summary>
        [DataMemberIgnore]
        public Sprite CurrentSprite
        {
            get
            {
                if (SpriteProviderInternal == null)
                    return null;

                return SpriteProviderInternal.GetSprite(CurrentFrame);
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

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}