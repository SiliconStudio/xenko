// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A gesture that acts as an axis, having a positive or negative float value
    /// </summary>
    public interface IAxisGesture : IInputGesture
    {
        /// <summary>
        /// The axis state of this gesture
        /// </summary>
        float Axis { get; }

        /// <summary>
        /// If <c>true</c>, axis input should be scaled with delta time, otherwise this is an absolute movement (mouse wheel for example)
        /// </summary>
        bool IsRelative { get; }
    }
}