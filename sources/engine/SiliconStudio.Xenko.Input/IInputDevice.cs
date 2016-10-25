// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    public interface IInputDevice : IDisposable
    {
        /// <summary>
        /// Name for this device
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Unique Id for this device
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// Device priority, larger means higher priority (usefull for selecting primary pointer devices)
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Updates the input device, all events sent from this input device must be sent from the Update function. Input devices are always updated after their respective input source
        /// </summary>
        void Update();
    }
}