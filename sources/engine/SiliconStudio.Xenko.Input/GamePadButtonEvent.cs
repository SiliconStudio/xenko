// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in gamepad button state
    /// </summary>
    public class GamePadButtonEvent : ButtonEvent
    {
        /// <summary>
        /// The index of the button
        /// </summary>
        public int Index;

        /// <summary>
        /// The common gamepad button, if mapped using  <see cref="GamePadLayout"/>
        /// </summary>
        public GamePadButton Button;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGamePadDevice GamePad => Device as IGamePadDevice;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {nameof(Button)}: {Button}, {nameof(State)}: {State}, {nameof(GamePad)}: {GamePad.DeviceName}";
        }
    }
}