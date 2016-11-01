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
            public override bool IsMousePositionLocked => false;
            
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

            public override void SetMousePosition(Vector2 position)
            {
                HandleMove(position);
            }
            
            public void SimulatePointer(PointerState state, Vector2 position)
            {
                InputEventType eventType;
                switch (state)
                {
                    case PointerState.Down:
                        eventType = InputEventType.Down;
                        break;
                    case PointerState.Move:
                        eventType = InputEventType.Move;
                        break;
                    case PointerState.Up:
                        eventType = InputEventType.Up;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                PointerInputEvents.Add(new PointerInputEvent { Id = 0, Position = position, Type = eventType });
            }

            public void InjectPointerEvent(Vector2 position, Vector2 deltaPosition, TimeSpan delta, PointerState state, int id = 0, PointerType type = PointerType.Mouse)
            {
                injectedPointerEvents.Add(new PointerEvent(this)
                {
                    Position = position, 
                    DeltaPosition = deltaPosition,
                    DeltaTime = delta,
                    IsDown = state != PointerState.Up,
                    PointerId = id,
                    PointerType = type,
                    State = state
                });
            }

            public override void LockMousePosition(bool forceCenter = false)
            {
            }

            public override void UnlockMousePosition()
            {
            }
        }
    }
}