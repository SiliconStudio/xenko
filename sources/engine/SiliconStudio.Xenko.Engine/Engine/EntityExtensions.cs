// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Extensions for <see cref="Entity"/>
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Deep clone of this entity.
        /// </summary>
        /// <param name="entity">The entity to clone</param>
        /// <returns>The cloned entity</returns>
        public static Entity Clone(this Entity entity)
        {
            return EntityCloner.Clone(entity);
        }

        /// <summary>
        /// Gets the children of this entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>An iteration on the children entity</returns>
        public static IEnumerable<Entity> GetChildren(this Entity entity)
        {
            var transformationComponent = entity.Transform;
            if (transformationComponent != null)
            {
                foreach (var child in transformationComponent.Children)
                {
                    yield return child.Entity;
                }
            }
        }

        /// <summary>
        /// Enables or disables components of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <param name="entity">The entity to apply this method.</param>
        /// <param name="enabled">If set to <c>true</c>, all components of {T} will be enabled; otherwise they will be disabled</param>
        /// <param name="applyOnChildren">Recursively apply this method to the children of this entity</param>
        public static void Enable<T>(this Entity entity, bool enabled = true, bool applyOnChildren = false) where T : ActivableEntityComponent
        {
            // NOTE: This method is recursive. That might not be the best solution in case of deep entities.
            for (var i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i] as T;
                if (component != null)
                {
                    component.Enabled = enabled;
                }
            }

            if (!applyOnChildren) return;

            var transformationComponent = entity.Transform;

            if (transformationComponent == null) return;

            var children = transformationComponent.Children;
            for (var i = 0; i < children.Count; i++)
            {
                Enable<T>(children[i].Entity, enabled, true);
            }
        }

        /// <summary>
        /// Enables or disables all <see cref="ActivableEntityComponent"/>.
        /// </summary>
        /// <param name="entity">The entity to apply this method.</param>
        /// <param name="enabled">If set to <c>true</c>, all <see cref="ActivableEntityComponent"/> will be enabled; otherwise they will be disabled</param>
        /// <param name="applyOnChildren">Recursively apply this method to the children of this entity</param>
        public static void EnableAll(this Entity entity, bool enabled = true, bool applyOnChildren = false)
        {
            Enable<ActivableEntityComponent>(entity, enabled, applyOnChildren);
        }

        /// <summary>
        /// Performs a breadth first search of the entity and it's children for a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="includeDisabled">Should search include <see cref="ActivableEntityComponent"/> where <see cref="ActivableEntityComponent.Enabled"/> is <c>false</c>.</param>
        /// <returns>The component or null if does no exist.</returns>
        /// <exception cref="ArgumentNullException">The entity was <c>null</c>.</exception>
        public static T GetComponentInChildren<T>(this Entity entity, bool includeDisabled = false) where T : EntityComponent
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            //breadth first
            var queue = new Queue<Entity>();
            queue.Enqueue(entity);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();


                var component = current.Get<T>();

                var isEnabled = ((component as ActivableEntityComponent)?.Enabled).GetValueOrDefault(true);
                if (component != null && (isEnabled || includeDisabled))
                {
                    return component;
                }

                var children = current.Transform.Children;

                for (int i = 0; i < children.Count; i++)
                {
                    queue.Enqueue(children[i].Entity);
                }
            }

            return null;
        }

        /// <summary>
        /// Performs a depth first search of the entity and it's children for all components of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="includeDisabled">Should search include <see cref="ActivableEntityComponent"/> where <see cref="ActivableEntityComponent.Enabled"/> is <c>false</c>.</param>
        /// <returns>An iteration on the components.</returns>
        /// <exception cref="ArgumentNullException">The entity was <c>null</c>.</exception>
        public static IEnumerable<T> GetComponentsInChildren<T>(this Entity entity, bool includeDisabled = false) where T : EntityComponent
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            //depth first
            var stack = new Stack<Entity>();
            stack.Push(entity);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var component in current.GetAll<T>())
                {
                    var isEnabled = ((component as ActivableEntityComponent)?.Enabled).GetValueOrDefault(true);
                    if (component != null && (isEnabled || includeDisabled))
                    {
                        yield return component;
                    }
                }

                var children = current.Transform.Children;

                for (int i = 0; i < children.Count; i++)
                {
                    stack.Push(children[i].Entity);
                }
            }
        }

        /// <summary>
        /// Performs a search of the entity and it's ancestors for a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="includeDisabled">Should search include <see cref="ActivableEntityComponent"/> where <see cref="ActivableEntityComponent.Enabled"/> is <c>false</c>.</param>
        /// <returns>The component or <c>null</c> if does no exist.</returns>
        /// <exception cref="ArgumentNullException">The entity was <c>null</c>.</exception>
        public static T GetComponentInParent<T>(this Entity entity, bool includeDisabled = false) where T : EntityComponent
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var current = entity;

            do
            {
                var component = current.Get<T>();

                var isEnabled = ((component as ActivableEntityComponent)?.Enabled).GetValueOrDefault(true);
                if (component != null && (isEnabled || includeDisabled))
                {
                    return component;
                }

            } while ((current = current.GetParent()) != null);

            return null;
        }

        /// <summary>
        /// Performs a search of the entity and it's ancestors for all components of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="includeDisabled">Should search include <see cref="ActivableEntityComponent"/> where <see cref="ActivableEntityComponent.Enabled"/> is <c>false</c>.</param>
        /// <returns>An iteration on the components.</returns>
        /// <exception cref="ArgumentNullException">The entity was <c>null</c>.</exception>
        public static IEnumerable<T> GetComponentsInParent<T>(this Entity entity, bool includeDisabled = false) where T : EntityComponent
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var current = entity;

            do
            {
                foreach (var component in current.GetAll<T>())
                {
                    var isEnabled = ((component as ActivableEntityComponent)?.Enabled).GetValueOrDefault(true);
                    if (component != null && (isEnabled || includeDisabled))
                    {
                        yield return component;
                    }
                }


            } while ((current = current.GetParent()) != null);
        }
    }
}