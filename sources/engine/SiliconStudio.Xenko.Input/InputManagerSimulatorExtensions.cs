using SiliconStudio.Core.Mathematics;
using System;
using System.Diagnostics;

namespace SiliconStudio.Xenko.Input.SimulatorExtensions
{
    public struct SimulatedKeyPress : IDisposable
    {
        internal Keys Key;
        internal InputManager InputManager;

        public void Dispose()
        {
            lock (InputManager.KeyboardInputEvents)
            {
                InputManager.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = Key, Type = InputManagerBase.InputEventType.Up });
            }
        }
    }

    public struct SimulatedTap : IDisposable
    {
        internal Vector2 Coords;
        internal InputManager InputManager;

        public void Dispose()
        {
            InputManager.InjectPointerEvent(new PointerEvent(0, Coords, Vector2.Zero, TimeSpan.Zero, PointerState.Up, PointerType.Touch, true));
        }
    }

    public static class InputManagerSimulatorExtensions
    {
        public static SimulatedKeyPress SimulateKeyPress(this InputManager b, Keys key)
        {
            lock (b.KeyboardInputEvents)
            {
                b.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Down });
            }
            return new SimulatedKeyPress { InputManager = b, Key = key };
        }

        public static void SimulateKeyDown(this InputManager b, Keys key)
        {
            lock (b.KeyboardInputEvents)
            {
                b.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Down });
            }
        }

        public static void SimulateKeyUp(this InputManager b, Keys key)
        {
            lock (b.KeyboardInputEvents)
            {
                b.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Up });
            }
        }

        public static SimulatedTap SimulateTap(this InputManager b, Vector2 coords)
        {
            b.InjectPointerEvent(new PointerEvent(0, coords, Vector2.Zero, TimeSpan.Zero, PointerState.Down, PointerType.Touch, true));
            return new SimulatedTap { Coords = coords, InputManager = b };
        }
    }
}