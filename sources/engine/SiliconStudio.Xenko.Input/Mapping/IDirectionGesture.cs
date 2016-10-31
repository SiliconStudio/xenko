// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A gesture that acts as a direction, represented as a 2D vector
    /// </summary>
    public interface IDirectionGesture : IInputGesture
    {
        /// <summary>
        /// The direction state of this gesture
        /// </summary>
        Vector2 Direction { get; }
    }
}