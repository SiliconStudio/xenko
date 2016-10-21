// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
    /// </summary>
    public class TypeDescriptorFactory : TypeDescriptorFactoryBase
    {
        private readonly bool emitDefaultValues;
        private readonly IMemberNamingConvention namingConvention;

        /// <summary>
        /// The default type descriptor factory.
        /// </summary>
        public static readonly TypeDescriptorFactory Default = new TypeDescriptorFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactory"/> class.
        /// </summary>
        public TypeDescriptorFactory() : this(new AttributeRegistry(), false, new DefaultNamingConvention())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactory" /> class.
        /// </summary>
        /// <param name="attributeRegistry">The attribute registry.</param>
        /// <exception cref="System.ArgumentNullException">attributeRegistry</exception>
        public TypeDescriptorFactory(IAttributeRegistry attributeRegistry)
            : this(attributeRegistry, false, new DefaultNamingConvention())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactory" /> class.
        /// </summary>
        /// <param name="attributeRegistry">The attribute registry.</param>
        /// <exception cref="System.ArgumentNullException">attributeRegistry</exception>
        public TypeDescriptorFactory(IAttributeRegistry attributeRegistry, bool emitDefaultValues, IMemberNamingConvention namingConvention)
            : base(new DefaultKeyComparer(), attributeRegistry)
        {
            this.emitDefaultValues = emitDefaultValues;
            this.namingConvention = namingConvention;
        }

        /// <summary>
        /// Creates a type descriptor for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An instance of type descriptor.</returns>
        protected override ITypeDescriptor Create(Type type)
        {
            ITypeDescriptor descriptor;
            // The order of the descriptors here is important

            if (PrimitiveDescriptor.IsPrimitive(type))
            {
                descriptor = new PrimitiveDescriptor(this, type, emitDefaultValues, namingConvention);
            }
            else if (DictionaryDescriptor.IsDictionary(type)) // resolve dictionary before collections, as they are also collections
            {
                // IDictionary
                descriptor = new DictionaryDescriptor(this, type, emitDefaultValues, namingConvention);
            }
            else if (CollectionDescriptor.IsCollection(type))
            {
                // ICollection
                descriptor = new CollectionDescriptor(this, type, emitDefaultValues, namingConvention);
            }
            else if (type.IsArray)
            {
                // array[]
                descriptor = new ArrayDescriptor(this, type, emitDefaultValues, namingConvention);
            }
            else if (NullableDescriptor.IsNullable(type))
            {
                descriptor = new NullableDescriptor(this, type, emitDefaultValues, namingConvention);
            }
            else
            {
                // standard object (class or value type)
                descriptor = new ObjectDescriptor(this, type, emitDefaultValues, namingConvention);
            }

            return descriptor;
        }
    }
}
