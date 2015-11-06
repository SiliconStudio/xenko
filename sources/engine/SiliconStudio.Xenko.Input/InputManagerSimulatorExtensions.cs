using System;
using SiliconStudio.Core.Mathematics;

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
            InputManager.PointerEvents.Add(new PointerEvent(1, Coords, Vector2.Zero, TimeSpan.Zero, PointerState.Up, PointerType.Touch, true));
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

        public static SimulatedTap SimulateTap(this InputManager b, Vector2 coords)
        {
            b.PointerEvents.Add(new PointerEvent(1, coords, Vector2.Zero, TimeSpan.Zero, PointerState.Down, PointerType.Touch, true));
            return new SimulatedTap { Coords = coords, InputManager = b };
        }
    }
}