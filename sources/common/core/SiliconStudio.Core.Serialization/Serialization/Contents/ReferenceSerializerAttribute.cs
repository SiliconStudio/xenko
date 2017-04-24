// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// Used to detect whether a type is using <see cref="ReferenceSerializer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [AssemblyScan]
    public class ReferenceSerializerAttribute : Attribute
    {
    }
}
