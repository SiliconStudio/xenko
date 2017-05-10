// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Keeps track of <see cref="GamePadLayout"/>
    /// </summary>
    public static class GamePadLayouts
    {
        private static readonly List<GamePadLayout> layouts = new List<GamePadLayout>();

        static GamePadLayouts()
        {
            // XInput device layout for any plaform that does not support xinput directly
            AddLayout(new GamePadLayoutXInput());
        }

        /// <summary>
        /// Adds a new layout that cane be used for mapping gamepads to <see cref="GamePadState"/>
        /// </summary>
        /// <param name="layout">The layout to add</param>
        public static void AddLayout(GamePadLayout layout)
        {
            lock (layouts)
            {
                if (!layouts.Contains(layout))
                {
                    layouts.Add(layout);
                }
            }
        }

        /// <summary>
        /// Finds a layout matching the given gamepad
        /// </summary>
        /// <param name="source">The source that the <paramref name="device"/> came from</param>
        /// <param name="device">The device to find a layout for</param>
        /// <returns>The gamepad layout that was found, or null if none was found</returns>
        public static GamePadLayout FindLayout(IInputSource source, IGameControllerDevice device)
        {
            lock (layouts)
            {
                foreach (var layout in layouts)
                {
                    if (layout.MatchDevice(source, device))
                        return layout;
                }
                return null;
            }
        }
    }
}