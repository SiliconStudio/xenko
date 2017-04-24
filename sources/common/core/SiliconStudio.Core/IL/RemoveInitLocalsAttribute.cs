// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.IL
{
    /// <summary>
    /// Using this optimization attribute will prevent local variables in this method to be zero-ed in the prologue (if the runtime supports it).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoveInitLocalsAttribute : Attribute
    {
    }
}
