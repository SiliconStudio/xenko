// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a gamepad axis
    /// </summary>
    public class GamePadAxisEvent : InputEvent
    {
        /// <summary>
        /// Index of the axis
        /// </summary>
        public int Index;

        /// <summary>
        /// The new value of the axis
        /// </summary>
        public float Value;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGamePadDevice GamePad => Device as IGamePadDevice;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index} ({GamePad.AxisInfos[Index].Name}), {nameof(Value)}: {Value}, {nameof(GamePad)}: {GamePad.DeviceName}";
        }
    }
}