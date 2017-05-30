// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Allows a component of the same type to be added multiple time to the same entity (default is <c>false</c>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowMultipleComponentsAttribute : EntityComponentAttributeBase
    {
    }
}
