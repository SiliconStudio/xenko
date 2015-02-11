// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// An attribute used to associate a default <see cref="IEntityComponentRenderer"/> to an entity component.
    /// </summary>
    public class EntityComponentRendererAttribute : Attribute
    {
        private readonly EntityComponentRendererType value;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRendererAttribute"/> class.
        /// </summary>
        public EntityComponentRendererAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRendererAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="IEntityComponentRenderer"/>.</param>
        public EntityComponentRendererAttribute(Type type)
        {
            value = new EntityComponentRendererType(type, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRendererAttribute" /> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="IEntityComponentRenderer" />.</param>
        /// <param name="order">The order.</param>
        public EntityComponentRendererAttribute(Type type, int order)
        {
            value = new EntityComponentRendererType(type, order);
        }

        /// <summary>
        /// Gets the renderer type.
        /// </summary>
        /// <value>The type.</value>
        public EntityComponentRendererType Value
        {
            get
            {
                return value;
            }
        }
    }
} 