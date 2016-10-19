// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Describe a member of an object.
    /// </summary>
    public interface IMemberDescriptor : IMemberDescriptorBase
    {
        /// <summary>
        /// Gets the type descriptor of the member.
        /// </summary>
        ITypeDescriptor TypeDescriptor { get; }

        /// <summary>
        /// Gets the type that is declaring this member.
        /// </summary>
        /// <value>The type that is declaring this member.</value>
        Type DeclaringType { get; }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inherit">if set to <c>true</c> [inherited].</param>
        /// <returns></returns>
        IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute;
    }
}
