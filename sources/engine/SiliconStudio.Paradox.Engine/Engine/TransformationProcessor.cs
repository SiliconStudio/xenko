// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Threading;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Updates <see cref="TransformationComponent.WorldMatrix"/> of entities.
    /// </summary>
    public class TransformationProcessor : EntityProcessor<TransformationProcessor.AssociatedData>
    {
        /// <summary>
        /// List of <see cref="TransformationComponent"/> of every <see cref="Entity"/> in <see cref="EntitySystem.RootEntities"/>.
        /// </summary>
        private readonly TrackingHashSet<TransformationComponent> transformationRoots = new TrackingHashSet<TransformationComponent>();

        /// <summary>
        /// The list of the components that are not special roots.
        /// </summary>
        /// <remarks>This field is instantiated here to avoid reallocation at each frames</remarks>
        private readonly FastCollection<TransformationComponent> notSpecialRootComponents = new FastCollection<TransformationComponent>(); 

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationProcessor" /> class.
        /// </summary>
        public TransformationProcessor()
            : base(new PropertyKey[] { TransformationComponent.Key })
        {
        }

        /// <inheritdoc/>
        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData { TransformationComponent = entity.Transformation };
        }

        /// <inheritdoc/>
        protected internal override void OnSystemAdd()
        {
            var rootEntities = EntitySystem.GetProcessor<HierarchicalProcessor>().RootEntities;
            ((ITrackingCollectionChanged)rootEntities).CollectionChanged += rootEntities_CollectionChanged;

            // Add transformation of existing root entities
            foreach (var entity in rootEntities)
            {
                transformationRoots.Add(entity.Transformation);
            }
        }

        /// <inheritdoc/>
        protected internal override void OnSystemRemove()
        {
            transformationRoots.Clear();
        }

        internal static void UpdateTransformations(FastCollection<TransformationComponent> transformationComponents, bool skipSpecialRoots)
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
                            if (skipSpecialRoots && transformation.isSpecialRoot)
                                return;

                            UpdateTransformation(transformation);

                            // Recurse
                            if (transformation.Children.Count > 0)
                                UpdateTransformations(transformation.Children, true);
                        }
                    );
            }
            else
            {
                foreach (var transformation in transformationComponents)
                {
                    if (skipSpecialRoots && transformation.isSpecialRoot)
                        continue;

                    UpdateTransformation(transformation);

                    // Recurse
                    if (transformation.Children.Count > 0)
                        UpdateTransformations(transformation.Children, true);
                }
            }
        }

        private static void UpdateTransformation(TransformationComponent transformation)
        {
            // Update transformation
            transformation.UpdateLocalMatrix();
            transformation.UpdateWorldMatrixNonRecursive();
        }

        /// <summary>
        /// Updates all the <see cref="TransformationComponent.WorldMatrix"/>.
        /// </summary>
        /// <param name="time"></param>
        public override void Draw(GameTime time)
        {
            notSpecialRootComponents.Clear();
            foreach (var t in transformationRoots)
                if(!t.isSpecialRoot)
                    notSpecialRootComponents.Add(t);

            // Special roots are already filtered out
            UpdateTransformations(notSpecialRootComponents, false);
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
            
            // Translation
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;

            result.M14 = 0.0f;
            result.M24 = 0.0f;
            result.M34 = 0.0f;
            result.M44 = 1.0f;
        }

        public struct AssociatedData
        {
            public TransformationComponent TransformationComponent;
        }

        private void rootEntities_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    transformationRoots.Add(((Entity)e.Item).Transformation);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    transformationRoots.Remove(((Entity)e.Item).Transformation);
                    break;
            }
        }
    }
}