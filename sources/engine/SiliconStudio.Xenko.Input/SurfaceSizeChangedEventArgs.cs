// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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