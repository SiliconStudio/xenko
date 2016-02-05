// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

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
        /// <typeparam name="T">Type of the parent Entity receiving the child.</typeparam>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="childEntity">The child parent Entity.</param>
        /// <returns>The this instance.</returns>
        /// <exception cref="System.ArgumentNullException">childEntity</exception>
        public static Entity AddChild(this Entity parentEntity, Entity childEntity)
        {
            if (childEntity == null) throw new ArgumentNullException(nameof(childEntity));
            parentEntity.Transform.Children.Add(childEntity.Transform);
            return parentEntity;
        }

        /// <summary>
        /// Removes a child Entity to the transform component of a parent Entity. Note that the child entity is still in the <see cref="SceneInstance"/>.
        /// In order to remove it from the scene instance, you should call <see cref="SceneInstance.Remove"/>
        /// </summary>
        /// <typeparam name="T">Type of the parent Entity to remove the child from.</typeparam>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="childEntity">The child Entity.</param>
        /// <returns>The this instance.</returns>
        /// <exception cref="System.ArgumentNullException">childEntity</exception>
        public static Entity RemoveChild(this Entity parentEntity, Entity childEntity)
        {
            if (childEntity == null) throw new ArgumentNullException(nameof(childEntity));
            parentEntity.Transform.Children.Remove(childEntity.Transform);
            return parentEntity;
        }

        /// <summary>
        /// Get the nth child of an Entity as stored in its <see cref="TransformComponent"/>.
        /// </summary>
        /// <param name="parentEntity">The parent Entity.</param>
        /// <param name="index">The child index.</param>
        /// <returns></returns>
        public static Entity GetChild(this Entity parentEntity, int index)
        {
            if (parentEntity == null) throw new ArgumentNullException(nameof(parentEntity));
            return parentEntity.Transform.Children[index].Entity;
        }

        /// <summary>
        /// Returns the parent of this <see cref="Entity"/> as stored in its <see cref="TransformComponent"/>, or null if it has no parent.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The parent entity, or null if it has no parent.</returns>
        public static Entity GetParent(this Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return entity.Transform.Parent?.Entity;
        }
    }
}