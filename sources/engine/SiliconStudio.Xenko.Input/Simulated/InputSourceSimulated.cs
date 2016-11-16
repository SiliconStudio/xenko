// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides a virtual mouse and keyboard that generate input events like a normal mouse/keyboard when any of the functions (Simulate...) are called
    /// </summary>
    public class InputSourceSimulated : InputSourceBase
    {
        /// <summary>
        /// Should simulated input added to the input manager by default
        /// </summary>
        public static bool Enabled = false;

        /// <summary>
        /// The simulated input source
        /// </summary>
        public static InputSourceSimulated Instance;

        public KeyboardSimulated Keyboard;
        public MouseSimulated Mouse;

        public override void Initialize(InputManager inputManager)
        {
            Keyboard = new KeyboardSimulated();
            Mouse = new MouseSimulated();
            RegisterDevice(Keyboard);
            RegisterDevice(Mouse);
            Instance = this;
        }

        public class KeyboardSimulated : KeyboardDeviceBase
        {
            public KeyboardSimulated()
            {
                Priority = -1000;
            }

            public override string DeviceName => "Simulated Keyboard";
            public override Guid Id => new Guid(10, 10, 1, 0, 0, 0, 0, 0, 0, 0, 0);

            public void SimulateDown(Keys key)
            {
                HandleKeyDown(key);
            }

            public void SimulateUp(Keys key)
            {
                HandleKeyUp(key);
            }
        }

        public class MouseSimulated : MouseDeviceBase
        {
            private readonly List<PointerEvent> injectedPointerEvents = new List<PointerEvent>();

            public MouseSimulated()
            {
                Priority = -1000;
                SetSurfaceSize(Vector2.One);
            }

            public override string DeviceName => "Simulated Mouse";
            public override Guid Id => new Guid(10, 10, 2, 0, 0, 0, 0, 0, 0, 0, 0);
            public override bool IsPositionLocked => false;
            
            public override void Update(List<InputEvent> inputEvents)
            {
                base.Update(inputEvents);
                inputEvents.AddRange(injectedPointerEvents);
                injectedPointerEvents.Clear();
            }

            public void SimulateMouseDown(MouseButton button)
            {
                HandleButtonDown(button);
            }

            public void SimulateMouseUp(MouseButton button)
            {
                HandleButtonUp(button);
            }

            public override void SetPosition(Vector2 position)
            {
                HandleMove(position);
            }
            
            public void SimulatePointer(PointerEventType pointerEventType, Vector2 position)
            {
                PointerInputEvents.Add(new PointerInputEvent { Id = 0, Position = position, Type = pointerEventType });
            }

            public void InjectPointerEvent(Vector2 position, Vector2 deltaPosition, TimeSpan delta, PointerEventType eventType, int id = 0, PointerType type = PointerType.Mouse)
            {
                var pointerEvent = InputEventPool<PointerEvent>.GetOrCreate(this);
                pointerEvent.Position = position;
                pointerEvent.DeltaPosition = deltaPosition;
                pointerEvent.DeltaTime = delta;
                pointerEvent.IsDown = eventType != PointerEventType.Released;
                pointerEvent.PointerId = id;
                pointerEvent.PointerType = type;
                pointerEvent.EventType = eventType;

                injectedPointerEvents.Add(pointerEvent);
            }

            public override void LockPosition(bool forceCenter = false)
            {
            }

            public override void UnlockPosition()
            {
            }
        }
    }
}