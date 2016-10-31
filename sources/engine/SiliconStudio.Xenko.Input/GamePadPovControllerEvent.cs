// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a gamepad point of view controller
    /// </summary>
    public class GamePadPovControllerEvent : InputEvent
    {
        /// <summary>
        /// Creates a new pov controller/direction event
        /// </summary>
        /// <param name="device">The device that produces this event</param>
        public GamePadPovControllerEvent(IGamePadDevice device) : base(device)
        {
        }

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
        /// <remarks>Goes from 0 to 1 where 0 is up</remarks>
        public float Value;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGamePadDevice GamePad => Device as IGamePadDevice;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index} ({GamePad.PovControllerInfos[Index].Name}), {nameof(Value)}: {Value}, {nameof(Enabled)}: {Enabled}, {nameof(GamePad)}: {GamePad.DeviceName}";
        }
    }
}