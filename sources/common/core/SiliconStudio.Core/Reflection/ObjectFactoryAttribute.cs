// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Attribute to define for a <see cref="IObjectFactory"/> for a <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ObjectFactoryAttribute : Attribute
    {
        private readonly string typeName;

        /// <summary>
        /// Gets the name of the <see cref="ObjectFactoryAttribute"/> type
        /// </summary>
        /// <value>The name of the serializable type.</value>
        public string FactoryTypeName
        {
            get { return this.typeName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFactoryAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must be of type <see cref="IObjectFactory"/>.</param>
        public ObjectFactoryAttribute(Type type)
        {
            this.typeName = type != null ? type.AssemblyQualifiedName : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFactoryAttribute"/> class.
        /// </summary>
        /// <param name="typeName">The type must be of type <see cref="IObjectFactory"/>.</param>
        public ObjectFactoryAttribute(string typeName)
        {
            this.typeName = typeName != null ? typeName.ToUpperInvariant() : null;
        }
    }
}