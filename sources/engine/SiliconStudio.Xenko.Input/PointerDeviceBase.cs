// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
        private Vector2 invSurfaceSize;
        private float aspectRatio;

        public abstract string Name { get; }

        public abstract Guid Id { get; }

        public abstract PointerType Type { get; }

        public int Priority { get; set; }

        public abstract IInputSource Source { get; }

        public Vector2 SurfaceSize => surfaceSize;

        public Vector2 InverseSurfaceSize => invSurfaceSize;

        public float SurfaceAspectRatio => aspectRatio;

        public IReadOnlyList<PointerPoint> PointerPoints => PointerDatas;

        public event EventHandler<SurfaceSizeChangedEventArgs> SurfaceSizeChanged;

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
        /// Updates the surface size of the pointing device, updates <see cref="SurfaceSize"/>, <see cref="SurfaceAspectRatio"/>, <see cref="invSurfaceSize"/> and calls <see cref="SurfaceSizeChanged"/>
        /// </summary>
        /// <param name="newSize">New size of the surface</param>
        protected void SetSurfaceSize(Vector2 newSize)
        {
            surfaceSize = newSize;
            aspectRatio = SurfaceSize.Y/SurfaceSize.X;
            invSurfaceSize = 1.0f/SurfaceSize;
            SurfaceSizeChanged?.Invoke(this, new SurfaceSizeChangedEventArgs { NewSurfaceSize = newSize });
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
                data.Clock.Restart();
                data.IsDown = true;
            }
            else if (evt.Type == PointerEventType.Released || evt.Type == PointerEventType.Canceled)
            {
                data.IsDown = false;
            }
            
            var pointerEvent = InputEventPool<PointerEvent>.GetOrCreate(this);
            pointerEvent.Position = data.Position;
            pointerEvent.DeltaPosition = data.Delta;
            pointerEvent.DeltaTime = data.Clock.Elapsed;
            pointerEvent.IsDown = data.IsDown;
            pointerEvent.PointerId = evt.Id;
            pointerEvent.PointerType = Type;
            pointerEvent.EventType = evt.Type;

            // Reset pointer clock
            data.Clock.Restart();

            return pointerEvent;
        }

        protected PointerData GetPointerData(int pointerId)
        {
            while (PointerDatas.Count <= pointerId)
            {
                PointerDatas.Add(new PointerData {Pointer = this});
            }
            return PointerDatas[pointerId];
        }

        /// <summary>
        /// Some additional data kept on top of <see cref="PointerPoint"/> for the purpose of generating <see cref="PointerEvent"/>
        /// </summary>
        protected class PointerData : PointerPoint
        {
            public Stopwatch Clock = new Stopwatch();
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