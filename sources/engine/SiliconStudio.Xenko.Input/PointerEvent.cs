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
        /// Initializes a new instance of the <see cref="PointerEvent" /> class.
        /// </summary>
        /// <param name="pointer">The device that produces this event</param>
        internal PointerEvent(IPointerDevice pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerEvent" /> class.
        /// </summary>
        /// <param name="pointer">The device that produces this event</param>
        /// <param name="pointerId">The pointer id.</param>
        /// <param name="position">The position.</param>
        /// <param name="deltaPosition">The delta position.</param>
        /// <param name="deltaTime">The delta time.</param>
        /// <param name="state">The state.</param>
        /// <param name="pointerType">Type of the pointer.</param>
        internal PointerEvent(IPointerDevice pointer, int pointerId, Vector2 position, Vector2 deltaPosition, TimeSpan deltaTime, PointerState state, PointerType pointerType) : base(pointer)
        {
            PointerId = pointerId;
            Position = position;
            DeltaPosition = deltaPosition;
            DeltaTime = deltaTime;
            State = state;
            PointerType = pointerType;
        }

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
            return $"{nameof(PointerId)}: {PointerId}, {nameof(Position)}: {Position}, {nameof(DeltaPosition)}: {DeltaPosition}, {nameof(DeltaTime)}: {DeltaTime}, {nameof(State)}: {State}, {nameof(PointerType)}: {PointerType}, {nameof(Pointer)}: {Pointer.DeviceName}";
        }

        public PointerEvent Clone()
        {
            return new PointerEvent(Pointer, PointerId, Position, DeltaPosition, DeltaTime, State, PointerType);
        }
    }
}
