// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Particles;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine
{
    // TODO: Add ParticleSystem to SiliconStudio.Xenko.Graphics
    // TODO: Add ParticleSystemComponentRenderer to SiliconStudio.Xenko.Rendering
    // TODO: Add ParticleSystemProcessor to SiliconStudio.Xenko.Rendering.Particles


    /// <summary>
    /// Add a <see cref="ParticleSystem"/> to an <see cref="Entity"/>
    /// </summary>
    [DataContract("ParticleSystemComponent")]
    [Display(10200, "ParticleSystemComponent", Expand = ExpandRule.Once)]
    //    [DefaultEntityComponentRenderer(typeof(ParticleSystemComponentRenderer))]
    //    [DefaultEntityComponentProcessor(typeof(ParticleSystemProcessor))]
    [DefaultEntityComponentRenderer(typeof(ParticleSystemComponentRenderer))]
    [DefaultEntityComponentProcessor(typeof(ParticleSystemProcessor))]


    public sealed class ParticleSystemComponent : ActivableEntityComponent
    {
        public static PropertyKey<ParticleSystemComponent> Key = new PropertyKey<ParticleSystemComponent>("Key", typeof(ParticleSystemComponent));

        /// <summary>
        /// The particle asset data associated to the component.
        /// </summary>
        [DataMember(1)]
        [Display("Test Sprite")]
        public ISpriteProvider TestSprite;

        /// <summary>
        /// Gets the current sprite.
        /// </summary>
        [DataMemberIgnore]
        // Expression-bodied get-only property
        public Sprite CurrentSprite => TestSprite?.GetSprite(0);

        /// <summary>
        /// The particle system data associated to the component.
        /// </summary>
        private ParticleSystem particleSystem;

        [DataMember(10)]
        public ParticleSystem ParticleSystem
        {
            get
            {
                return particleSystem;
            }
        }

        /// <summary>
        /// The tint color to apply to all particles.
        /// </summary>
        [DataMember(40)]
        [Display("Tint")]
        public Color4 Color = Color4.White;

        /// <summary>
        /// The speed scale at which the particle simulation runs.
        /// </summary>
        [DataMember(50)]
        [Display("Speed")]
        public float Speed = 1;

        [DataMemberIgnore]
        internal double ElapsedTime;

        [DataMemberIgnore]
        internal bool IsPaused;

        public ParticleSystemComponent()
        {
            particleSystem = new ParticleSystem();

            TestSprite = new SpriteFromSheet();
        }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}
