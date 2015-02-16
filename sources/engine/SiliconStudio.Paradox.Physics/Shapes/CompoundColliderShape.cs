// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Physics
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

            InternalShape = InternalCompoundShape = new BulletSharp.CompoundShape();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            foreach (var shape in colliderShapes)
            {
                InternalCompoundShape.RemoveChildShape(shape.InternalShape);
                shape.Dispose();
            }
            colliderShapes.Clear();

            base.Dispose();
        }

        private readonly FastList<ColliderShape> colliderShapes = new FastList<ColliderShape>();

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

            InternalCompoundShape.AddChildShape(shape.PositiveCenterMatrix, shape.InternalShape);

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
        public ColliderShape this[int i]
        {
            get { return colliderShapes[i]; }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                return colliderShapes.Count;
            }
        }
    }
}