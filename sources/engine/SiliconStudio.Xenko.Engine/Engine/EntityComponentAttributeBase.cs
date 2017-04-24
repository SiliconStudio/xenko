// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
