using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Particles.Components
{
    /// <summary>
    /// Add a <see cref="ParticleSystem"/> to an <see cref="Entity"/>
    /// </summary>
    [DataContract("ParticleSystemComponent")]
    [Display(10200, "ParticleSystemComponent", Expand = ExpandRule.Once)]
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
        [DataMember(4)]
        [Display("Test Tint", "This tint description doesn't show on the Property grid")]
        public Color4 Color = Color4.White;

        /// <summary>
        /// The speed scale at which the particle simulation runs.
        /// </summary>
        [DataMember(5)]
        [Display("Test Speed")]
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
