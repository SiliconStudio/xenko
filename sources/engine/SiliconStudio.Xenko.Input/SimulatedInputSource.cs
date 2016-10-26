// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows.Input;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    public class SimulatedInputSource : InputSourceBase
    {
        /// <summary>
        /// Should simulated input be enabled
        /// </summary>
        public static bool Enabled = false;

        /// <summary>
        /// The simulated input source
        /// </summary>
        public static SimulatedInputSource Instance;

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

        public override bool IsEnabled(GameContext gameContext)
        {
            return Enabled;
        }

        public class KeyboardSimulated : KeyboardDeviceBase
        {
            public override string DeviceName => "Simulated Keyboard";
            public override Guid Id => new Guid(10, 10, 1, 0, 0, 0, 0, 0, 0, 0, 0);

            public KeyboardSimulated()
            {
                Priority = -1000;
            }

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
            public override string DeviceName => "Simulated Mouse";
            public override Guid Id => new Guid(10, 10, 2, 0, 0, 0, 0, 0, 0, 0, 0);
            public override bool IsMousePositionLocked => false;

            public MouseSimulated()
            {
                Priority = -1000;
                SetSurfaceSize(Vector2.One);
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
                pointerInputEvents.Add(new PointerInputEvent { Id = 0, Position = position, Type = eventType });
            }

            public override void LockMousePosition(bool forceCenter = false)
            {
                throw new NotImplementedException();
            }

            public override void UnlockMousePosition()
            {
                throw new NotImplementedException();
            }
        }
    }
}