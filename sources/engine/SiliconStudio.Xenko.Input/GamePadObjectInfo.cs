// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides information about an object exposed by a gamepad
    /// </summary>
    public class GamePadObjectInfo
    {
        /// <summary>
        /// The index of this type of object, as reported by the device
        /// </summary>
        /// <remarks>Each category (button,axis,pov) has it's own index counter</remarks>
        public int Index;

        /// <summary>
        /// The name of the object, reported by the device
        /// </summary>
        public string Name;

        public override string ToString()
        {
            return $"GamePad Object {{{Name}}}";
        }
    }
}