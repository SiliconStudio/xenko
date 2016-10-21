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

        public EventHandler<PointerEvent> OnPointer { get; set; }
        public IReadOnlyList<PointerEvent> PointerEvents => currentPointerEvents;

        public Vector2 AbsolutePosition { get; protected set; }
        public Vector2 Delta { get; protected set; }

        public abstract PointerType Type { get; }
        public abstract Vector2 SurfaceSize { get; }

        public readonly Stopwatch PointerClock = new Stopwatch();

        private readonly List<PointerEvent> currentPointerEvents = new List<PointerEvent>();
        private readonly List<PointerInputEvent> pointerInputEvents = new List<PointerInputEvent>();

        public virtual void Update()
        {
            ClearPointerEvents();

            // Fire events
            foreach (var evt in pointerInputEvents)
            {
                if (evt.Type == InputEventType.Down)
                {
                    HandlePointerEvents(PointerState.Down);
                }
                else if (evt.Type == InputEventType.Up)
                {
                    HandlePointerEvents(PointerState.Up);
                }
                else if (evt.Type == InputEventType.MoveDelta)
                {
                    Delta = evt.Position;
                    HandlePointerEvents(PointerState.Move);
                }
                else
                {
                    // Update delta
                    Delta = evt.Position - AbsolutePosition;
                    // Update position
                    AbsolutePosition = evt.Position;

                    HandlePointerEvents(PointerState.Move);
                }
            }
            pointerInputEvents.Clear();
        }

        public void HandlePointerDown()
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Down });
        }

        public void HandlePointerUp()
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Up });
        }

        public void HandleMove(Vector2 newPosition)
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.Move, Position = newPosition });
        }

        // Special move that only registers mouse delta
        public void HandleMoveDelta(Vector2 delta)
        {
            pointerInputEvents.Add(new PointerInputEvent { Type = InputEventType.MoveDelta, Position = delta });
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

        internal void HandlePointerEvents(PointerState pState)
        {
            if (pState == PointerState.Down)
            {
                PointerClock.Restart();
            }

            var pointerEvent = PointerEvent.GetOrCreatePointerEvent();

            pointerEvent.PointerId = 0; // TODO
            pointerEvent.Position = AbsolutePosition;
            pointerEvent.DeltaPosition = Delta;
            pointerEvent.DeltaTime = PointerClock.Elapsed;
            pointerEvent.State = pState;
            pointerEvent.PointerType = Type;
            pointerEvent.IsPrimary = true; // TODO

            lock (currentPointerEvents)
                currentPointerEvents.Add(pointerEvent);

            OnPointer?.Invoke(this, pointerEvent);
            PointerClock.Restart();
        }

        public virtual void Dispose()
        {
        }

        protected struct PointerInputEvent
        {
            public InputEventType Type;
            public Vector2 Position;
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