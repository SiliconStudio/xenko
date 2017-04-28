// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad point of view controller
    /// </summary>
    public class PovControllerInfo : GameControllerObjectInfo
    {
        public override string ToString()
        {
            return $"GameController Pov Controller {{{Name}}}";
        }
    }
}