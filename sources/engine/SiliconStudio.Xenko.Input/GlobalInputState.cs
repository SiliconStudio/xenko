// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Class that keeps track of the the global input state of all devices
    /// </summary>
    internal class GlobalInputState : IInputEventListener<KeyEvent>, 
        IInputEventListener<PointerEvent>, 
        IInputEventListener<MouseButtonEvent>, 
        IInputEventListener<MouseWheelEvent>,
        IInputEventListener<GamePadButtonEvent>
    {
        public readonly HashSet<Keys> DownKeysSet = new HashSet<Keys>();
        public readonly HashSet<Keys> PressedKeysSet = new HashSet<Keys>();
        public readonly HashSet<Keys> ReleasedKeysSet = new HashSet<Keys>();

        public readonly HashSet<MouseButton> DownButtonSet = new HashSet<MouseButton>();
        public readonly HashSet<MouseButton> PressedButtonsSet = new HashSet<MouseButton>();
        public readonly HashSet<MouseButton> ReleasedButtonsSet = new HashSet<MouseButton>();

        public readonly List<PointerEvent> PointerEvents = new List<PointerEvent>();
        public readonly List<KeyEvent> KeyEvents = new List<KeyEvent>();

        public readonly List<GamePadButtonEvent> PressedGamePadButtonEvents = new List<GamePadButtonEvent>();
        public readonly List<GamePadButtonEvent> ReleasedGamePadButtonEvents = new List<GamePadButtonEvent>();

        /// <summary>
        /// Mouse delta in normalized (0,1) coordinates
        /// </summary>
        public Vector2 MouseDelta { get; private set; }

        /// <summary>
        /// Mouse movement in device coordinates
        /// </summary>
        public Vector2 AbsoluteMouseDelta
        {
            get
            {
                if (LastPointerDevice != null)
                    return MouseDelta * LastPointerDevice.SurfaceSize;
                return MouseDelta;
            }
        }

        /// <summary>
        /// Normalized mouse position
        /// </summary>
        public Vector2 MousePosition { get; private set; }

        /// <summary>
        /// Delta of the mouse wheel
        /// </summary>
        public float MouseWheelDelta { get; private set; }

        /// <summary>
        /// Device that is responsible for setting the current <see cref="MouseDelta"/> and <see cref="MousePosition"/>
        /// </summary>
        public IPointerDevice LastPointerDevice { get; private set; }

        /// <summary>
        /// Resets the state before updating
        /// </summary>
        public void Reset()
        {
            // Reset convenience states
            PressedKeysSet.Clear();
            ReleasedKeysSet.Clear();
            PressedButtonsSet.Clear();
            ReleasedButtonsSet.Clear();
            PointerEvents.Clear();
            KeyEvents.Clear();
            PressedGamePadButtonEvents.Clear();
            ReleasedGamePadButtonEvents.Clear();
            MouseWheelDelta = 0;
            MouseDelta = Vector2.Zero;
        }

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
            {
                DownKeysSet.Add(inputEvent.Key);
                PressedKeysSet.Add(inputEvent.Key);
            }
            else
            {
                DownKeysSet.Remove(inputEvent.Key);
                ReleasedKeysSet.Add(inputEvent.Key);
            }
            KeyEvents.Add(inputEvent);
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            PointerEvents.Add(inputEvent);

            // Update position and delta from whatever device sends position updates
            MousePosition = inputEvent.Position;
            MouseDelta = inputEvent.DeltaPosition;
            LastPointerDevice = inputEvent.Pointer;
        }

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
            {
                DownButtonSet.Add(inputEvent.Button);
                PressedButtonsSet.Add(inputEvent.Button);
            }
            else
            {
                DownButtonSet.Remove(inputEvent.Button);
                ReleasedButtonsSet.Add(inputEvent.Button);
            }
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            if (Math.Abs(inputEvent.WheelDelta) > Math.Abs(MouseWheelDelta))
                MouseWheelDelta = inputEvent.WheelDelta;
        }

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if(inputEvent.State == ButtonState.Pressed)
                PressedGamePadButtonEvents.Add(inputEvent);
            else
                ReleasedGamePadButtonEvents.Add(inputEvent);
        }

        #region InputManager Compatibility

        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsKeyDown(Keys key)
        {
            return DownKeysSet.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        public bool IsKeyPressed(Keys key)
        {
            return PressedKeysSet.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        public bool IsKeyReleased(Keys key)
        {
            return ReleasedKeysSet.Contains(key);
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are down
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are down; otherwise, <c>false</c>.</returns>
        public bool HasDownMouseButtons()
        {
            return DownButtonSet.Count > 0;
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedMouseButtons()
        {
            return ReleasedButtonsSet.Count > 0;
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedMouseButtons()
        {
            return PressedButtonsSet.Count > 0;
        }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return DownButtonSet.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return PressedButtonsSet.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return ReleasedButtonsSet.Contains(mouseButton);
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

        #endregion
    }
}