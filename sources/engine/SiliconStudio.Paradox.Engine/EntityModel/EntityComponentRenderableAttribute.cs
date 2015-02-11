// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// An attribute used to associate a default <see cref="IEntityComponentRenderer"/> to an entity component.
    /// </summary>
    [DataContract] // TODO: Remove the need for using a DataContract. This is caused by CameraRendererMode.RendererTypesKey
    public class EntityComponentRenderableAttribute : Attribute
    {
        public static readonly IComparer<EntityComponentRenderableAttribute> DefaultComparer = new EntityComponentRenderableAttributeComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRenderableAttribute"/> class.
        /// </summary>
        public EntityComponentRenderableAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRenderableAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="IEntityComponentRenderer"/>.</param>
        public EntityComponentRenderableAttribute(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRenderableAttribute" /> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="IEntityComponentRenderer" />.</param>
        /// <param name="order">The order.</param>
        public EntityComponentRenderableAttribute(Type type, int order)
        {
            this.Type = type;
            this.Order = order;
        }

        /// <summary>
        /// Gets the renderer type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        private class EntityComponentRenderableAttributeComparer : IComparer<EntityComponentRenderableAttribute>
        {
            public int Compare(EntityComponentRenderableAttribute x, EntityComponentRenderableAttribute y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }
    }
}