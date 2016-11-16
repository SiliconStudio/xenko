// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad axis
    /// </summary>
    public class GameControllerAxisInfo : GameControllerObjectInfo
    {
        /// <summary>
        /// <c>true</c> for bidirectional axes that go from -1 to 1
        /// </summary>
        public bool IsBiDirectional = true;

        public override string ToString()
        {
            return $"GameController Axis {{{Name}}}, {nameof(IsBiDirectional)}: {IsBiDirectional}";
        }
    }
}