// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Engine
{
    [Display("Particle Emitter")]
    public class ParticleEmitterComponent : EntityComponent
    {
        public static PropertyKey<ParticleEmitterComponent> Key = new PropertyKey<ParticleEmitterComponent>("Key", typeof(ParticleEmitterComponent));

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleEmitterComponent" /> class.
        /// </summary>
        public ParticleEmitterComponent()
        {
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets or sets the particle count.
        /// </summary>
        /// <value>The particle count.</value>
        [DataMemberConvert]
        [Display]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the type of this emitter..
        /// </summary>
        /// <value>The type.</value>
        [DataMemberConvert]
        [Display]
        public ParticleEmitterType Type { get; set; }

        /// <summary>
        /// Gets or sets the shader.
        /// </summary>
        /// <value>The shader.</value>
        [DataMemberConvert]
        public ShaderClassSource Shader { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// Gets or sets the size of the particle element.
        /// </summary>
        /// <value>The size of the particle element.</value>
        public int ParticleElementSize { get; set; }

        /// <summary>
        /// Gets or sets the particle data.
        /// </summary>
        /// <value>The particle data.</value>
        public Array ParticleData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [update next buffer].
        /// </summary>
        /// <value><c>true</c> if [update next buffer]; otherwise, <c>false</c>.</value>
        public bool UpdateNextBuffer { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a dynamic emitter.
        /// </summary>
        /// <value><c>true</c> if this instance is a dynamic emitter; otherwise, <c>false</c>.</value>
        public bool IsDynamicEmitter
        {
            get
            {
                return Type == ParticleEmitterType.CpuDynamic || Type == ParticleEmitterType.GpuDynamic;
            }
        }

        public event Action<ParticleEmitterComponent> UpdateData;

        public void OnUpdateData()
        {
            var updateData = UpdateData;
            if (updateData != null) updateData(this);
        }

        public event Action<ParticleEmitterComponent> UpdateSystem;

        public void OnUpdateSystem()
        {
            var updateSystem = UpdateSystem;
            if (updateSystem != null) updateSystem(this);
        }

        /// <summary>
        /// A callback called whenever the component is updated by the ParticleSystem.
        /// </summary>
        public event Action<ParticleEmitterComponent, Entity, TrackingCollectionChangedEventArgs> MeshUpdate;

        public virtual void OnAddToSystem(IServiceRegistry registry)
        {
            
        }

        public void OnMeshUpdate(Entity entity, TrackingCollectionChangedEventArgs updateArgs)
        {
            var update = MeshUpdate;
            if (update != null) update(this, Entity, updateArgs);
        }

        public static int CalculateMaximumPowerOf2Count(int value)
        {
            return (int)Math.Pow(2.0, Math.Ceiling(Math.Log(value, 2)));
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}