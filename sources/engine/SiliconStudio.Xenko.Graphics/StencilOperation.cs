// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
