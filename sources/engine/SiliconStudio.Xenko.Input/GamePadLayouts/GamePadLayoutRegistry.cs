// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Keeps track of <see cref="GamePadLayout"/>s defined in all assemblies
    /// </summary>
    public static class GamePadLayoutRegistry
    {
        public static IReadOnlyList<GamePadLayout> Layouts => layouts;
        private static List<GamePadLayout> layouts = new List<GamePadLayout>();
        private static readonly InstantiatableTypeBasedRegistry<GamePadLayout> typeRegistry = new InstantiatableTypeBasedRegistry<GamePadLayout>();

        static GamePadLayoutRegistry()
        {
            typeRegistry.Updated += (sender, args) => Update();
        }

        /// <summary>
        /// Update the registry with all found type
        /// </summary>
        public static void Update()
        {
            layouts.Clear();
            layouts = typeRegistry.CreateAllInstances().ToList();
        }

        /// <summary>
        /// Finds a layout matching the given gamepad
        /// </summary>
        /// <param name="device">The device to find a layout for</param>
        /// <returns>The gamepad layout that was found, or null if none was found</returns>
        public static GamePadLayout FindLayout(IGamePadDevice device)
        {
            foreach (var layout in layouts)
            {
                if (layout.MatchDevice(device))
                    return layout;
            }
            return null;
        }
    }
}