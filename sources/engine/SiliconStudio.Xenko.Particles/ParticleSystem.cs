// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.BoundingShapes;
using SiliconStudio.Xenko.Particles.DebugDraw;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleSystem")]
    public class ParticleSystem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParticleSystem"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember(-10)]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Should the Particle System's bounds be displayed as a debug draw
        /// </summary>
        /// <userdoc>
        /// Display the Particle System's boinds as a wireframe debug shape. Temporary feature (will be removed later)!
        /// </userdoc>
        [DataMember(-1)]
        [DefaultValue(false)]
        public bool DebugDraw { get; set; } = false;

        private bool ToWorldSpace(ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            scale *= UniformScale;

            rotation *= Rotation;

            Rotation.Rotate(ref translation);
            translation *= UniformScale;
            translation += Translation;

            return true;
        }

        public bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            foreach (var particleEmitter in Emitters)
            {
                foreach (var initializer in particleEmitter.Initializers)
                {
                    if (initializer.DebugDraw && initializer.TryGetDebugDrawShape(ref debugDrawShape, ref translation, ref rotation, ref scale))
                    {
                        // Convert to world space if local
                        if (particleEmitter.SimulationSpace == EmitterSimulationSpace.Local)
                            return ToWorldSpace(ref translation, ref rotation, ref scale);

                        return true;
                    }
                }

                foreach (var updater in particleEmitter.Updaters)
                {
                    if (updater.DebugDraw && updater.TryGetDebugDrawShape(ref debugDrawShape, ref translation, ref rotation, ref scale))
                    {
                        // Convert to world space if local
                        if (particleEmitter.SimulationSpace == EmitterSimulationSpace.Local)
                            return ToWorldSpace(ref translation, ref rotation, ref scale);

                        return true;
                    }
                }
            }

            if (DebugDraw && BoundingShape.TryGetDebugDrawShape(ref debugDrawShape, ref translation, ref rotation, ref scale))
                return ToWorldSpace(ref translation, ref rotation, ref scale);

            return false;
        }

        /// <summary>
        /// AABB of this Particle System
        /// </summary>
        /// <userdoc>
        /// AABB (Axis-Aligned Bounding Box) used for fast culling, optimizations etc. Can be specified by the user
        /// </userdoc>
        [DataMember(5)]
        [NotNull]
        [Display("Bounding Shape")]
        public BoundingShape BoundingShape { get; set; } = new BoundingBoxStatic();

        /// <summary>
        /// Gets the current AABB of the <see cref="ParticleSystem"/>
        /// </summary>
        public BoundingBox GetAABB()
        {
            return BoundingShape?.GetAABB(Translation, Rotation, UniformScale) ?? new BoundingBox(new Vector3(-1), new Vector3(1));
        }

        private readonly SafeList<ParticleEmitter> emitters;
        /// <summary>
        /// List of <see cref="ParticleEmitter"/>
        /// </summary>
        /// <userdoc>
        /// Emitters in this Particle System. Each Emitter has a separate pool (group) of Particles in it
        /// </userdoc>
        [DataMember(10)]
        [Display("Emitters")]
        [NotNullItems]
        [MemberCollection(CanReorderItems = true)]
        public SafeList<ParticleEmitter> Emitters
        {
            get
            {
                return emitters;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ParticleSystem()
        {
            emitters = new SafeList<ParticleEmitter>();
        }

        /// <summary>
        /// Translation of the ParticleSystem. Usually inherited directly from the ParticleSystemComponent.
        /// </summary>
        [DataMemberIgnore]
        public Vector3 Translation = new Vector3(0, 0, 0);

        /// <summary>
        /// Rotation of the ParticleSystem, expressed as a quaternion rotation. Usually inherited directly from the ParticleSystemComponent.
        /// </summary>
        [DataMemberIgnore]
        public Quaternion Rotation = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// Scale of the ParticleSystem. Only uniform scale is supported. Usually inherited directly from the ParticleSystemComponent.
        /// </summary>
        [DataMemberIgnore]
        public float UniformScale = 1f;

        /// <summary>
        /// Updates the particles
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            BoundingShape.Dirty = true;

            foreach (var particleEmitter in Emitters)
            {
                if (particleEmitter.Enabled)
                {
                    particleEmitter.Update(dt, this);
                }
            }            
        }

        /// <summary>
        /// Draws the particles
        /// </summary>
        public void Draw(GraphicsDevice device, RenderContext context, ref Matrix viewMatrix, ref Matrix projMatrix, ref Matrix invViewMatrix, Color4 color)
        {
            foreach (var particleEmitter in Emitters)
            {
                if (particleEmitter.Enabled)
                {
                    try
                    {
                        particleEmitter.Draw(device, context, ref viewMatrix, ref projMatrix, ref invViewMatrix, color);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }

       

    }
}
