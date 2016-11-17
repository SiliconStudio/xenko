// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event used when a device was changed
    /// </summary>
    public class DeviceChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The input source this device belongs to
        /// </summary>
        public IInputSource Source;

        /// <summary>
        /// The device that changed
        /// </summary>
        public IInputDevice Device;

        /// <summary>
        /// The type of change that happened
        /// </summary>
        public DeviceChangedEventType Type;
    }
}