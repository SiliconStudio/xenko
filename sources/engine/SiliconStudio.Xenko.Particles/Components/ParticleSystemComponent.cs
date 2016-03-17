// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles.Rendering;

namespace SiliconStudio.Xenko.Particles.Components
{
    /// <summary>
    /// Add a <see cref="ParticleSystem"/> to an <see cref="Entity"/>
    /// </summary>
    [DataContract("ParticleSystemComponent")]
    [Display("Particle System", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(ParticleSystemSimulationProcessor))]
    [DefaultEntityComponentRenderer(typeof(ParticleSystemRenderProcessor))]
    [ComponentOrder(10200)]
    public sealed class ParticleSystemComponent : ActivableEntityComponent
    {        
        private ParticleSystem particleSystem;

        /// <summary>
        /// The particle system associated with this component
        /// </summary>
        /// <userdoc>
        /// The Particle System associated with this component
        /// </userdoc>
        [DataMember(10)]
        [Display("Source")]
        public ParticleSystem ParticleSystem
        {
            get
            {
                return particleSystem;
            }
        }

        ~ParticleSystemComponent()
        {
            particleSystem = null;
        }

        [DataMember(1)]
        [Display("Editor control")]
        public ParticleSystemControl Control = new ParticleSystemControl();

        /// <summary>
        /// The color shade will be applied to all particles (via their materials) during rendering.
        /// The shade acts as a color scale multiplication, making the color darker. White shade is neutral.
        /// </summary>
        /// <userdoc>
        /// Color shade (RGBA) will be multiplied to all particles' color in this Particle System
        /// </userdoc>
        [DataMember(4)]
        [Display("Color Shade")]
        public Color4 Color = Color4.White;

        /// <summary>
        /// The speed scale at which the particle simulation runs. Increasing the scale increases the simulation speed,
        /// while setting it to 0 effectively pauses the simulation.
        /// </summary>
        /// <userdoc>
        /// The speed scale at which this Particle System runs the simulation. Set it to 0 to pause it
        /// </userdoc>
        [DataMember(5)]
        [DataMemberRange(0, 10, 0.01, 1)]
        [Display("Speed Scale")]
        public float Speed { get; set; } = 1f;

        public ParticleSystemComponent()
        {
            particleSystem = new ParticleSystem();
        }
    }
}
