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
    public class PointerEvent : EventArgs
    {
        public readonly static Queue<PointerEvent> Pool = new Queue<PointerEvent>();

        public static PointerEvent GetOrCreatePointerEvent()
        {
            lock (Pool)
                return Pool.Count > 0 ? Pool.Dequeue() : new PointerEvent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerEvent" /> class.
        /// </summary>
        public PointerEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerEvent" /> class.
        /// </summary>
        /// <param name="pointerId">The pointer id.</param>
        /// <param name="position">The position.</param>
        /// <param name="deltaPosition">The delta position.</param>
        /// <param name="deltaTime">The delta time.</param>
        /// <param name="state">The state.</param>
        /// <param name="pointerType">Type of the pointer.</param>
        /// <param name="isPrimary">if set to <c>true</c> [is primary].</param>
        internal PointerEvent(int pointerId, Vector2 position, Vector2 deltaPosition, TimeSpan deltaTime, PointerState state, PointerType pointerType, bool isPrimary)
        {
            PointerId = pointerId;
            Position = position;
            DeltaPosition = deltaPosition;
            DeltaTime = deltaTime;
            State = state;
            PointerType = pointerType;
            IsPrimary = isPrimary;
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
        /// Gets a boolean indicating whether this is the default primary pointer.
        /// </summary>
        /// <value><c>true</c> if this instance is primary; otherwise, <c>false</c>.</value>
        public bool IsPrimary { get; internal set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>PointerEvent.</returns>
        public PointerEvent Clone()
        {
            var clone = GetOrCreatePointerEvent();

            clone.PointerId = PointerId;
            clone.Position = Position;
            clone.DeltaPosition = DeltaPosition;
            clone.DeltaTime = DeltaTime;
            clone.State = State;
            clone.PointerType = PointerType;
            clone.IsPrimary = IsPrimary;

            return clone;
        }

        public override string ToString()
        {
            return string.Format("PointerId: {0}, Position: {1:0.00}, DeltaPosition: {2:0.00}, DeltaTime: {3:0.000}, State: {4}, PointerType: {5}, IsPrimary: {6}", PointerId, Position, DeltaPosition, DeltaTime.TotalSeconds, State, PointerType, IsPrimary);
        }
    }
}
