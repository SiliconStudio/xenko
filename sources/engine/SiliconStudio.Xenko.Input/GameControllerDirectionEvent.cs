// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in game controller direction state
    /// </summary>
    public class GameControllerDirectionEvent : InputEvent
    {
        /// <summary>
        /// The index of the direction controller
        /// </summary>
        public int Index;

        /// <summary>
        /// The new direction
        /// </summary>
        public Direction Direction;

        /// <summary>
        /// The gamepad that sent this event
        /// </summary>
        public IGameControllerDevice GameController => (IGameControllerDevice)Device;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index} ({GameController.DirectionInfos[Index].Name}), {nameof(Direction)}: {Direction}, {nameof(GameController)}: {GameController.Name}";
        }
    }
}