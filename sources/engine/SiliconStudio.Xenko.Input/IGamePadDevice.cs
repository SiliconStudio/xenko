// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A gamepad is a game controller that has a fixed button mapping, stored in <see cref="State"/>
    /// </summary>
    public interface IGamePadDevice : IInputDevice
    {
        /// <summary>
        /// Product Id of the device
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// The state of the gamepad
        /// </summary>
        GamePadState State { get; }
        
        /// <summary>
        /// The index of the gamepad assigned by the input manager
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Raised if the index assigned to this gamepad changed
        /// </summary>
        event EventHandler<GamePadIndexChangedEventArgs> IndexChanged;

        /// <summary>
        /// Sets 4 vibration motors one the device or approximates the effect if less are supported
        /// </summary>
        /// <param name="smallLeft">The small left side motor</param>
        /// <param name="smallRight">The small right side motor</param>
        /// <param name="largeLeft">The large left side motor</param>
        /// <param name="largeRight">The large right side motor</param>
        /// <remarks>Values range from 0 to 1, where 0 is off and 1 is maximum vibration</remarks>
        /// <remarks>Currently vibration is only supported on windows for XInput devices and UWP supported gamepads</remarks>
        void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight);
    }
}