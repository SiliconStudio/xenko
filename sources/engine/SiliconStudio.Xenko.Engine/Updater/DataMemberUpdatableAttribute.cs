// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines this member should be supported by <see cref="UpdateEngine"/>
    /// even if <see cref="SiliconStudio.Core.DataMemberIgnoreAttribute"/> is applied on it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DataMemberUpdatableAttribute : Attribute
    {
    }
}
