// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad axis
    /// </summary>
    public class GamePadAxisInfo : GamePadObjectInfo
    {
        /// <summary>
        /// The type of axis
        /// </summary>
        public GamePadAxisType Type;

        public override string ToString()
        {
            return $"GamePad Axis {{{Name}}} [{Type}]";
        }
    }
}