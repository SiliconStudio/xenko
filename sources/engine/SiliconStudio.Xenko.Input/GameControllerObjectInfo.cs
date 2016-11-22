// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about an object exposed by a gamepad
    /// </summary>
    public class GameControllerObjectInfo
    {
        /// <summary>
        /// The name of the object, reported by the device
        /// </summary>
        public string Name;

        public override string ToString()
        {
            return $"GameController Object {{{Name}}}";
        }
    }
}