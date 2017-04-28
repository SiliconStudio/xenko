// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Generates certain events based on input events it received
    /// </summary>
    public interface IInputGesture
    {
        /// <summary>
        /// Allows the gesture to reset states, e.g. putting delta input values back on zero
        /// </summary>
        /// <param name="elapsedTime"></param>
        void PreUpdate(TimeSpan elapsedTime);

        /// <summary>
        /// Allows the gesture to update states and raise events based on events received
        /// </summary>
        /// <param name="elapsedTime"></param>
        void Update(TimeSpan elapsedTime);
    }
}