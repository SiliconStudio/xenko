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
        public abstract string DeviceName { get; }
        public abstract Guid Id { get; }
        public int Priority { get; set; }

        public EventHandler<PointerEvent> OnPointer { get; set; }
        public EventHandler OnSurfaceSizeChanged { get; set; }
        public IReadOnlyList<PointerEvent> PointerEvents => currentPointerEvents;

        public Vector2 Position => pointerDatas.Count > 0 ? pointerDatas[0].Position : Vector2.Zero;
        public Vector2 Delta => pointerDatas.Count > 0 ? pointerDatas[0].Delta : Vector2.Zero;

        public abstract PointerType Type { get; }
        public Vector2 SurfaceSize => surfaceSize;
        public Vector2 InverseSurfaceSize => invSurfaceSize;
        public float SurfaceAspectRatio => aspectRatio;


        protected readonly List<PointerEvent> currentPointerEvents = new List<PointerEvent>();
        protected readonly List<PointerInputEvent> pointerInputEvents = new List<PointerInputEvent>();

        protected readonly List<PointerData> pointerDatas = new List<PointerData>();

        private Vector2 surfaceSize;
        private float aspectRatio;
        private Vector2 invSurfaceSize;

        public virtual void Update()
        {
            ClearPointerEvents();

            // Reset delta for all pointers before processing newly received events
            foreach (var pointerData in pointerDatas)
            {
                pointerData.Delta = Vector2.Zero;
            }

            // Turn internal input events into pointer events and mouse position + delta
            foreach (var evt in pointerInputEvents)
            {
                ConvertToPointerEvent(evt);
            }

            pointerInputEvents.Clear();
        }

        public virtual void Dispose()
        {
        }

        public void HandlePointerDown()
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Down, Position = Position, Id = 0 });
        }

        public void HandlePointerUp()
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Up, Position = Position, Id = 0 });
        }

        public void HandleMove(Vector2 newPosition)
        {
            // Normalize position
            newPosition *= invSurfaceSize;

            // Generate Event
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Move, Position = newPosition, Id = 0 });
        }

        // Special move that only registers mouse delta
        public void HandleMoveDelta(Vector2 delta)
        {
            // Normalize delta
            delta *= invSurfaceSize;

            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.MoveDelta, Position = delta, Id = 0 });
        }

        internal void HandlePointerEvents(PointerState pointerState, int pointerId)
        {
            var data = GetPointerData(pointerId);
            if (pointerState == PointerState.Down)
            {
                data.PointerClock.Restart();
                data.Down = true;
            }
            else if (pointerState == PointerState.Up)
            {
                data.Down = false;
            }
            else
            {
                // Ignore move events when not between Down/Up events
                if (!data.Down)
                    return;
            }

            var pointerEvent = PointerEvent.GetOrCreatePointerEvent();

            pointerEvent.PointerId = pointerId;
            pointerEvent.Position = data.Position;
            pointerEvent.DeltaPosition = data.Delta;
            pointerEvent.DeltaTime = data.PointerClock.Elapsed;
            pointerEvent.State = pointerState;
            pointerEvent.PointerType = Type;
            pointerEvent.IsPrimary = pointerId == 0;

            lock (currentPointerEvents)
                currentPointerEvents.Add(pointerEvent);

            OnPointer?.Invoke(this, pointerEvent);
            data.PointerClock.Restart();
        }

        protected void ClearPointerEvents()
        {
            // Clear pointer events
            lock (PointerEvent.Pool)
                lock (currentPointerEvents)
                {
                    // Insert back into pool
                    foreach (var pointerEvent in currentPointerEvents)
                        PointerEvent.Pool.Enqueue(pointerEvent);
                    currentPointerEvents.Clear();
                }
        }

        protected void SetSurfaceSize(Vector2 newSize)
        {
            surfaceSize = newSize;
            aspectRatio = SurfaceSize.Y/SurfaceSize.X;
            invSurfaceSize = 1.0f / SurfaceSize;
            OnSurfaceSizeChanged?.Invoke(this, null);
        }

        private void ConvertToPointerEvent(PointerInputEvent evt)
        {
            var data = GetPointerData(evt.Id);

            // Update pointer position + delta
            if (evt.Type == InputEventType.MoveDelta)
            {
                // Special case, used when the cursor is locked to the center of the screen and only wants to send delta events
                data.Delta = evt.Position;
                HandlePointerEvents(PointerState.Move, evt.Id);
            }
            else
            {
                // Update delta
                data.Delta = evt.Position - data.Position;
                // Update position
                data.Position = evt.Position;

                if (evt.Type == InputEventType.Down)
                {
                    HandlePointerEvents(PointerState.Down, evt.Id);
                }
                else if (evt.Type == InputEventType.Up)
                {
                    HandlePointerEvents(PointerState.Up, evt.Id);
                }
                else
                {
                    HandlePointerEvents(PointerState.Move, evt.Id);
                }
            }

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