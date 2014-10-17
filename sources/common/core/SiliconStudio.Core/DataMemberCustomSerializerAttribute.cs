// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// When specified on a property or field, a serializer won't be needed for this type (useful if serializer is dynamically or manually registered).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DataMemberCustomSerializerAttribute : Attribute
    {
        
    }
}