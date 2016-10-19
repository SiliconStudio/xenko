using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
    /// </summary>
    public abstract class TypeDescriptorFactoryBase : ITypeDescriptorFactory
    {
        private readonly Dictionary<Type, ITypeDescriptor> registeredDescriptors = new Dictionary<Type, ITypeDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactoryBase"/> class.
        /// </summary>
        protected TypeDescriptorFactoryBase() : this(new AttributeRegistry())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactoryBase" /> class.
        /// </summary>
        /// <param name="attributeRegistry">The attribute registry.</param>
        /// <exception cref="System.ArgumentNullException">attributeRegistry</exception>
        protected TypeDescriptorFactoryBase(IAttributeRegistry attributeRegistry)
        {
            if (attributeRegistry == null) throw new ArgumentNullException(nameof(attributeRegistry));
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

                    registeredDescriptors.Add(type, descriptor);  // Register this descriptor
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
