using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
    /// </summary>
    public abstract class TypeDescriptorFactoryBase : ITypeDescriptorFactory
    {
        private readonly IComparer<object> keyComparer;
        private readonly Dictionary<Type, ITypeDescriptor> registeredDescriptors = new Dictionary<Type, ITypeDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactoryBase"/> class.
        /// </summary>
        /// <param name="keyComparer">The comparer used to sort keys</param>
        protected TypeDescriptorFactoryBase(IComparer<object> keyComparer) : this(keyComparer, new AttributeRegistry())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactoryBase" /> class.
        /// </summary>
        /// <param name="keyComparer">The comparer used to sort keys</param>
        /// <param name="attributeRegistry">The attribute registry.</param>
        /// <exception cref="System.ArgumentNullException">attributeRegistry</exception>
        protected TypeDescriptorFactoryBase(IComparer<object> keyComparer, IAttributeRegistry attributeRegistry)
        {
            if (attributeRegistry == null) throw new ArgumentNullException(nameof(attributeRegistry));
            this.keyComparer = keyComparer;
            AttributeRegistry = attributeRegistry;
        }

        public IAttributeRegistry AttributeRegistry { get; }

        public ITypeDescriptor Find(Type type)
        {
            if (type == null)
                return null;

            // Caching is integrated in this class, avoiding a ChainedTypeDescriptorFactory
            ITypeDescriptor descriptor;
            lock (registeredDescriptors)
            {
                if (!registeredDescriptors.TryGetValue(type, out descriptor))
                {
                    descriptor = Create(type);

                    // Register this descriptor (before initializing!)
                    registeredDescriptors.Add(type, descriptor);

                    // Make sure the descriptor is initialized
                    descriptor.Initialize(keyComparer);
                }
            }

            return descriptor;
        }


        /// <summary>
        /// Creates a type descriptor for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An instance of type descriptor.</returns>
        protected abstract ITypeDescriptor Create(Type type);
    }
}
