// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a gamepad axis
    /// </summary>
    public class GameControllerAxisEvent : InputEvent
    {
        /// <summary>
        /// Index of the axis
        /// </summary>
        public int Index;

        /// <summary>
        /// The common gamepad axis, if mapped using <see cref="GamePadLayout"/>
        /// </summary>
        public GamePadAxis Axis;

        /// <summary>
        /// The new value of the axis
        /// </summary>
        public float Value;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGameControllerDevice GameController => Device as IGameControllerDevice;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {nameof(Axis)}: {Axis}, {nameof(Value)}: {Value}, {nameof(GameController)}: {GameController.DeviceName}";
        }
    }
}