// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// Interface for input management system, including keyboard, mouse, gamepads and touch.
    /// </summary>
    public interface IInputManager : IGameSystemBase, IUpdateable
    {
        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        Vector2 MousePosition { get; }

        /// <summary>
        /// Gets a value indicating whether the keyboard is available.
        /// </summary>
        /// <value><c>true</c> if the keyboard is available; otherwise, <c>false</c>.</value>
        bool HasKeyboard { get; }

        /// <summary>
        /// Gets the list of keys being pressed down.
        /// </summary>
        /// <value>The key pressed.</value>
        List<Keys> KeyDown { get; }

        /// <summary>
        /// Gets the list of key events (pressed or released) since the previous update.
        /// </summary>
        /// <value>The key events.</value>
        List<KeyEvent> KeyEvents { get; }

        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        bool IsKeyDown(Keys key);

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        bool IsKeyReleased(Keys key);

        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        bool IsKeyPressed(Keys key);

        /// <summary>
        /// Gets a value indicating whether the mouse is available.
        /// </summary>
        /// <value><c>true</c> if the mouse is available; otherwise, <c>false</c>.</value>
        bool HasMouse { get; }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        bool IsMouseButtonDown(MouseButton mouseButton);

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        bool IsMouseButtonPressed(MouseButton mouseButton);

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        bool IsMouseButtonReleased(MouseButton mouseButton);

        /// <summary>
        /// Determines whether one or more of the mouse buttons are down
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are down; otherwise, <c>false</c>.</returns>
        bool HasDownMouseButtons();

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        bool HasPressedMouseButtons();

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        bool HasReleasedMouseButtons();

        /// <summary>
        /// Gets the delta value of the mouse wheel button since last frame.
        /// </summary>
        float MouseWheelDelta { get; }

        /// <summary>
        /// Gets a value indicating whether pointer device is available.
        /// </summary>
        /// <value><c>true</c> if pointer devices are available; otherwise, <c>false</c>.</value>
        bool HasPointer { get; }

        /// <summary>
        /// Gets a collection of pointer events since the previous updates.
        /// </summary>
        /// <value>The pointer events.</value>
        List<PointerEvent> PointerEvents { get; }
        
        /// <summary>
        /// Gets a value indicating whether gamepads are available.
        /// </summary>
        /// <value><c>true</c> if gamepads are available; otherwise, <c>false</c>.</value>
        bool HasGamePad { get; }

        /// <summary>
        /// Gets the number of gamepad connected.
        /// </summary>
        /// <value>The number of gamepad connected.</value>
        int GamePadCount { get; }

        /// <summary>
        /// Gets the state of the specified gamepad.
        /// </summary>
        /// <param name="gamepadIndex">Index of the gamepad. -1 to return the first connected gamepad</param>
        /// <returns>The state of the gamepad.</returns>
        GamePadState GetGamePad(int gamepadIndex);

        /// <summary>
        /// Gets or sets the configuration for virtual buttons.
        /// </summary>
        /// <value>The current binding.</value>
        VirtualButtonConfigSet VirtualButtonConfigSet { get; set; }

        /// <summary>
        /// Gets a binding value for the specified name and the specified config extract from the current <see cref="VirtualButtonConfigSet"/>.
        /// </summary>
        /// <param name="configIndex">An index to a <see cref="VirtualButtonConfig"/> stored in the <see cref="VirtualButtonConfigSet"/></param>
        /// <param name="bindingName">Name of the binding.</param>
        /// <returns>The value of the binding.</returns>
        float GetVirtualButton(int configIndex, object bindingName);

        /// <summary>
        /// Rescans all input devices in order to query new device connected. See remarks.
        /// </summary>
        /// <remarks>
        /// This method could take several milliseconds and should be used at specific time in a game where performance is not crucial (pause, configuration screen...etc.)
        /// </remarks>
        void Scan();

        /// <summary>
        /// List of the gestures to recognize.
        /// </summary>
        /// <remarks>To detect a new gesture add its configuration to the list. 
        /// To stop detecting a gesture remove its configuration from the list. 
        /// To all gestures detection clear the list.
        /// Note that once added to the list the <see cref="GestureConfig"/>s are frozen by the system and cannot be modified anymore.</remarks>
        /// <seealso cref="GestureConfig"/>
        GestureConfigCollection ActivatedGestures { get; }
        
        /// <summary>
        /// Gets the collection of gesture events since the previous updates.
        /// </summary>
        /// <value>The gesture events.</value>
        List<GestureEvent> GestureEvents { get; }
        
        /// <summary>
        /// Gets or sets the value indicating if simultaneous multiple finger touches are enabled or not.
        /// If not enabled only the events of one finger at a time are triggered.
        /// </summary>
        bool MultiTouchEnabled { get; set; }
    }
}