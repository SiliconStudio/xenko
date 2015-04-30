// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Engine
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
        public static T AddChild<T>(this T parentEntity, Entity childEntity) where T : Entity
        {
            if (childEntity == null) throw new ArgumentNullException("childEntity");
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
        public static T RemoveChild<T>(this T parentEntity, Entity childEntity) where T : Entity
        {
            if (childEntity == null) throw new ArgumentNullException("childEntity");
            parentEntity.Transform.Children.Remove(childEntity.Transform);
            return parentEntity;
        }
    }
}