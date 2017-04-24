// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    public class CompoundColliderShape : ColliderShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundColliderShape"/> class.
        /// </summary>
        public CompoundColliderShape()
        {
            Type = ColliderShapeTypes.Compound;
            Is2D = false;

            CachedScaling = Vector3.One;
            InternalShape = InternalCompoundShape = new BulletSharp.CompoundShape
            {
                LocalScaling = CachedScaling
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            foreach (var shape in colliderShapes)
            {
                InternalCompoundShape.RemoveChildShape(shape.InternalShape);

                if (!shape.IsPartOfAsset)
                {
                    shape.Dispose();
                }
                else
                {
                    shape.Parent = null;
                }
            }
            colliderShapes.Clear();

            base.Dispose();
        }

        private readonly List<ColliderShape> colliderShapes = new List<ColliderShape>();

        private BulletSharp.CompoundShape internalCompoundShape;

        internal BulletSharp.CompoundShape InternalCompoundShape
        {
            get
            {
                return internalCompoundShape;
            }
            set
            {
                InternalShape = internalCompoundShape = value;
            }
        }

        /// <summary>
        /// Adds a child shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public void AddChildShape(ColliderShape shape)
        {
            colliderShapes.Add(shape);

            var compoundMatrix = Matrix.RotationQuaternion(shape.LocalRotation) * Matrix.Translation(shape.LocalOffset);

            InternalCompoundShape.AddChildShape(compoundMatrix, shape.InternalShape);

            shape.Parent = this;
        }

        /// <summary>
        /// Removes a child shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public void RemoveChildShape(ColliderShape shape)
        {
            colliderShapes.Remove(shape);

            InternalCompoundShape.RemoveChildShape(shape.InternalShape);

            shape.Parent = null;
        }

        /// <summary>
        /// Gets the <see cref="ColliderShape"/> with the specified i.
        /// </summary>
        /// <value>
        /// The <see cref="ColliderShape"/>.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public ColliderShape this[int i] => colliderShapes[i];

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => colliderShapes.Count;

        public override Vector3 Scaling
        {
            get
            {
                return CachedScaling;
            }
            set
            {
                base.Scaling = value;

                foreach (var colliderShape in colliderShapes)
                {
                    colliderShape.Scaling = CachedScaling;
                }
            }
        }
    }
}
