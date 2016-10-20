// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A <see cref="IMemberDescriptor"/> for a <see cref="PropertyInfo"/>
    /// </summary>
    public class PropertyDescriptor : MemberDescriptorBase
    {
        private readonly MethodInfo getMethod;
        private readonly MethodInfo setMethod;

        public PropertyDescriptor(ITypeDescriptor typeDescriptor, PropertyInfo propertyInfo, StringComparer defaultNameComparer)
            : base(propertyInfo, defaultNameComparer)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            PropertyInfo = propertyInfo;

            getMethod = propertyInfo.GetGetMethod(false);
            if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(false) != null)
            {
                setMethod = propertyInfo.GetSetMethod(false);
            }
            TypeDescriptor = typeDescriptor;
        }

        /// <summary>
        /// Gets the property information attached to this instance.
        /// </summary>
        /// <value>The property information.</value>
        public PropertyInfo PropertyInfo { get; }

        public override Type Type => PropertyInfo.PropertyType;

        public override bool IsPublic => getMethod.IsPublic;

        public override bool HasSet => setMethod != null;

        public override object Get(object thisObject)
        {
            return getMethod.Invoke(thisObject, null);
        }

        public override void Set(object thisObject, object value)
        {
            if (HasSet)
                setMethod.Invoke(thisObject, new[] {value});
        }

        public override IEnumerable<T> GetCustomAttributes<T>(bool inherit)
        {
            return PropertyInfo.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("Property [{0}] from Type [{1}]", Name, PropertyInfo.DeclaringType != null ? PropertyInfo.DeclaringType.FullName : string.Empty);
        }
    }
}
