// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// InputManager extra utility methods
    /// </summary>
    public static class InputManagerExtensions
    {
        /// <summary>
        /// Injects a Key down event into the events stack, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="key">the key you want to simulate</param>
        public static void SimulateKeyDown(this InputManager inputManager, Keys key)
        {
            InputSourceSimulated.Instance.Keyboard.SimulateDown(key);
        }

        /// <summary>
        /// Injects a Key up (release) event into the events stack, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="key">the key you want to simulate</param>
        public static void SimulateKeyUp(this InputManager inputManager, Keys key)
        {
            InputSourceSimulated.Instance.Keyboard.SimulateUp(key);
        }

        /// <summary>
        /// Simulate mouse button presses, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="button">the mouse button</param>
        public static void SimulateMouseDown(this InputManager inputManager, MouseButton button)
        {
            InputSourceSimulated.Instance.Mouse.SimulateMouseDown(button);
        }

        /// <summary>
        /// Simulate mouse button releases, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="button">the mouse button</param>
        public static void SimulateMouseUp(this InputManager inputManager, MouseButton button)
        {
            InputSourceSimulated.Instance.Mouse.SimulateMouseUp(button);
        }

        /// <summary>
        /// Simulate mouse wheel movement, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="wheelDelta">the amount to scroll</param>
        public static void SimulateMouseWheel(this InputManager inputManager, float wheelDelta)
        {
            InputSourceSimulated.Instance.Mouse.SimulateMouseWheel(wheelDelta);
        }

        /// <summary>
        /// Simulates pointer down events, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="coords">the coordinates (0.0 -> 1.0) on the screen</param>
        public static void SimulatePointerDown(this InputManager inputManager, Vector2 coords)
        {
            InputSourceSimulated.Instance.Mouse.SimulatePointer(PointerEventType.Pressed, coords);
        }

        /// <summary>
        /// Simulates pointer move events, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="coords">the coordinates (0.0 -> 1.0) on the screen</param>
        public static void SimulatePointerMove(this InputManager inputManager, Vector2 coords)
        {
            InputSourceSimulated.Instance.Mouse.SimulatePointer(PointerEventType.Moved, coords);
        }

        /// <summary>
        /// Simulates pointer up events, requires <see cref="InputSourceSimulated"/> to be enabled
        /// </summary>
        /// <param name="inputManager">the InputManager</param>
        /// <param name="coords">the coordinates (0.0 -> 1.0) on the screen</param>
        public static void SimulatePointerUp(this InputManager inputManager, Vector2 coords)
        {
            InputSourceSimulated.Instance.Mouse.SimulatePointer(PointerEventType.Released, coords);
        }
    }
}