// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// Updates <see cref="TransformComponent.WorldMatrix"/> of entities.
    /// </summary>
    public class TransformProcessor : EntityProcessor<TransformComponent, TransformComponent>
    {
        /// <summary>
        /// List of <see cref="TransformComponent"/> of every <see cref="Entity"/> in <see cref="EntityManager.RootEntities"/>.
        /// </summary>
        private readonly TrackingHashSet<TransformComponent> transformationRoots = new TrackingHashSet<TransformComponent>();

        /// <summary>
        /// The list of the components that are not special roots.
        /// </summary>
        /// <remarks>This field is instantiated here to avoid reallocation at each frames</remarks>
        private readonly FastCollection<TransformComponent> notSpecialRootComponents = new FastCollection<TransformComponent>(); 

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformProcessor" /> class.
        /// </summary>
        public TransformProcessor()
        {
            Order = -100;            
        }

        /// <inheritdoc/>
        protected override TransformComponent GenerateAssociatedData(Entity entity, TransformComponent component)
        {
            return component;
        }

        /// <inheritdoc/>
        protected internal override void OnSystemAdd()
        {
            var rootEntities = EntityManager.GetProcessor<HierarchicalProcessor>().RootEntities;
            ((ITrackingCollectionChanged)rootEntities).CollectionChanged += rootEntities_CollectionChanged;

            // Add transform of existing root entities
            foreach (var entity in rootEntities)
            {
                transformationRoots.Add(entity.Transform);
            }
        }

        /// <inheritdoc/>
        protected internal override void OnSystemRemove()
        {
            transformationRoots.Clear();
        }

        internal static void UpdateTransformations(FastCollection<TransformComponent> transformationComponents)
        {
            // To avoid GC pressure (due to lambda), parallelize only if required
            if (transformationComponents.Count >= 1024)
            {
                TaskList.Dispatch(
                    transformationComponents,
                    8,
                    1024,
                    (i, transformation) =>
                        {
                            UpdateTransformation(transformation);

                            // Recurse
                            if (transformation.Children.Count > 0)
                                UpdateTransformations(transformation.Children);
                        }
                    );
            }
            else
            {
                foreach (var transformation in transformationComponents)
                {
                    UpdateTransformation(transformation);

                    // Recurse
                    if (transformation.Children.Count > 0)
                        UpdateTransformations(transformation.Children);
                }
            }
        }

        private static void UpdateTransformation(TransformComponent transform)
        {
            // Update transform
            transform.UpdateLocalMatrix();
            transform.UpdateWorldMatrixInternal(false);
        }

        /// <summary>
        /// Updates all the <see cref="TransformComponent.WorldMatrix"/>.
        /// </summary>
        /// <param name="context"></param>
        public override void Draw(RenderContext context)
        {
            notSpecialRootComponents.Clear();
            foreach (var t in transformationRoots)
                notSpecialRootComponents.Add(t);

            // Special roots are already filtered out
            UpdateTransformations(notSpecialRootComponents);
        }

        /// <summary>
        /// Creates a matrix that contains the X, Y and Z rotation.
        /// </summary>
        /// <param name="rotation">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void CreateMatrixR(ref Vector3 rotation, out Matrix result)
        {
            // Equivalent to:
            //result =
            //    *Matrix.RotationX(rotation.X)
            //    *Matrix.RotationY(rotation.Y)
            //    *Matrix.RotationZ(rotation.Z)

            // Precompute cos and sin
            var cosX = (float)Math.Cos(rotation.X);
            var sinX = (float)Math.Sin(rotation.X);
            var cosY = (float)Math.Cos(rotation.Y);
            var sinY = (float)Math.Sin(rotation.Y);
            var cosZ = (float)Math.Cos(rotation.Z);
            var sinZ = (float)Math.Sin(rotation.Z);

            // Precompute some multiplications
            var sinZY = sinZ * sinY;
            var cosXZ = cosZ * cosX;
            var cosZsinY = cosZ * sinX;

            // Rotation
            result.M11 = cosZ * cosY;
            result.M21 = cosZsinY * sinY - cosX * sinZ;
            result.M31 = sinZ * sinX + cosXZ * sinY;
            result.M12 = cosY * sinZ;
            result.M22 = cosXZ + sinZY * sinX;
            result.M32 = cosX * sinZY - cosZsinY;
            result.M13 = -sinY;
            result.M23 = cosY * sinX;
            result.M33 = cosY * cosX;
            
            // Position
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;

            result.M14 = 0.0f;
            result.M24 = 0.0f;
            result.M34 = 0.0f;
            result.M44 = 1.0f;
        }

        private void rootEntities_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    transformationRoots.Add(((Entity)e.Item).Transform);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    transformationRoots.Remove(((Entity)e.Item).Transform);
                    break;
            }
        }
    }
}