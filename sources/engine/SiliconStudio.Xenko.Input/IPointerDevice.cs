// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public interface IPointerDevice : IInputDevice
    {
        /// <summary>
        /// Raised when a pointer pointer event is triggered
        /// </summary>
        EventHandler<PointerEvent> OnPointer { get; set; }

        /// <summary>
        /// List of pointer events since the last update
        /// </summary>
        IReadOnlyList<PointerEvent> PointerEvents { get; }

        /// <summary>
        /// The type of the pointer device
        /// </summary>
        PointerType Type { get; }

        /// <summary>
        /// Absolute position of the pointer
        /// </summary>
        Vector2 AbsolutePosition { get; }

        /// <summary>
        /// Difference from the last position to the current position
        /// </summary>
        Vector2 Delta { get; }

        /// <summary>
        /// The size of the surface used by the pointer, for a mouse this is the size of the window, for a touch device, the size of the touch area, etc.
        /// </summary>
        Vector2 SurfaceSize { get; }
    }
}