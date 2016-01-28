// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// Defines the default <see cref="IEntityComponentRenderer"/> attached to an <see cref="EntityComponent"/>. 
    /// </summary>
    [DataContract]
    public struct EntityComponentRendererType : IComparable<EntityComponentRendererType>, IEquatable<EntityComponentRendererType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRendererType" /> struct.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="rendererType">The type.</param>
        /// <param name="displayOrder">The display order.</param>
        public EntityComponentRendererType(TypeInfo componentType, Type rendererType, int displayOrder)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");
            if (rendererType == null) throw new ArgumentNullException("rendererType");

            if (!typeof(EntityComponent).GetTypeInfo().IsAssignableFrom(componentType.GetTypeInfo()))
            {
                throw new ArgumentException("Must inherit from EntityComponent", "componentType");
            }

            if (!typeof(IEntityComponentRenderer).GetTypeInfo().IsAssignableFrom(rendererType.GetTypeInfo()))
            {
                throw new ArgumentException("Must inherit from IEntityComponentRenderer", "rendererType");
            }

            ComponentType = componentType;
            RendererType = rendererType;
            DisplayOrder = displayOrder;
        }

        /// <summary>
        /// The type of component. Must be derived from <see cref="EntityComponent"/>/
        /// </summary>
        public readonly TypeInfo ComponentType;


        /// <summary>
        /// The type of the renderer. Must derived from <see cref="IEntityComponentRenderer"/> and have a public parameter-less
        /// constructor.
        /// </summary>
        public readonly Type RendererType;

        /// <summary>
        /// The display order of this renderer. Zero is the default for the <see cref="ModelComponent"/> renderer.
        /// </summary>
        public readonly int DisplayOrder;

        public bool Equals(EntityComponentRendererType other)
        {
            return ComponentType == other.ComponentType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EntityComponentRendererType && Equals((EntityComponentRendererType)obj);
        }

        public override int GetHashCode()
        {
            return (ComponentType != null ? ComponentType.GetHashCode() : 0);
        }

        public int CompareTo(EntityComponentRendererType other)
        {
            return DisplayOrder.CompareTo(other.DisplayOrder);
        }

        public override string ToString()
        {
            return string.Format("Component: {0}, DisplayOrder: {1} (Renderer: {2})", ComponentType, DisplayOrder, RendererType);
        }
    }
}