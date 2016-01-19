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

        /// <summary>
        /// Fixes local space location back to world space location. Used for debug drawing.
        /// </summary>
        /// <param name="translation">The locator's translation</param>
        /// <param name="rotation">The locator's quaternion rotation</param>
        /// <param name="scale">The locator's non-uniform scaling</param>
        /// <returns></returns>
        private bool ToWorldSpace(ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            scale *= UniformScale;

            rotation *= Rotation;

            Rotation.Rotate(ref translation);
            translation *= UniformScale;
            translation += Translation;

            return true;
        }

        /// <summary>
        /// Tries to acquire and draw a debug shape for better feedback and visualization.
        /// </summary>
        /// <param name="debugDrawShape">The type of the debug shape (sphere, cone, etc.)</param>
        /// <param name="translation">The shape's translation</param>
        /// <param name="rotation">The shape's rotation</param>
        /// <param name="scale">The shape's non-uniform scaling</param>
        /// <returns><c>true</c> if debug shape can be displayed</returns>
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
        /// List of Emitters in this <see cref="ParticleSystem"/>. Each Emitter has a separate <see cref="ParticlePool"/> (group) of Particles in it
        /// </summary>
        /// <userdoc>
        /// List of emitters in this particle system. Each Emitter has a separate particle pool (group) of particles in it
        /// </userdoc>
        [DataMember(10)]
        [Display("Emitters")]
        //[NotNullItems] // This attribute is not supported for non-derived classes
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
        /// Translation of the ParticleSystem. Usually inherited directly from the ParticleSystemComponent or can be directly set.
        /// </summary>
        /// <userdoc>
        /// Translation of the ParticleSystem. Usually inherited directly from the ParticleSystemComponent or can be directly set.
        /// </userdoc>
        [DataMemberIgnore]
        public Vector3 Translation = new Vector3(0, 0, 0);

        /// <summary>
        /// Rotation of the ParticleSystem, expressed as a quaternion rotation. Usually inherited directly from the ParticleSystemComponent or can be directly set.
        /// </summary>
        /// <userdoc>
        /// Rotation of the ParticleSystem, expressed as a quaternion rotation. Usually inherited directly from the ParticleSystemComponent or can be directly set.
        /// </userdoc>
        [DataMemberIgnore]
        public Quaternion Rotation = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// Scale of the ParticleSystem. Only uniform scale is supported. Usually inherited directly from the ParticleSystemComponent or can be directly set.
        /// </summary>
        /// <userdoc>
        /// Scale of the ParticleSystem. Only uniform scale is supported. Usually inherited directly from the ParticleSystemComponent or can be directly set.
        /// </userdoc>
        [DataMemberIgnore]
        public float UniformScale = 1f;

        /// <summary>
        /// Updates the particles
        /// </summary>
        /// <param name="dt">Delta time - time, in seconds, elapsed since the last Update call to this particle system</param>
        /// <userdoc>
        /// Updates the particle system and all particles contained within. Delta time is the time, in seconds, which has passed since the last Update call.
        /// </userdoc>
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
        /// Render all particles in this particle system. Particles might have different materials assigned.
        /// </summary>
        /// <userdoc>
        /// Render all particles in this particle system. Particles might have different materials assigned.
        /// </userdoc>
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
