// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Class that keeps track of the the global input state of all devices
    /// </summary>
    public partial class InputManager : IInputEventListener<KeyEvent>, 
        IInputEventListener<PointerEvent>, 
        IInputEventListener<MouseButtonEvent>, 
        IInputEventListener<MouseWheelEvent>
    {
        private Vector2 mousePosition;
        
        private readonly HashSet<Keys> downKeys = new HashSet<Keys>();
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private readonly HashSet<Keys> releasedKeys = new HashSet<Keys>();
        private readonly HashSet<MouseButton> downButtons = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> pressedButtons = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> releasedButtons = new HashSet<MouseButton>();
        private readonly List<KeyEvent> keyEvents = new List<KeyEvent>();

        // TODO: This is left internal until the UI test have been upgraded to use the input simulation layer
        internal readonly List<PointerEvent> pointerEvents = new List<PointerEvent>();
        
        /// <summary>
        /// Mouse delta in normalized (0,1) coordinates
        /// </summary>
        public Vector2 MouseDelta { get; private set; }

        /// <summary>
        /// Mouse movement in device coordinates
        /// </summary>
        public Vector2 AbsoluteMouseDelta { get; private set; }
        
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
        public bool HasDownKeys => downKeys.Count > 0;

        /// <summary>
        /// Determines whether one or more keys are released
        /// </summary>
        /// <returns><c>true</c> if one or more keys are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedKeys => releasedKeys.Count > 0;
        
        /// <summary>
        /// Determines whether one or more keys are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more keys are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedKeys => pressedKeys.Count > 0;

        /// <summary>
        /// Determines whether one or more of the mouse buttons are down
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are down; otherwise, <c>false</c>.</returns>
        public bool HasDownMouseButtons => downButtons.Count > 0;

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedMouseButtons => releasedButtons.Count > 0;

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedMouseButtons => pressedButtons.Count > 0;

        /// <summary>
        /// The keys that are down
        /// </summary>
        public IReadOnlySet<Keys> DownKeys { get; }

        /// <summary>
        /// The keys that have been pressed since the last frame
        /// </summary>
        public IReadOnlySet<Keys> PressedKeys { get; }

        /// <summary>
        /// The keys that have been released since the last frame
        /// </summary>
        public IReadOnlySet<Keys> ReleasedKeys { get; }

        /// <summary>
        /// The mouse buttons that are down
        /// </summary>
        public IReadOnlySet<MouseButton> DownButtons { get; }

        /// <summary>
        /// The mouse buttons that have been pressed since the last frame
        /// </summary>
        public IReadOnlySet<MouseButton> PressedButtons { get; }

        /// <summary>
        /// The mouse buttons that have been released since the last frame
        /// </summary>
        public IReadOnlySet<MouseButton> ReleasedButtons { get; }

        /// <summary>
        /// Pointer events that happened since the last frame
        /// </summary>
        public IReadOnlyList<PointerEvent> PointerEvents => pointerEvents;

        /// <summary>
        /// Key events that happened since the last frame
        /// </summary>
        public IReadOnlyList<KeyEvent> KeyEvents => keyEvents;
        
        /// <summary>
        /// Resets the state before updating
        /// </summary>
        public void ResetGlobalInputState()
        {
            // Reset convenience states
            pressedKeys.Clear();
            releasedKeys.Clear();
            pressedButtons.Clear();
            releasedButtons.Clear();
            keyEvents.Clear();
            pointerEvents.Clear();
            MouseWheelDelta = 0;
            MouseDelta = Vector2.Zero;
            AbsoluteMouseDelta = Vector2.Zero;
        }

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.IsDown)
            {
                if (inputEvent.RepeatCount == 0)
                    downKeys.Add(inputEvent.Key);
                pressedKeys.Add(inputEvent.Key);
            }
            else
            {
                downKeys.Remove(inputEvent.Key);
                releasedKeys.Add(inputEvent.Key);
            }
            keyEvents.Add(inputEvent);
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            pointerEvents.Add(inputEvent);

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
            if (inputEvent.IsDown)
            {
                downButtons.Add(inputEvent.Button);
                pressedButtons.Add(inputEvent.Button);
            }
            else
            {
                downButtons.Remove(inputEvent.Button);
                releasedButtons.Add(inputEvent.Button);
            }
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            if (Math.Abs(inputEvent.WheelDelta) > Math.Abs(MouseWheelDelta))
            {
                MouseWheelDelta = inputEvent.WheelDelta;
            }
        }

        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsKeyDown(Keys key)
        {
            return downKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        public bool IsKeyPressed(Keys key)
        {
            return pressedKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        public bool IsKeyReleased(Keys key)
        {
            return releasedKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return downButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return pressedButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return releasedButtons.Contains(mouseButton);
        }
    }
}