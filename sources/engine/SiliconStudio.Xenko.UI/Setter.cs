// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Represents a setter tha sets a property value.
    /// </summary>
    internal abstract class Setter
    {
        /// <summary>
        /// Internal helper to apply value (if not already overriden).
        /// </summary>
        /// <param name="propertyContainer"></param>
        internal abstract void ApplyIfNotSet([NotNull] PropertyContainerClass propertyContainer);
    }

    /// <summary>
    /// Represents a setter tha sets a property value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Setter<T> : Setter
    {
        public Setter(PropertyKey<T> property, T value)
        {
            Property = property;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the property to which <see cref="Value"/> will be set.
        /// </summary>
        public PropertyKey<T> Property { get; set; }

        /// <summary>
        /// Gets or sets the value to set to the property.
        /// </summary>
        public T Value { get; set; }

        /// <inheritdoc/>
        internal override void ApplyIfNotSet(PropertyContainerClass propertyContainer)
        {
            if (!propertyContainer.ContainsKey(Property))
                propertyContainer.Set(Property, Value);
        }
    }
}
