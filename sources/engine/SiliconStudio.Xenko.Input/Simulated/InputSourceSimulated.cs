// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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

        private bool keyboardConnected;
        private bool mouseConnected;

        public KeyboardSimulated Keyboard;

        public MouseSimulated Mouse;

        public override void Initialize(InputManager inputManager)
        {
            Keyboard = new KeyboardSimulated();
            Mouse = new MouseSimulated();
            SetKeyboardConnected(true);
            SetMouseConnected(true);
            Instance = this;
        }

        public override void Dispose()
        {
            base.Dispose();
            Instance = null;
        }

        public void SetKeyboardConnected(bool connected)
        {
            if (connected != keyboardConnected)
            {
                if (connected)
                {
                    RegisterDevice(Keyboard);
                }
                else
                {
                    UnregisterDevice(Keyboard);
                }

                keyboardConnected = connected;
            }
        }

        public void SetMouseConnected(bool connected)
        {
            if (connected != mouseConnected)
            {
                if (connected)
                {
                    RegisterDevice(Mouse);
                }
                else
                {
                    UnregisterDevice(Mouse);
                }

                mouseConnected = connected;
            }
        }

        public class KeyboardSimulated : KeyboardDeviceBase
        {
            public KeyboardSimulated()
            {
                Priority = -1000;
            }

            public override string Name => "Simulated Keyboard";

            public override Guid Id => new Guid(10, 10, 1, 0, 0, 0, 0, 0, 0, 0, 0);

            public override IInputSource Source => Instance;

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
            private bool positionLocked;
            private Vector2 capturedPosition;

            public MouseSimulated()
            {
                Priority = -1000;
                SetSurfaceSize(Vector2.One);
            }

            public override string Name => "Simulated Mouse";

            public override Guid Id => new Guid(10, 10, 2, 0, 0, 0, 0, 0, 0, 0, 0);

            public override bool IsPositionLocked => positionLocked;

            public override IInputSource Source => Instance;

            public override void Update(List<InputEvent> inputEvents)
            {
                base.Update(inputEvents);
                inputEvents.AddRange(injectedPointerEvents);
                injectedPointerEvents.Clear();

                if (positionLocked)
                {
                    Position = capturedPosition;
                    GetPointerData(0).Position = capturedPosition;
                }
            }

            public void SimulateMouseDown(MouseButton button)
            {
                HandleButtonDown(button);
            }

            public void SimulateMouseUp(MouseButton button)
            {
                HandleButtonUp(button);
            }

            public void SimulateMouseWheel(float wheelDelta)
            {
                HandleMouseWheel(wheelDelta);
            }

            public override void SetPosition(Vector2 position)
            {
                if (IsPositionLocked)
                {
                    HandleMouseDelta(position * SurfaceSize - capturedPosition);
                }
                else
                {
                    HandleMove(position * SurfaceSize);
                }
            }
            
            public void SimulatePointer(PointerEventType pointerEventType, Vector2 position, int id = 0)
            {
                PointerInputEvents.Add(new PointerInputEvent { Id = id, Position = position, Type = pointerEventType });
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
                positionLocked = true;
                capturedPosition = forceCenter ? new Vector2(0.5f) : Position;
            }

            public override void UnlockPosition()
            {
                positionLocked = false;
            }
        }
    }
}