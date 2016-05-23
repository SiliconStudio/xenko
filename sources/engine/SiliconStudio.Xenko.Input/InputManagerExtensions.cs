using SiliconStudio.Core.Mathematics;
using System;

namespace SiliconStudio.Xenko.Input.Extensions
{
    /// <summary>
    /// InputManager extra utility methods
    /// </summary>
    public static class InputManagerExtensions
    {
        /// <summary>
        /// Injects a Key down event into the events stack
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="key">the key you want to simulate</param>
        public static void SimulateKeyDown(this InputManager inputManager, Keys key)
        {
            lock (inputManager.KeyboardInputEvents)
            {
                inputManager.KeyboardInputEvents.Add(new InputManager.KeyboardInputEvent { Key = key, Type = InputManager.InputEventType.Down });
            }
        }

        /// <summary>
        /// Injects a Key up (release) event into the events stack
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="key">the key you want to simulate</param>
        public static void SimulateKeyUp(this InputManager inputManager, Keys key)
        {
            lock (inputManager.KeyboardInputEvents)
            {
                inputManager.KeyboardInputEvents.Add(new InputManager.KeyboardInputEvent { Key = key, Type = InputManager.InputEventType.Up });
            }
        }

        /// <summary>
        /// Injects a tap down touch pointer event into the events stack
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="coords">the coordinates (0.0 -> 1.0) on the screen</param>
        public static void SimulateTapDown(this InputManager inputManager, Vector2 coords)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, Vector2.Zero, TimeSpan.Zero, PointerState.Down, PointerType.Touch, true));
        }

        /// <summary>
        /// Injects a tap move touch pointer event into the events stack
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="coords">the coordinates (0.0 -> 1.0) on the screen</param>
        /// <param name="deltaCoords">the delta coordinates from the previous event</param>
        /// <param name="delta">the delta time between events</param>
        public static void SimulateTapMove(this InputManager inputManager, Vector2 coords, Vector2 deltaCoords, TimeSpan delta)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, deltaCoords, delta, PointerState.Move, PointerType.Touch, true));
        }

        /// <summary>
        /// Injects a tap up (release) touch pointer event into the events stack
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="coords">the coordinates (0.0 -> 1.0) on the screen</param>
        /// <param name="deltaCoords">the delta coordinates from the previous event</param>
        /// <param name="delta">the delta time between events</param>
        public static void SimulateTapUp(this InputManager inputManager, Vector2 coords, Vector2 deltaCoords, TimeSpan delta)
        {
            inputManager.InjectPointerEvent(new PointerEvent(0, coords, deltaCoords, delta, PointerState.Up, PointerType.Touch, true));
        }
    }
}