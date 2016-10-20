// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An abstraction for a platform specific mechanism that provides input in the form of one of multiple <see cref="IInputDevice"/>(s)
    /// </summary>
    public interface IInputSource : IDisposable
    {
        /// <summary>
        /// Initializes the input source
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> initializing this source</param>
        void Initialize(InputManager inputManager);

        /// <summary>
        /// Update the input source
        /// </summary>
        void Update();

        /// <summary>
        /// Raised when an input device is added by this source
        /// </summary>
        EventHandler<IInputDevice> OnInputDeviceAdded { get; set; }

        /// <summary>
        /// Raised when an input device is removed by this source
        /// </summary>
        EventHandler<IInputDevice> OnInputDeviceRemoved { get; set; }

        /// <summary>
        /// All the input devices currently proviced by this source
        /// </summary>
        IReadOnlyList<IInputDevice> InputDevices { get; }
    }
}