// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in game controller button state
    /// </summary>
    public class GameControllerButtonEvent : ButtonEvent
    {
        /// <summary>
        /// The index of the button
        /// </summary>
        public int Index;

        /// <summary>
        /// The game controller that sent this event
        /// </summary>
        public IGameControllerDevice GameController => (IGameControllerDevice)Device;

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index} ({GameController.ButtonInfos[Index].Name}), {nameof(State)}: {State}, {nameof(GameController)}: {GameController.Name}";
        }
    }
}