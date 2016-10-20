using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Describe a member of an object.
    /// </summary>
    public interface IMemberDescriptor
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        /// <value>The type.</value>
        Type Type { get; }

        /// <summary>
        /// Gets the type that is declaring this member.
        /// </summary>
        /// <value>The type that is declaring this member.</value>
        Type DeclaringType { get; }

        /// <summary>
        /// Gets the type descriptor of the member.
        /// </summary>
        ITypeDescriptor TypeDescriptor { get; }

        /// <summary>
        /// Gets the order of this member. 
        /// Default is -1, meaning that it is using the alphabetical order 
        /// based on the name of this property.
        /// </summary>
        /// <value>The order.</value>
        int? Order { get; }

        /// <summary>
        /// Gets the mode of serialization for this member.
        /// </summary>
        /// <value>The mode.</value>
        DataMemberMode Mode { get; }

        /// <summary>
        /// Gets the value of this member for the specified instance.
        /// </summary>
        /// <param name="thisObject">The this object to get the value from.</param>
        /// <returns>Value of the member.</returns>
        object Get(object thisObject);

        /// <summary>
        /// Sets a value of this member for the specified instance.
        /// </summary>
        /// <param name="thisObject">The this object.</param>
        /// <param name="value">The value.</param>
        void Set(object thisObject, object value);

        /// <summary>
        /// Gets a value indicating whether this instance has set method.
        /// </summary>
        /// <value><c>true</c> if this instance has set method; otherwise, <c>false</c>.</value>
        bool HasSet { get; }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inherit">if set to <c>true</c> [inherited].</param>
        /// <returns></returns>
        IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute;
    }
}
