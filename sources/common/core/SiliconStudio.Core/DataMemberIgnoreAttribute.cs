// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// When specified on a property or field, it will not be used when serializing/deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class DataMemberIgnoreAttribute : Attribute
    {
    }
}
