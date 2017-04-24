// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// TODO Comments
    /// </summary>
    [DataContract]
    public enum StencilOperation
    {
        /// <summary>
        /// 
        /// </summary>
        Keep = 1,
        /// <summary>
        /// 
        /// </summary>
        Zero = 2,
        /// <summary>
        /// 
        /// </summary>
        Replace = 3,
        /// <summary>
        /// 
        /// </summary>
        IncrementSaturation = 4,
        /// <summary>
        /// 
        /// </summary>
        DecrementSaturation = 5,
        /// <summary>
        /// 
        /// </summary>
        Invert = 6,
        /// <summary>
        /// 
        /// </summary>
        Increment = 7,
        /// <summary>
        /// 
        /// </summary>
        Decrement = 8,
    }
}
