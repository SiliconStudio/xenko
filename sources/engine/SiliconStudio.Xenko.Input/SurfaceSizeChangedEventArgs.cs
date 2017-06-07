// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event for when the size of a pointer surface changed
    /// </summary>
    public class SurfaceSizeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new size of the surface
        /// </summary>
        public Vector2 NewSurfaceSize;
    }
}