using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
    /// </summary>
    public abstract class TypeDescriptorFactoryBase : ITypeDescriptorFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDescriptorFactoryBase"/> class.
        /// </summary>
        /// <param name="keyComparer">The comparer used to sort keys</param>
        protected TypeDescriptorFactoryBase(IComparer<object> keyComparer) : this(keyComparer, new AttributeRegistry())
        {
        }

    }
}
