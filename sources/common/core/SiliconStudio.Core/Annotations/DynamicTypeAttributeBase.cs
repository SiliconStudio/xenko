// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
