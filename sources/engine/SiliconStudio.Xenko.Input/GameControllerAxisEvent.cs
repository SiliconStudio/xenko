// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a game controller axis state
    /// </summary>
    public class GameControllerAxisEvent : AxisEvent
    {
        /// <summary>
        /// Index of the axis
        /// </summary>
        public int Index;

        /// <summary>
        /// The game controller that sent this event
        /// </summary>
        public IGameControllerDevice GameController => (IGameControllerDevice)Device;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index} ({GameController.AxisInfos[Index].Name}), {nameof(Value)}: {Value}, {nameof(GameController)}: {GameController.Name}";
        }
    }
}