// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in gamepad button state
    /// </summary>
    public class GamePadButtonEvent : ButtonEvent
    {
        /// <summary>
        /// The gamepad button identifier
        /// </summary>
        public GamePadButton Button;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGamePadDevice GamePad => (IGamePadDevice)Device;

        public override string ToString()
        {
            return $"{nameof(Button)}: {Button}, {nameof(State)}: {State}, {nameof(GamePad)}: {GamePad.Name}";
        }
    }
}