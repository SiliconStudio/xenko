// Copyright (c) 2014-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Extensions for <see cref="Entity"/> and the <see cref="TransformComponent"/>.
    /// </summary>
    public static class EntityTransformExtensions
    {
        /// <summary>
        /// Adds a child Entity to the transform component of a parent Entity.
        /// </summary>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="childEntity">The child parent Entity.</param>
        /// <returns>The this instance.</returns>
        /// <exception cref="NullReferenceException"><paramref name="childEntity"/> is <c>null</c></exception>
        /// <exception cref="NullReferenceException"><paramref name="parentEntity"/> is <c>null</c></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddChild([NotNull] this Entity parentEntity, [NotNull] Entity childEntity)
        {
            parentEntity.Transform.Children.Add(childEntity.Transform);
        }

        /// <summary>
        /// Removes a child Entity to the transform component of a parent Entity. Note that the child entity is still in the <see cref="SceneInstance"/>.
        /// In order to remove it from the scene instance, you should call <see cref="SceneInstance.Remove"/>
        /// </summary>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="childEntity">The child Entity.</param>
        /// <exception cref="NullReferenceException"><paramref name="childEntity"/> is <c>null</c></exception>
        /// <exception cref="NullReferenceException"><paramref name="parentEntity"/> is <c>null</c></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveChild([NotNull] this Entity parentEntity, [NotNull] Entity childEntity)
        {
            parentEntity.Transform.Children.Remove(childEntity.Transform);
        }

        /// <summary>
        /// Removes a child entity from the transform component of a parent Entity.
        /// </summary>
        /// <param name="parentEntity">The parent entity.</param>
        /// <param name="childId">The child id of the child entity.</param>
        /// <returns>The this instance.</returns>
        /// <exception cref="NullReferenceException"><paramref name="parentEntity"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="childId"/> is <see cref="Guid.Empty"/></exception>
        public static void RemoveChild([NotNull] this Entity parentEntity, Guid childId)
        {
            if (childId == Guid.Empty) throw new ArgumentException(nameof(childId));
            for (var i = 0; i < parentEntity.Transform.Children.Count; i++)
            {
                var child = parentEntity.Transform.Children[i];
                if (child.Entity.Id == childId)
                {
                    parentEntity.Transform.Children.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Get the nth child of an Entity as stored in its <see cref="TransformComponent"/>.
        /// </summary>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="index">The child index.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"><paramref name="parentEntity"/> is <c>null</c></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetChild([NotNull] this Entity parentEntity, int index)
        {
            return parentEntity.Transform.Children[index].Entity;
        }

        /// <summary>
        /// Returns the parent of this <see cref="Entity"/> as stored in its <see cref="TransformComponent"/>, or null if it has no parent.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The parent entity, or null if it has no parent.</returns>
        /// <exception cref="NullReferenceException"><paramref name="entity"/> is <c>null</c></exception>
        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetParent([NotNull] this Entity entity)
        {
            return entity.Transform.Parent?.Entity;
        }

        /// <summary>
        /// Returns the first child in the hierarchy with the provided name.
        /// This function can be slow, do not use every frame!
        /// </summary>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="childName">The name of the child to look for.</param>
        /// <returns>Null or the first child with the requested name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parentEntity"/> is <c>null</c></exception>
        [CanBeNull]
        public static Entity FindChild([NotNull] this Entity parentEntity, string childName)
        {
            if (parentEntity == null) throw new ArgumentNullException(nameof(parentEntity));
            return Utilities.IterateTree(parentEntity, entity => entity?.GetChildren()).FirstOrDefault(entity => entity != null && entity.Name == childName);
        }

        /// <summary>
        /// Returns the root of this <see cref="Entity"/>, or itself if it has no parent.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The root entity, or itself if it has no parent.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <c>null</c></exception>
        [NotNull]
        public static Entity FindRoot([NotNull] this Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var root = entity;
            Entity parent;
            while ((parent = root.GetParent()) != null)
            {
                root = parent;
            }
            return root;
        }
    }
}
