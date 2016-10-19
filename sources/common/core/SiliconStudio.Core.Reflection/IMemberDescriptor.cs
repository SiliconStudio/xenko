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
    }
}
