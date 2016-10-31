// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A gesture that acts as a button, having a true/false state
    /// </summary>
    public interface IButtonGesture : IInputGesture
    {
        /// <summary>
        /// The button state of this gesture
        /// </summary>
        bool Button { get; }
    }
}