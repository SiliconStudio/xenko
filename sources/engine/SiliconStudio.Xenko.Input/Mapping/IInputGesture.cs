// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An interface for an object that monitors a physical device or other input gesture and generates input for and <see cref="InputAction"/>
    /// </summary>
    public interface IInputGesture
    {
        /// <summary>
        /// Allows the gesture to reset states, e.g. putting delta input values back on zero
        /// </summary>
        /// <param name="elapsedTime"></param>
        void Reset(TimeSpan elapsedTime);
    }
}