// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad axis
    /// </summary>
    public class GameControllerAxisInfo : GameControllerObjectInfo
    {
        public override string ToString()
        {
            return $"GameController Axis {{{Name}}}";
        }
    }
}