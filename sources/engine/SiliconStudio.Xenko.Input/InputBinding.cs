// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows.Input;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Common data for all input bindings
    /// </summary>
    public class InputBinding
    {
        /// <summary>
        /// Sensitivity (scales the output)
        /// </summary>
        public readonly float Sensitivity = 0.1f;

        /// <summary>
        /// True for relative input devices, such as mouse
        /// </summary>
        public readonly bool IsRelative;

        /// <summary>
        /// The underlying virtual button
        /// </summary>
        public readonly IVirtualButton VirtualButton;

        public InputBinding(IVirtualButton virtualButton, float sensitivity = 1.0f)
        {
            this.VirtualButton = virtualButton;

            // TODO: only axes on gamepads should be non relative, so that their value get's properly scaled by deltaTime
            this.IsRelative = virtualButton.GetType() != typeof(VirtualButton.GamePad);
        }
        public float GetValue(InputManager inputManager)
        {
            return VirtualButton.GetValue(inputManager);
        }
    }
}