using SiliconStudio.Core.Mathematics;
using System;

namespace SiliconStudio.Xenko.Input.Extensions
{
    public static class InputManagerExtensions
    {
        public static void SimulateKeyDown(this InputManager inputManager, Keys key)
        {
            lock (inputManager.KeyboardInputEvents)
            {
                inputManager.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Down });
            }
        }

        public static void SimulateKeyUp(this InputManager inputManager, Keys key)
        {
            lock (inputManager.KeyboardInputEvents)
            {
                inputManager.KeyboardInputEvents.Add(new InputManagerBase.KeyboardInputEvent { Key = key, Type = InputManagerBase.InputEventType.Up });
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
    }
}