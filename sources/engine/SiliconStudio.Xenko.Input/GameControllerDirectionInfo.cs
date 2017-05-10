// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad direction input
    /// </summary>
    public class GameControllerDirectionInfo : GameControllerObjectInfo
    {
        public override string ToString()
        {
            return $"GameController Direction {{{Name}}}";
        }
    }
}