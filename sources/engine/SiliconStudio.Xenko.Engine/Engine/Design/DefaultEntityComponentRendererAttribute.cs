// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// An attribute used to associate a default <see cref="IEntityComponentRenderProcessor"/> to an entity component.
    /// </summary>
    public class DefaultEntityComponentRendererAttribute : DynamicTypeAttributeBase
    {
        private readonly int order;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEntityComponentRendererAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="IEntityComponentRenderProcessor"/>.</param>
        public DefaultEntityComponentRendererAttribute(Type type) : base(type)
        {
            order = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEntityComponentRendererAttribute" /> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="IEntityComponentRenderProcessor" />.</param>
        /// <param name="order">The order.</param>
        public DefaultEntityComponentRendererAttribute(Type type, int order) : base(type)
        {
            this.order = order;
        }

        public int Order
        {
            get
            {
                return order;
            }
        }
    }
} 
