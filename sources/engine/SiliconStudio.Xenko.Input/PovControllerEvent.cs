// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a game controller's point of view controller
    /// </summary>
    public class PovControllerEvent : InputEvent
    {
        /// <summary>
        /// The index of the pov controller
        /// </summary>
        public int Index;

        /// <summary>
        /// <c>true</c> if the controller is enabled, <c>false</c> if the controller is in a neutral position (disabled)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// The new value of the pov controller
        /// </summary>
        /// <remarks>Goes clockwise from 0 to 1 where 0 is up</remarks>
        public float Value;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGameControllerDevice GameController => (IGameControllerDevice)Device;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index} ({GameController.PovControllerInfos[Index].Name}), {nameof(Value)}: {Value}, {nameof(Enabled)}: {Enabled}, {nameof(GameController)}: {GameController.Name}";
        }
    }
}