// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Allows delegates passed as parameters to be allocated from a pool and recycled after the method call.
    /// To prevent recycling, use <see cref="PooledDelegateHelper.AddReference"/> and <see cref="PooledDelegateHelper.Release"/> to hoold onto references to the delegate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PooledAttribute : Attribute
    {  
    }
}
