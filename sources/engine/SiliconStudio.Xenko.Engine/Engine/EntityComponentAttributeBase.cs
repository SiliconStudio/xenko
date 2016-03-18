// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Base class for attributes for <see cref="EntityComponent"/>
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class EntityComponentAttributeBase : Attribute
    {
    }
}