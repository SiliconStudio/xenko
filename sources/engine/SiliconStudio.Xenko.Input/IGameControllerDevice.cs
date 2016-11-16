// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This interface is used for interacting with game controller devices.
    /// </summary>
    /// <remarks>If vibration is required, check if the object implements <see cref="IGamePadVibration"/></remarks>
    public interface IGameControllerDevice : IInputDevice
    {
        /// <summary>
        /// Product Id of the device
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// The index of the game controller
        /// </summary>
        int Index { get; }

        /// <summary>
        /// <c>true</c> if the game controller is currently connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The state of the gamepad, if mapped with <see cref="GamePadLayout"/>
        /// </summary>
        GamePadState State { get; }

        /// <summary>
        /// Information about the buttons on this game controller
        /// </summary>
        IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }

        /// <summary>
        /// Information about the axes on this game controller
        /// </summary>
        IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }

        /// <summary>
        /// Information about the point of view controllers on this game controller 
        /// </summary>
        IReadOnlyList<GameControllerPovControllerInfo> PovControllerInfos { get; }

        /// <summary>
        /// Raised when this gamepad gets disconnected
        /// </summary>
        event EventHandler Disconnected;

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
    }
}