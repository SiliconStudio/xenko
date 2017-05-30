// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 

using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Implementing a new graphic backend requires copying all the content of the Null graphic backend
    /// to a new folder and start implementing all the members. 
    /// To make it easy the default implementation of them in the null backend won't throw
    /// unless you change <see cref="isThrowing"/> to true.
    /// </summary>
    internal static class NullHelper
    {
        private const bool isThrowing = false;

        /// <summary>
        /// Depending on the system configuration, it will do nothing or throw a NotImplementedException.
        /// </summary>
        public static void ToImplement()
        {
            if (isThrowing)
            {
                throw new NotImplementedException();
            }
        }
    }
}

#endif
