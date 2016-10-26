// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad button
    /// </summary>
    public class GamePadButtonInfo : GamePadObjectInfo
    {
        /// <summary>
        /// The type of button
        /// </summary>
        public GamePadButtonType Type;

        public override string ToString()
        {
            return $"GamePad Button {{{Name}}} [{Type}]";
        }
    }
}