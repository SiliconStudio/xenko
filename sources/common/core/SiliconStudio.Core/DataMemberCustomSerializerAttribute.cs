// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
