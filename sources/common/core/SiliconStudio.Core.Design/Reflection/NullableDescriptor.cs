// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Describes a descriptor for a nullable type <see cref="Nullable{T}"/>.
    /// </summary>
    public class NullableDescriptor : ObjectDescriptor
    {
        private static readonly List<IMemberDescriptor> EmptyMembers = new List<IMemberDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentException">Type [{0}] is not a primitive</exception>
        public NullableDescriptor(ITypeDescriptorFactory factory, Type type)
            : base(factory, type)
        {
            if (!IsNullable(type))
                throw new ArgumentException("Type [{0}] is not a primitive");

            Category = DescriptorCategory.Nullable;
            UnderlyingType = Nullable.GetUnderlyingType(type);
        }

        /// <summary>
        /// Gets the type underlying type T of the nullable <see cref="Nullable{T}"/>
        /// </summary>
        /// <value>The type of the element.</value>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// Determines whether the specified type is nullable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is nullable; otherwise, <c>false</c>.</returns>
        public static bool IsNullable(Type type)
        {
            return type.IsNullable();
        }

        protected override System.Collections.Generic.List<IMemberDescriptor> PrepareMembers()
        {
            return EmptyMembers;
        }
    }
}