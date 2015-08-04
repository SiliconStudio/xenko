// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Physics
{
    public class ColliderShape : IDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (InternalShape == null) return;
            InternalShape.Dispose();
            InternalShape = null;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public ColliderShapeTypes Type { get; protected set; }

        /// <summary>
        /// The local offset
        /// </summary>
        public Vector3 LocalOffset;

        /// <summary>
        /// The local rotation
        /// </summary>
        public Quaternion LocalRotation = Quaternion.Identity;

        /// <summary>
        /// Updates the local transformations, required if you change LocalOffset and/or LocalRotation.
        /// </summary>
        public void UpdateLocalTransformations()
        {
            var inverseRotation = LocalRotation;
            inverseRotation.Invert();

            PositiveCenterMatrix = Matrix.RotationQuaternion(LocalRotation) * Matrix.Translation(LocalOffset);
            NegativeCenterMatrix = Matrix.RotationQuaternion(inverseRotation) * Matrix.Translation(-LocalOffset);

            //if we are part of a compund we should update the transformation properly
            if (Parent == null) return;
            var childs = Parent.InternalCompoundShape.ChildList;
            for (var i = 0; i < childs.Count; i++)
            {
                if (childs[i].ChildShape == InternalShape)
                {
                    Parent.InternalCompoundShape.UpdateChildTransform(i, PositiveCenterMatrix, true);
                }
            }
        }

        /// <summary>
        /// Gets the positive center matrix.
        /// </summary>
        /// <value>
        /// The positive center matrix.
        /// </value>
        public Matrix PositiveCenterMatrix { get; private set; }

        /// <summary>
        /// Gets the negative center matrix.
        /// </summary>
        /// <value>
        /// The negative center matrix.
        /// </value>
        public Matrix NegativeCenterMatrix { get; private set; }

        /// <summary>
        /// Gets or sets the scaling.
        /// Make sure that you manually created and assigned an exclusive ColliderShape to the Collider otherwise since the engine shares shapes among many Colliders, all the colliders will be scaled.
        /// Please note that this scaling has no relation to the TransformComponent scaling.
        /// </summary>
        /// <value>
        /// The scaling.
        /// </value>
        public Vector3 Scaling
        {
            get
            {
                return InternalShape.LocalScaling;
            }
            set
            {
                var newScaling = value;
                
                if (Is2D) newScaling.Z = 1.0f;

                DebugPrimitiveMatrix *= Matrix.Scaling(newScaling);

                if (Is2D) newScaling.Z = 0.0f;

                InternalShape.LocalScaling = newScaling;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collider shape is 2D.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is2 d]; otherwise, <c>false</c>.
        /// </value>
        public bool Is2D { get; internal set; }

        public IColliderShapeDesc Description { get; internal set; }

        internal BulletSharp.CollisionShape InternalShape;

        internal CompoundColliderShape Parent;

        public virtual GeometricPrimitive CreateDebugPrimitive(GraphicsDevice device)
        {
            return null;
        }

        public Model DebugModel;

        public Matrix DebugPrimitiveMatrix;

        internal bool NeedsCustomCollisionCallback;
    }
}