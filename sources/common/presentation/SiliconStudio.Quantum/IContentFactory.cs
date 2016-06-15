// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This interface represents a factory capable of creating <see cref="IContent"/> instances for <see cref="IGraphNode"/> object. An <see cref="IContent"/>
    /// object is a wrapper that allows read/write access to the actual value of a node.
    /// </summary>
    public interface IContentFactory
    {
        /// <summary>
        /// Creates an <see cref="IContent"/> instance that represents a class object.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="obj">The object represented by the <see cref="IContent"/> instance to create.</param>
        /// <param name="descriptor">The <see cref="ITypeDescriptor"/> of the object represented by the <see cref="IContent"/> instance to create.</param>
        /// <param name="isPrimitive">Indicates if this object should be considered as a primitive type. This is only <c>true</c> if the object type has been registered as a primitive type in the <see cref="INodeBuilder"/> instance.</param>
        /// <returns>A new <see cref="IContent"/> instance representing the given class object.</returns>
        IContent CreateObjectContent(INodeBuilder nodeBuilder, object obj, ITypeDescriptor descriptor, bool isPrimitive);

        /// <summary>
        /// Creates an <see cref="IContent"/> instance that represents a boxed structure object.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="structure">The boxed structure object represented bu the <see cref="IContent"/> instace to create.</param>
        /// <param name="descriptor">The <see cref="ITypeDescriptor"/> of the structure represented by the <see cref="IContent"/> instance to create.</param>
        /// <param name="isPrimitive">Indicates if this object should be considered as a primitive type. This is only <c>true</c> if the object type has been registered as a primitive type in the <see cref="INodeBuilder"/> instance.</param>
        /// <returns>A new <see cref="IContent"/> instance representing the given boxed structure object.</returns>
        IContent CreateBoxedContent(INodeBuilder nodeBuilder, object structure, ITypeDescriptor descriptor, bool isPrimitive);

        /// <summary>
        /// Creates an <see cref="IContent"/> instance that represents a member property of a parent object.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="container">The <see cref="IContent"/> instance of the container (parent) object.</param>
        /// <param name="member">The <see cref="IMemberDescriptor"/> of the member.</param>
        /// <param name="isPrimitive">Indicates if this object should be considered as a primitive type. This is <c>true</c> if the member type is a primitve .NET type, or if it is a type that has been registered as a primitive type in the <see cref="INodeBuilder"/> instance.</param>
        /// <param name="value">The value of this object.</param>
        /// <returns>A new <see cref="IContent"/> instance representing the given member property.</returns>
        IContent CreateMemberContent(INodeBuilder nodeBuilder, ContentBase container, IMemberDescriptor member, bool isPrimitive, object value);
    }
}
