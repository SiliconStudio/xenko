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
        public Vector2 SurfaceSize => surfaceSize;
        public Vector2 InverseSurfaceSize => invSurfaceSize;
        public float SurfaceAspectRatio => aspectRatio;
        
        public event EventHandler SurfaceSizeChanged;

        public virtual void Update(List<InputEvent> inputEvents)
        {
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
            PointerInputEvents.Add(new PointerInputEvent { Type = PointerEventType.Pressed, Position = lastPrimaryPointerPosition, Id = 0 });
        }

        /// <summary>
        /// Handles a single pointer up
        /// </summary>
        protected void HandlePointerUp()
        {
            PointerInputEvents.Add(new PointerInputEvent { Type = PointerEventType.Released, Position = lastPrimaryPointerPosition, Id = 0 });
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
            PointerInputEvents.Add(new PointerInputEvent { Type = PointerEventType.Moved, Position = newPosition, Id = 0 });
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

            // Update pointer position + delta
            // Update delta
            data.Delta = evt.Position - data.Position;
            // Update position
            data.Position = evt.Position;

            if (evt.Type == PointerEventType.Pressed)
            {
                data.PointerClock.Restart();
                data.Down = true;
            }
            else if (evt.Type == PointerEventType.Released || evt.Type == PointerEventType.Canceled)
            {
                data.Down = false;
            }

            var pointerEvent = InputEventPool<PointerEvent>.GetOrCreate(this);
            pointerEvent.Position = data.Position;
            pointerEvent.DeltaPosition = data.Delta;
            pointerEvent.DeltaTime = data.PointerClock.Elapsed;
            pointerEvent.IsDown = data.Down;
            pointerEvent.PointerId = evt.Id;
            pointerEvent.PointerType = Type;
            pointerEvent.EventType = evt.Type;

            // Reset pointer clock
            data.PointerClock.Restart();

            return pointerEvent;
        }

        protected PointerData GetPointerData(int pointerId)
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

        /// <summary>
        /// Simplified event data used to generate the full events when <see cref="Update"/> gets called
        /// </summary>
        protected struct PointerInputEvent
        {
            public PointerEventType Type;
            public Vector2 Position;
            public Vector2 Delta;
            public int Id;
        }
    }
}