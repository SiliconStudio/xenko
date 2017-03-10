// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
