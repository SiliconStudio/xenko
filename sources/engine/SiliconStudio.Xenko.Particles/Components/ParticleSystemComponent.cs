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
        [Display("Test Tint")]
        public Color4 Color = Color4.White;

        /// <summary>
        /// The speed scale at which the particle simulation runs.
        /// </summary>
        [DataMember(5)]
        [DataMemberRange(0, 10, 0.01, 1)]
        [Display("Time speed")]
        public float Speed { get; set; } = 1f;

        [DataMemberIgnore]
        internal double ElapsedTime;

        [DataMemberIgnore]
        internal bool IsPaused;

        public ParticleSystemComponent()
        {
            particleSystem = new ParticleSystem();
        }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}
