// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// Defines the default <see cref="IEntityComponentRenderer"/> attached to an <see cref="EntityComponent"/>. 
    /// </summary>
    [DataContract]
    public struct EntityComponentRendererType
    {
        public static readonly IComparer<EntityComponentRendererType> DefaultComparer = new EntityComponentRendererTypeComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRendererType"/> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="displayOrder">The display order.</param>
        public EntityComponentRendererType(Type type, int displayOrder)
        {
            Type = type;
            DisplayOrder = displayOrder;
        }

        /// <summary>
        /// The type of the renderer. Must derived from <see cref="IEntityComponentRenderer"/> and have a public parameter-less
        /// constructor.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// The display order of this renderer. Zero is the default for the <see cref="ModelComponent"/> renderer.
        /// </summary>
        public readonly int DisplayOrder;

        private class EntityComponentRendererTypeComparer : IComparer<EntityComponentRendererType>
        {
            public int Compare(EntityComponentRendererType x, EntityComponentRendererType y)
            {
                return x.DisplayOrder.CompareTo(y.DisplayOrder);
            }
        }
    }
}