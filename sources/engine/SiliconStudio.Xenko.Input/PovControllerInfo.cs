// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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