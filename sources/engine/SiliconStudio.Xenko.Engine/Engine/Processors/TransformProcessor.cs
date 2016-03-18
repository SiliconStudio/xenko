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
    /// Handle <see cref="TransformComponent.Children"/> and updates <see cref="TransformComponent.WorldMatrix"/> of entities.
    /// </summary>
    public class TransformProcessor : EntityProcessor<TransformComponent>
    {
        /// <summary>
        /// List of root entities <see cref="TransformComponent"/> of every <see cref="Entity"/> in <see cref="EntityManager"/>.
        /// </summary>
        internal readonly HashSet<TransformComponent> TransformationRoots = new HashSet<TransformComponent>();

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
            Order = -200;            
        }

        /// <inheritdoc/>
        protected override TransformComponent GenerateComponentData(Entity entity, TransformComponent component)
        {
            return component;
        }

        /// <inheritdoc/>
        protected internal override void OnSystemAdd()
        {
        }

        /// <inheritdoc/>
        protected internal override void OnSystemRemove()
        {
            TransformationRoots.Clear();
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentAdding(Entity entity, TransformComponent component, TransformComponent data)
        {
            if (component.Parent == null)
            {
                TransformationRoots.Add(component);
            }

            foreach (var child in data.Children)
            {
                InternalAddEntity(child.Entity);
            }

            ((TrackingCollection<TransformComponent>)data.Children).CollectionChanged += Children_CollectionChanged;
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentRemoved(Entity entity, TransformComponent component, TransformComponent data)
        {
            var entityToRemove = new List<Entity>();
            foreach (var child in data.Children)
            {
                entityToRemove.Add(child.Entity);
            }

            foreach (var childEntity in entityToRemove)
            {
                InternalRemoveEntity(childEntity, false);
            }

            if (component.Parent == null)
            {
                TransformationRoots.Remove(component);
            }

            ((TrackingCollection<TransformComponent>)data.Children).CollectionChanged -= Children_CollectionChanged;
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
            foreach (var t in TransformationRoots)
                notSpecialRootComponents.Add(t);

            // Special roots are already filtered out
            UpdateTransformations(notSpecialRootComponents);
        }
        
        private void Children_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            // Added/removed children of entities in the entity manager have to be added/removed of the entity manager.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InternalAddEntity(((TransformComponent)e.Item).Entity);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    InternalRemoveEntity(((TransformComponent)e.Item).Entity, false);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}