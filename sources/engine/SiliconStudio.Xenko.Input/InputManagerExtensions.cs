using SiliconStudio.Core.Mathematics;
using System;

namespace SiliconStudio.Xenko.Input.Extensions
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

    //todo very much WIP, I will change the way these method are exposed

    public static class InputManagerExtensions
    {
        public static SimulatedKeyPress SimulateKeyPress(this InputManager iputManager, Keys key)
        {
            lock (iputManager.KeyboardInputEvents)
            {
                iputManager.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Down });
            }
            return new SimulatedKeyPress { InputManager = iputManager, Key = key };
        }

        public static void SimulateKeyDown(this InputManager iputManager, Keys key)
        {
            lock (iputManager.KeyboardInputEvents)
            {
                iputManager.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Down });
            }
        }

        public static void SimulateKeyUp(this InputManager iputManager, Keys key)
        {
            lock (iputManager.KeyboardInputEvents)
            {
                iputManager.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Up });
            }
        }

        public static void SimulateTapDown(this InputManager inputManager, Vector2 coords)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, Vector2.Zero, TimeSpan.Zero, PointerState.Down, PointerType.Touch, true));
        }

        public static void SimulateTapMove(this InputManager inputManager, Vector2 coords, Vector2 deltaCoords, TimeSpan delta)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, deltaCoords, delta, PointerState.Move, PointerType.Touch, true));
        }

        public static void SimulateTapUp(this InputManager inputManager, Vector2 coords, Vector2 deltaCoords, TimeSpan delta)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, deltaCoords, delta, PointerState.Up, PointerType.Touch, true));
        }

        public static SimulatedTap SimulateTap(this InputManager inputManager, Vector2 coords)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, Vector2.Zero, TimeSpan.Zero, PointerState.Down, PointerType.Touch, true));
            return new SimulatedTap { Coords = coords, InputManager = inputManager };
        }
    }
}