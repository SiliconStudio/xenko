// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This interface is used for interacting with gamepad devices.
    /// </summary>
    /// <remarks>If vibration is required, check if the object implements <see cref="IGamePadVibration"/></remarks>
    public interface IGamePadDevice : IInputDevice
    {
        /// <summary>
        /// The index of the GamePad
        /// </summary>
        int Index { get; }

        /// <summary>
        /// <c>true</c> if the GamePad is currently connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Information about the buttons on this GamePad
        /// </summary>
        IReadOnlyList<GamePadButtonInfo> ButtonInfos { get; }

        /// <summary>
        /// Information about the axes on this GamePad
        /// </summary>
        IReadOnlyList<GamePadAxisInfo> AxisInfos { get; }

        /// <summary>
        /// Information about the point of view controllers (dpad) on this GamePad 
        /// </summary>
        IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos { get; }

        /// <summary>
        /// Raised when this gamepad gets disconnected
        /// </summary>
        EventHandler OnDisconnect { get; set; }

        /// <summary>
        /// Raised when a button on the GamePad changes state
        /// </summary>
        EventHandler<GamePadButtonEvent> OnButton { get; set; }

        /// <summary>
        /// Raised when an axis on the GamePad changes state
        /// </summary>
        EventHandler<GamePadAxisEvent> OnAxisChanged { get; set; }

        /// <summary>
        /// Raised when a point of view controller on the GamePad changes state
        /// </summary>
        EventHandler<GamePadPovControllerEvent> OnPovControllerChanged { get; set; }

        /// <summary>
        /// Retrieves the button state of a single button
        /// </summary>
        /// <param name="index">The button's index, as exposed in <see cref="ButtonInfos"/></param>
        /// <returns><c>true</c> if the button is pressed, <c>false</c> otherwise</returns>
        bool GetButton(int index);

        /// <summary>
        /// Retrieves the state of a single axis
        /// </summary>
        /// <param name="index">The axis' index, as exposed in <see cref="AxisInfos"/></param>
        /// <returns>The value read directly from the axis</returns>
        float GetAxis(int index);

        /// <summary>
        /// Retrieves the state of a single point of view controller
        /// </summary>
        /// <param name="index">The pov controller's index, as exposed in <see cref="PovControllerInfos"/></param>
        /// <returns>The direction of the pov controller, starting from 0 being up, going clockwise</returns>
        float GetPovController(int index);

        /// <summary>
        /// Retrieves the state of a single point of view controller
        /// </summary>
        /// <param name="index">The pov controller's index, as exposed in <see cref="PovControllerInfos"/></param>
        /// <returns><c>true</c> if the pov controller is enabled, <c>false</c> otherwise</returns>
        bool GetPovControllerEnabled(int index);

        /// <summary>
        /// Tries to read the current state of the <see cref="IGamePadDevice"/> and writes it to <see cref="state"/> in the form of a generic gamepad
        /// </summary>
        /// <param name="state">The state to update with this GamePad</param>
        /// <returns><c>true</c> if the state was retrieved, <c>false</c> if the device does not provide a mapping to GamePadState</returns>
        bool GetGamePadState(ref GamePadState state);
    }
}