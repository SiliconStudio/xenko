// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about a gamepad point of view controller
    /// </summary>
    public class GamePadPovControllerInfo : GamePadObjectInfo
    {
        public override string ToString()
        {
            return $"GamePad Pov Controller {{{Name}}}";
        }
    }
}