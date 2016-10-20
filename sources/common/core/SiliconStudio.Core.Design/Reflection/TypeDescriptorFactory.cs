// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
    /// </summary>
    public class TypeDescriptorFactory : TypeDescriptorFactoryBase
    {
        /// <summary>
        /// The default type descriptor factory.
        /// </summary>
        public static readonly TypeDescriptorFactory Default = new TypeDescriptorFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactory"/> class.
        /// </summary>
        public TypeDescriptorFactory() : this(new AttributeRegistry())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactory" /> class.
        /// </summary>
        /// <param name="attributeRegistry">The attribute registry.</param>
        /// <exception cref="System.ArgumentNullException">attributeRegistry</exception>
        public TypeDescriptorFactory(IAttributeRegistry attributeRegistry)
            : base(new KeyComparer(), attributeRegistry)
        {
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
                descriptor = new PrimitiveDescriptor(this, type);
            }
            else if (DictionaryDescriptor.IsDictionary(type)) // resolve dictionary before collections, as they are also collections
            {
                // IDictionary
                descriptor = new DictionaryDescriptor(this, type);
            }
            else if (CollectionDescriptor.IsCollection(type))
            {
                // ICollection
                descriptor = new CollectionDescriptor(this, type);
            }
            else if (type.IsArray)
            {
                // array[]
                descriptor = new ArrayDescriptor(this, type);
            }
            else if (NullableDescriptor.IsNullable(type))
            {
                descriptor = new NullableDescriptor(this, type);
            }
            else
            {
                // standard object (class or value type)
                descriptor = new ObjectDescriptor(this, type);
            }

            return descriptor;
        }

        // TODO: Replace this by DefaultKeyComparer once this class has been moved to Core.Reflection
        private class KeyComparer : IComparer<object>
        {
            public int Compare(object x, object y)
            {
                IMemberDescriptor left = (IMemberDescriptor)x;
                IMemberDescriptor right = (IMemberDescriptor)y;
                // If order is defined, first order by order
                if (left.Order.HasValue || right.Order.HasValue)
                {
                    var leftOrder = left.Order ?? int.MaxValue;
                    var rightOrder = right.Order ?? int.MaxValue;
                    return leftOrder.CompareTo(rightOrder);
                }

                // else order by name
                return string.CompareOrdinal(left.Name, right.Name);
            }
        }
    }
}
