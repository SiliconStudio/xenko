// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This interface is used for interacting with game controller devices.
    /// </summary>
    public interface IGameControllerDevice : IInputDevice
    {
        /// <summary>
        /// Product Id of the device
        /// </summary>
        Guid ProductId { get; }
        
        /// <summary>
        /// Information about the buttons on this game controller
        /// </summary>
        IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }

        /// <summary>
        /// Information about the axes on this game controller
        /// </summary>
        IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }

        /// <summary>
        /// Information about the direction controllers on this game controller 
        /// </summary>
        IReadOnlyList<GameControllerDirectionInfo> DirectionInfos { get; }

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
        /// Retrieves the state of a single point of direction controller
        /// </summary>
        /// <param name="index">The direction controller's index, as exposed in <see cref="DirectionInfos"/></param>
        /// <returns>The current state of the direction controller</returns>
        Direction GetDirection(int index);
    }
}