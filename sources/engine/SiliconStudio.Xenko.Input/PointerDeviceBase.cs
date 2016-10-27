// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for pointer devices
    /// </summary>
    public abstract class PointerDeviceBase : IPointerDevice
    {
        protected readonly List<PointerEvent> currentPointerEvents = new List<PointerEvent>();
        protected readonly List<PointerInputEvent> pointerInputEvents = new List<PointerInputEvent>();
        protected readonly List<PointerData> pointerDatas = new List<PointerData>();
        private Vector2 surfaceSize;
        private float aspectRatio;
        private Vector2 invSurfaceSize;
        private Vector2 lastPrimaryPointerPosition;

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <inheritdoc />
        public abstract string DeviceName { get; }

        /// <inheritdoc />
        public abstract Guid Id { get; }

        /// <inheritdoc />
        public int Priority { get; set; }

        /// <inheritdoc />
        public EventHandler<PointerEvent> OnPointer { get; set; }

        /// <inheritdoc />
        public EventHandler OnSurfaceSizeChanged { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<PointerEvent> PointerEvents => currentPointerEvents;

        /// <inheritdoc />
        public Vector2 Position => pointerDatas.Count > 0 ? pointerDatas[0].Position : Vector2.Zero;

        /// <inheritdoc />
        public Vector2 Delta => pointerDatas.Count > 0 ? pointerDatas[0].Delta : Vector2.Zero;

        /// <inheritdoc />
        public abstract PointerType Type { get; }

        /// <inheritdoc />
        public Vector2 SurfaceSize => surfaceSize;

        /// <inheritdoc />
        public Vector2 InverseSurfaceSize => invSurfaceSize;

        /// <inheritdoc />
        public float SurfaceAspectRatio => aspectRatio;

        /// <inheritdoc />
        public virtual void Update(List<InputEvent> inputEvents)
        {
            currentPointerEvents.Clear();

            // Reset delta for all pointers before processing newly received events
            foreach (var pointerData in pointerDatas)
            {
                pointerData.Delta = Vector2.Zero;
            }

            // Turn internal input events into pointer events and mouse position + delta
            foreach (var evt in pointerInputEvents)
            {
                inputEvents.Add(ProcessPointerEvent(evt));
            }
            pointerInputEvents.Clear();
        }

        public void HandlePointerDown()
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Down, Position = lastPrimaryPointerPosition, Id = 0 });
        }

        public void HandlePointerUp()
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Up, Position = lastPrimaryPointerPosition, Id = 0 });
        }

        public void HandleMove(Vector2 newPosition)
        {
            // Normalize position
            newPosition *= invSurfaceSize;

            if (newPosition == lastPrimaryPointerPosition)
                return;

            lastPrimaryPointerPosition = newPosition;

            // Generate Event
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Move, Position = newPosition, Id = 0 });
        }

        // Special move that only registers mouse delta
        public void HandleMoveDelta(Vector2 delta)
        {
            if (delta == Vector2.Zero)
                return;

            // Normalize delta
            delta *= invSurfaceSize;

            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.MoveDelta, Position = delta, Id = 0 });
        }

        protected void SetSurfaceSize(Vector2 newSize)
        {
            surfaceSize = newSize;
            aspectRatio = SurfaceSize.Y/SurfaceSize.X;
            invSurfaceSize = 1.0f/SurfaceSize;
            OnSurfaceSizeChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Processes a <see cref="PointerInputEvent"/> by converting calling <see cref="HandlePointerEvent"/> with the correct arguments
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

                if (evt.Type == InputEventType.Down)
                {
                    pointerState = PointerState.Down;
                }
                else if (evt.Type == InputEventType.Up)
                {
                    pointerState = PointerState.Up;
                }
                else
                {
                    pointerState = PointerState.Move;
                }
            }
            
            if (pointerState == PointerState.Down)
            {
                data.PointerClock.Restart();
                data.Down = true;
            }
            else if (pointerState == PointerState.Up)
            {
                data.Down = false;
            }

            var pointerEvent = new PointerEvent(this, pointerId,
                data.Position, data.Delta, data.PointerClock.Elapsed, pointerState, Type, data.Down);
            
            currentPointerEvents.Add(pointerEvent);
            OnPointer?.Invoke(this, pointerEvent);

            // Reset pointer clock
            data.PointerClock.Restart();

            return pointerEvent;
        }

        private PointerData GetPointerData(int pointerId)
        {
            while (pointerDatas.Count <= pointerId)
            {
                pointerDatas.Add(new PointerData());
            }
            return pointerDatas[pointerId];
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
            MoveDelta,
        }
    }
}