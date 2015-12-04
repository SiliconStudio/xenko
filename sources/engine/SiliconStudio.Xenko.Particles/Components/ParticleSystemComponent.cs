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
        /// The color shade will be applied to all particles (via their materials) during rendering.
        /// The shade acts as a color scale multiplication, making the color darker. White shade is neutral.
        /// </summary>
        [DataMember(4)]
        [Display("Color Shade")]
        public Color4 Color = Color4.White;

        /// <summary>
        /// The speed scale at which the particle simulation runs. Increasing the scale increases the simulation speed,
        /// while setting it to 0 effectively pauses the simulation.
        /// </summary>
        [DataMember(5)]
        [DataMemberRange(0, 10, 0.01, 1)]
        [Display("Speed Scale")]
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
