// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attribute indicates that the associated type cannot be instanced in the property grid
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class NonInstantiableAttribute : Attribute
    {
    }
}
