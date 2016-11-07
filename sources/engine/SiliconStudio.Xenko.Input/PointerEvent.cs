// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A pointer event.
    /// </summary>
    public class PointerEvent : InputEvent
    {
        /// <summary>
        /// Gets a unique identifier of the pointer. See remarks.
        /// </summary>
        /// <value>The pointer id.</value>
        /// <remarks>The default mouse pointer will always be affected to the PointerId 0. On a tablet, a pen or each fingers will get a unique identifier.</remarks>
        public int PointerId { get; internal set; }

        /// <summary>
        /// Gets the absolute screen position of the pointer.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position { get; internal set; }

        /// <summary>
        /// Gets the delta position of the pointer since the previous frame.
        /// </summary>
        /// <value>The delta position.</value>
        public Vector2 DeltaPosition { get; set; }

        /// <summary>
        /// Gets the amount of time since the previous state.
        /// </summary>
        /// <value>The delta time.</value>
        public TimeSpan DeltaTime { get; internal set; }

        /// <summary>
        /// Gets the state of this pointer event (down, up, move... etc.)
        /// </summary>
        /// <value>The state.</value>
        public PointerState State { get; internal set; }

        /// <summary>
        /// Gets the type of the pointer.
        /// </summary>
        /// <value>The type of the pointer.</value>
        public PointerType PointerType { get; internal set; }

        /// <summary>
        /// Gets if the pointer is down, useful for filtering out move events that are not placed between drags
        /// </summary>
        public bool IsDown { get; internal set; }

        /// <summary>
        /// The pointer that sent this event
        /// </summary>
        public IPointerDevice Pointer => Device as IPointerDevice;

        public override string ToString()
        {
            return
                $"Pointer {PointerId} {State}, {Position}, Delta: {DeltaPosition}, DT: {DeltaTime}, {nameof(IsDown)}: {IsDown}, {nameof(PointerType)}: {PointerType}, {nameof(Pointer)}: {Pointer.DeviceName}";
        }

        /// <summary>
        /// Clones the pointer event, this is usefull if you intend to use it after this frame, since otherwise it would be recycled by the input manager the next frame
        /// </summary>
        /// <returns>The cloned event</returns>
        public PointerEvent Clone()
        {
            return new PointerEvent
            {
                Device = Device,
                PointerId = PointerId,
                Position = Position,
                DeltaPosition = DeltaPosition,
                DeltaTime = DeltaTime,
                State = State,
                PointerType = PointerType,
                IsDown = IsDown
            };
        }
    }
}