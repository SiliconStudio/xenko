// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// Base class for a dynamic type attribute.
    /// </summary>
    public abstract class DynamicTypeAttributeBase : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTypeAttributeBase"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        protected DynamicTypeAttributeBase([NotNull] Type type)
        {
            TypeName = type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTypeAttributeBase"/> class.
        /// </summary>
        /// <param name="typeName">The type.</param>
        protected DynamicTypeAttributeBase(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the name of the <see cref="DynamicTypeAttributeBase"/> type
        /// </summary>
        /// <value>The name of the serializable type.</value>
        public string TypeName { get; }
    }
}