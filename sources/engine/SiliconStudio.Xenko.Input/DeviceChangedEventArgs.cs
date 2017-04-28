// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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