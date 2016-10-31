// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Represents functionality specific to mouse input such as buttons, wheels, mouse locking and setting cursor position
    /// </summary>
    public interface IMouseDevice : IPointerDevice
    {
        /// <summary>
        /// Gets or sets if the mouse is locked to the screen
        /// </summary>
        bool IsMousePositionLocked { get; }

        /// <summary>
        /// Locks the mouse position to the screen
        /// </summary>
        /// <param name="forceCenter">Force the mouse position to the center of the screen</param>
        void LockMousePosition(bool forceCenter = false);

        /// <summary>
        /// Unlocks the mouse position if it was locked
        /// </summary>
        void UnlockMousePosition();

        /// <summary>
        /// Determines whether the specified button is being pressed down
        /// </summary>
        /// <param name="button">The button</param>
        /// <returns><c>true</c> if the specified button is being pressed down; otherwise, <c>false</c>.</returns>
        bool IsMouseButtonDown(MouseButton button);

        /// <summary>
        /// Attempts to set the pointer position, this only makes sense for mouse pointers
        /// </summary>
        /// <param name="normalizedPosition"></param>
        void SetMousePosition(Vector2 normalizedPosition);
    }
}