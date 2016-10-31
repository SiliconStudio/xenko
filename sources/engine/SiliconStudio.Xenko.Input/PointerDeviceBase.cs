// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for pointer devices
    /// </summary>
    public abstract class PointerDeviceBase : IPointerDevice
    {
        protected readonly List<PointerEvent> CurrentPointerEvents = new List<PointerEvent>();
        protected readonly List<PointerInputEvent> PointerInputEvents = new List<PointerInputEvent>();
        protected readonly List<PointerData> PointerDatas = new List<PointerData>();
        private Vector2 surfaceSize;
        private float aspectRatio;
        private Vector2 invSurfaceSize;
        private Vector2 lastPrimaryPointerPosition;

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        public abstract string DeviceName { get; }
        public abstract Guid Id { get; }
        public abstract PointerType Type { get; }
        public int Priority { get; set; }
        public IReadOnlyList<PointerEvent> PointerEvents => CurrentPointerEvents;
        public Vector2 Position => PointerDatas.Count > 0 ? PointerDatas[0].Position : Vector2.Zero;
        public Vector2 Delta => PointerDatas.Count > 0 ? PointerDatas[0].Delta : Vector2.Zero;
        public Vector2 SurfaceSize => surfaceSize;
        public Vector2 InverseSurfaceSize => invSurfaceSize;
        public float SurfaceAspectRatio => aspectRatio;
        
        public event EventHandler SurfaceSizeChanged;

        public virtual void Update(List<InputEvent> inputEvents)
        {
            CurrentPointerEvents.Clear();

            // Reset delta for all pointers before processing newly received events
            foreach (var pointerData in PointerDatas)
            {
                pointerData.Delta = Vector2.Zero;
            }

            // Turn internal input events into pointer events and mouse position + delta
            foreach (var evt in PointerInputEvents)
            {
                inputEvents.Add(ProcessPointerEvent(evt));
            }
            PointerInputEvents.Clear();
        }

        /// <summary>
        /// Handles a single pointer down
        /// </summary>
        protected void HandlePointerDown()
        {
            PointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Down, Position = lastPrimaryPointerPosition, Id = 0 });
        }

        /// <summary>
        /// Handles a single pointer up
        /// </summary>
        protected void HandlePointerUp()
        {
            PointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Up, Position = lastPrimaryPointerPosition, Id = 0 });
        }

        /// <summary>
        /// Handles a single pointer move
        /// </summary>
        /// <param name="newPosition">New position of the pointer</param>
        protected void HandleMove(Vector2 newPosition)
        {
            // Normalize position
            newPosition *= invSurfaceSize;

            if (newPosition == lastPrimaryPointerPosition)
                return;

            lastPrimaryPointerPosition = newPosition;

            // Generate Event
            PointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Move, Position = newPosition, Id = 0 });
        }

        /// <summary>
        /// Special move that only registers mouse delta
        /// </summary>
        /// <param name="delta">The movement delta</param>
        protected void HandleMoveDelta(Vector2 delta)
        {
            if (delta == Vector2.Zero)
                return;

            // Normalize delta
            delta *= invSurfaceSize;

            PointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.MoveDelta, Position = delta, Id = 0 });
        }

        /// <summary>
        /// Updates the surface size of the pointing device, updates <see cref="SurfaceSize"/>, <see cref="SurfaceAspectRatio"/>, <see cref="invSurfaceSize"/> and calls <see cref="SurfaceSizeChanged"/>
        /// </summary>
        /// <param name="newSize">New size of the surface</param>
        protected void SetSurfaceSize(Vector2 newSize)
        {
            surfaceSize = newSize;
            aspectRatio = SurfaceSize.Y/SurfaceSize.X;
            invSurfaceSize = 1.0f/SurfaceSize;
            SurfaceSizeChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Processes a <see cref="PointerInputEvent"/>, converting it to a <see cref="PointerEvent"/>. Also calls <see cref="OnPointer"/> and updates <see cref="CurrentPointerEvents"/>
        /// </summary>
        /// <param name="evt"></param>
        protected PointerEvent ProcessPointerEvent(PointerInputEvent evt)
        {
            var data = GetPointerData(evt.Id);
            var pointerId = evt.Id;
            PointerState pointerState = 0;

            // Update pointer position + delta
            if (evt.Type == InputEventType.MoveDelta)
            {
                // Special case, used when the cursor is locked to the center of the screen and only wants to send delta events
                data.Delta = evt.Position;
                pointerState = PointerState.Move;
            }
            else
            {
                // Update delta
                data.Delta = evt.Position - data.Position;
                // Update position
                data.Position = evt.Position;

                switch (evt.Type)
                {
                    case InputEventType.Cancel:
                        pointerState = PointerState.Cancel;
                        break;
                    case InputEventType.Out:
                        pointerState = PointerState.Out;
                        break;
                    case InputEventType.Down:
                        pointerState = PointerState.Down;
                        break;
                    case InputEventType.Up:
                        pointerState = PointerState.Up;
                        break;
                    default:
                        pointerState = PointerState.Move;
                        break;
                }
            }

            if (pointerState == PointerState.Down)
            {
                data.PointerClock.Restart();
                data.Down = true;
            }
            else if (pointerState == PointerState.Up || pointerState == PointerState.Cancel)
            {
                data.Down = false;
            }

            var pointerEvent = new PointerEvent(this, pointerId,
                data.Position, data.Delta, data.PointerClock.Elapsed, pointerState, Type, data.Down);

            CurrentPointerEvents.Add(pointerEvent);

            // Reset pointer clock
            data.PointerClock.Restart();

            return pointerEvent;
        }

        private PointerData GetPointerData(int pointerId)
        {
            while (PointerDatas.Count <= pointerId)
            {
                PointerDatas.Add(new PointerData());
            }
            return PointerDatas[pointerId];
        }

        /// <summary>
        /// Some data kept for each pointer id(finger) or mouse(only 1 in that case)
        /// </summary>
        protected class PointerData
        {
            /// <summary>
            /// Time since last pointer event
            /// </summary>
            public Stopwatch PointerClock = new Stopwatch();

            /// <summary>
            /// Last known position
            /// </summary>
            public Vector2 Position = Vector2.Zero;

            /// <summary>
            /// Distance moved since last frame
            /// </summary>
            public Vector2 Delta;

            /// <summary>
            /// Down state of the pointer
            /// </summary>
            public bool Down;
        }

        protected struct PointerInputEvent
        {
            public InputEventType Type;
            public Vector2 Position;
            public Vector2 Delta;
            public int Id;
        }

        protected enum InputEventType
        {
            Up,
            Down,
            Move,
            Out,
            Cancel,
            MoveDelta,
        }
    }
}