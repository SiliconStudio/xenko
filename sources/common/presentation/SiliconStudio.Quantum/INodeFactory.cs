// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This interface represents a factory capable of creating <see cref="IGraphNode"/> instances.
    /// </summary>
    public interface INodeFactory
    {
        /// <summary>
        /// Creates an <see cref="IGraphNode"/> instance that represents a class object.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="guid">The unique identifier of the node to build.</param>
        /// <param name="obj">The object represented by the <see cref="IGraphNode"/> instance to create.</param>
        /// <param name="descriptor">The <see cref="ITypeDescriptor"/> of the object represented by the <see cref="IGraphNode"/> instance to create.</param>
        /// <param name="isPrimitive">Indicates if this object should be considered as a primitive type. This is only <c>true</c> if the object type has been registered as a primitive type in the <see cref="INodeBuilder"/> instance.</param>
        /// <returns>A new <see cref="IGraphNode"/> instance representing the given class object.</returns>
        [NotNull]
        IGraphNode CreateObjectContent([NotNull] INodeBuilder nodeBuilder, Guid guid, [NotNull] object obj, [NotNull] ITypeDescriptor descriptor, bool isPrimitive);

        /// <summary>
        /// Creates an <see cref="IGraphNode"/> instance that represents a boxed structure object.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="guid">The unique identifier of the node to build.</param>
        /// <param name="structure">The boxed structure object represented bu the <see cref="IGraphNode"/> instace to create.</param>
        /// <param name="descriptor">The <see cref="ITypeDescriptor"/> of the structure represented by the <see cref="IGraphNode"/> instance to create.</param>
        /// <param name="isPrimitive">Indicates if this object should be considered as a primitive type. This is only <c>true</c> if the object type has been registered as a primitive type in the <see cref="INodeBuilder"/> instance.</param>
        /// <returns>A new <see cref="IGraphNode"/> instance representing the given boxed structure object.</returns>
        [NotNull]
        IGraphNode CreateBoxedContent([NotNull] INodeBuilder nodeBuilder, Guid guid, [NotNull] object structure, [NotNull] ITypeDescriptor descriptor, bool isPrimitive);

        /// <summary>
        /// Creates an <see cref="IGraphNode"/> instance that represents a member property of a parent object.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="guid">The unique identifier of the node to build.</param>
        /// <param name="parent">The node representing the parent container.</param>
        /// <param name="member">The <see cref="IMemberDescriptor"/> of the member.</param>
        /// <param name="isPrimitive">Indicates if this object should be considered as a primitive type. This is <c>true</c> if the member type is a primitve .NET type, or if it is a type that has been registered as a primitive type in the <see cref="INodeBuilder"/> instance.</param>
        /// <param name="value">The value of this object.</param>
        /// <returns>A new <see cref="IGraphNode"/> instance representing the given member property.</returns>
        [NotNull]
        IGraphNode CreateMemberContent([NotNull] INodeBuilder nodeBuilder, Guid guid, [NotNull] IObjectNode parent, [NotNull] IMemberDescriptor member, bool isPrimitive, object value);
    }
}
