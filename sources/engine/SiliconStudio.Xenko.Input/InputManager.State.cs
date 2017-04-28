// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Class that keeps track of the the global input state of all devices
    /// </summary>
    public partial class InputManager : IInputEventListener<KeyEvent>, 
        IInputEventListener<PointerEvent>, 
        IInputEventListener<MouseButtonEvent>, 
        IInputEventListener<MouseWheelEvent>,
        IInputEventListener<GamePadButtonEvent>
    {
        /// <summary>
        /// The keys that are down
        /// </summary>
        public readonly List<Keys> DownKeys = new List<Keys>();

        /// <summary>
        /// The keys that have been pressed since the last frame
        /// </summary>
        public readonly List<Keys> PressedKeys = new List<Keys>();

        /// <summary>
        /// The keys that have been released since the last frame
        /// </summary>
        public readonly List<Keys> ReleasedKeys = new List<Keys>();

        /// <summary>
        /// The mouse buttons that are down
        /// </summary>
        public readonly List<MouseButton> DownButtons = new List<MouseButton>();

        /// <summary>
        /// The mouse buttons that have been pressed since the last frame
        /// </summary>
        public readonly List<MouseButton> PressedButtons = new List<MouseButton>();

        /// <summary>
        /// The mouse buttons that have been released since the last frame
        /// </summary>
        public readonly List<MouseButton> ReleasedButtons = new List<MouseButton>();

        /// <summary>
        /// Pointer events that happened since the last frame
        /// </summary>
        public readonly List<PointerEvent> PointerEvents = new List<PointerEvent>();

        /// <summary>
        /// Key events that happened since the last frame
        /// </summary>
        public readonly List<KeyEvent> KeyEvents = new List<KeyEvent>();

        /// <summary>
        /// Gamepad button press events that happened since the last frame
        /// </summary>
        public readonly List<GamePadButtonEvent> PressedGamePadButtonEvents = new List<GamePadButtonEvent>();

        /// <summary>
        /// Gamepad button release events that happened since the last frame
        /// </summary>
        public readonly List<GamePadButtonEvent> ReleasedGamePadButtonEvents = new List<GamePadButtonEvent>();

        /// <summary>
        /// Mouse delta in normalized (0,1) coordinates
        /// </summary>
        public Vector2 MouseDelta { get; private set; }

        /// <summary>
        /// Mouse movement in device coordinates
        /// </summary>
        public Vector2 AbsoluteMouseDelta { get; private set; }

        /// <summary>
        /// Normalized mouse position
        /// </summary>
        private Vector2 mousePosition;

        /// <summary>
        /// Gets the delta value of the mouse wheel button since last frame.
        /// </summary>
        public float MouseWheelDelta { get; private set; }

        /// <summary>
        /// Device that is responsible for setting the current <see cref="MouseDelta"/> and <see cref="MousePosition"/>
        /// </summary>
        public IPointerDevice LastPointerDevice { get; private set; }

        /// <summary>
        /// Determines whether one or more keys are down
        /// </summary>
        /// <returns><c>true</c> if one or more keys are down; otherwise, <c>false</c>.</returns>
        public bool HasDownKeys => DownKeys.Count > 0;

        /// <summary>
        /// Determines whether one or more keys are released
        /// </summary>
        /// <returns><c>true</c> if one or more keys are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedKeys => ReleasedKeys.Count > 0;
        
        /// <summary>
        /// Determines whether one or more keys are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more keys are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedKeys => PressedKeys.Count > 0;

        /// <summary>
        /// Determines whether one or more of the mouse buttons are down
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are down; otherwise, <c>false</c>.</returns>
        public bool HasDownMouseButtons => DownButtons.Count > 0;

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedMouseButtons => ReleasedButtons.Count > 0;

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedMouseButtons => PressedButtons.Count > 0;

        /// <summary>
        /// Resets the state before updating
        /// </summary>
        public void ResetGlobalInputState()
        {
            // Reset convenience states
            PressedKeys.Clear();
            ReleasedKeys.Clear();
            PressedButtons.Clear();
            ReleasedButtons.Clear();
            KeyEvents.Clear();
            PointerEvents.Clear();
            PressedGamePadButtonEvents.Clear();
            ReleasedGamePadButtonEvents.Clear();
            MouseWheelDelta = 0;
            MouseDelta = Vector2.Zero;
            AbsoluteMouseDelta = Vector2.Zero;
        }

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Down)
            {
                if (inputEvent.RepeatCount == 0)
                    DownKeys.Add(inputEvent.Key);
                PressedKeys.Add(inputEvent.Key);
            }
            else
            {
                DownKeys.Remove(inputEvent.Key);
                ReleasedKeys.Add(inputEvent.Key);
            }
            KeyEvents.Add(inputEvent);
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            PointerEvents.Add(inputEvent);

            // Update position and delta from whatever device sends position updates
            LastPointerDevice = inputEvent.Pointer;

            if (inputEvent.Device is IMouseDevice)
            {
                mousePosition = inputEvent.Position;

                // Add deltas together, so nothing gets lost if a down events gets sent after a move event with the actual delta
                MouseDelta += inputEvent.DeltaPosition;
                AbsoluteMouseDelta += inputEvent.DeltaPosition * LastPointerDevice.SurfaceSize;
            }
        }

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Down)
            {
                DownButtons.Add(inputEvent.Button);
                PressedButtons.Add(inputEvent.Button);
            }
            else
            {
                DownButtons.Remove(inputEvent.Button);
                ReleasedButtons.Add(inputEvent.Button);
            }
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            if (Math.Abs(inputEvent.WheelDelta) > Math.Abs(MouseWheelDelta))
            {
                MouseWheelDelta = inputEvent.WheelDelta;
            }
        }

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Down)
            {
                PressedGamePadButtonEvents.Add(inputEvent);
            }
            else
            {
                ReleasedGamePadButtonEvents.Add(inputEvent);
            }
        }

        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsKeyDown(Keys key)
        {
            return DownKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        public bool IsKeyPressed(Keys key)
        {
            return PressedKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        public bool IsKeyReleased(Keys key)
        {
            return ReleasedKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return DownButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return PressedButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return ReleasedButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified game pad button is pressed since the previous update.
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonPressed(IGamePadDevice device, GamePadButton button)
        {
            return PressedGamePadButtonEvents.Any(x => x.Device == device && (x.Button & button) == button);
        }

        /// <summary>
        /// Determines whether the specified game pad button is released since the previous update.
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonReleased(IGamePadDevice device, GamePadButton button)
        {
            return ReleasedGamePadButtonEvents.Any(x => x.Device == device && (x.Button & button) == button);
        }

        /// <summary>
        /// Determines whether the specified game pad button is being pressed down.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonDown(int gamePadIndex, GamePadButton button)
        {
            return (GetGamePad(gamePadIndex).State.Buttons & button) != 0;
        }

        /// <summary>
        /// Determines whether the specified game pad button is pressed since the previous update.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonPressed(int gamePadIndex, GamePadButton button)
        {
            var device = GetGamePad(gamePadIndex);
            if (device == null)
                return false;

            return IsPadButtonPressed(device, button);
        }
        
        /// <summary>
        /// Determines whether the specified game pad button is pressed since the previous update.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonReleased(int gamePadIndex, GamePadButton button)
        {
            var device = GetGamePad(gamePadIndex);
            if (device == null)
                return false;

            return IsPadButtonReleased(device, button);
        }

        /// <summary>
        /// Determines whether the specified game pad button is released since the previous update.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool GetGamePad(int gamePadIndex, GamePadButton button)
        {
            var device = GetGamePad(gamePadIndex);
            if (device == null)
                return false;

            return IsPadButtonReleased(device, button);
        }
    }
}