// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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