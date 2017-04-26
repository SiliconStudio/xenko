// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for input sources, implements common parts of the <see cref="IInputSource"/> interface and keeps track of registered devices through <see cref="RegisterDevice"/> and <see cref="UnregisterDevice"/>
    /// </summary>
    public abstract class InputSourceBase : IInputSource
    {       
        public TrackingDictionary<Guid, IInputDevice> InputDevices { get; } = new TrackingDictionary<Guid, IInputDevice>();

        public abstract void Initialize(InputManager inputManager);

        public virtual void Update()
        {
        }
        
        public virtual void Pause()
        {
        }
        
        public virtual void Resume()
        {
        }
        
        public virtual void Scan()
        {
        }

        /// <summary>
        /// Unregisters all devices registered with <see cref="RegisterDevice"/> which have not been unregistered yet
        /// </summary>
        public virtual void Dispose()
        {
            // Remove all devices, done by clearing the tracking dictionary
            InputDevices.Clear();
        }

        /// <summary>
        /// Adds the device to the list <see cref="InputDevices"/>
        /// </summary>
        /// <param name="device">The device</param>
        protected void RegisterDevice(IInputDevice device)
        {
            if (InputDevices.ContainsKey(device.Id))
                throw new InvalidOperationException($"Input device with Id {device.Id} already registered");

            InputDevices.Add(device.Id, device);
        }

        /// <summary>
        /// CRemoves the device from the list <see cref="InputDevices"/>
        /// </summary>
        /// <param name="device">The device</param>
        protected void UnregisterDevice(IInputDevice device)
        {
            if (!InputDevices.ContainsKey(device.Id))
                throw new InvalidOperationException($"Input device with Id {device.Id} was not registered");

            InputDevices.Remove(device.Id);
        }
    }
}