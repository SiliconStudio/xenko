// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for input sources, implements common parts of the <see cref="IInputSource"/> interface and keeps track of registered devices through <see cref="RegisterDevice"/> and <see cref="UnregisterDevice"/>
    /// </summary>
    public abstract class InputSourceBase : IInputSource
    {
        protected List<IInputDevice> RegisteredInputDevices = new List<IInputDevice>();

        /// <summary>
        /// Unregisters all devices registered with <see cref="RegisterDevice"/> which have not been unregistered yet
        /// </summary>
        public virtual void Dispose()
        {
            // Unregister all devices
            foreach (var device in RegisteredInputDevices)
            {
                InputDeviceRemoved?.Invoke(this, device);
            }
            RegisteredInputDevices.Clear();
        }
        
        public IReadOnlyList<IInputDevice> InputDevices => RegisteredInputDevices;
        
        public event EventHandler<IInputDevice> InputDeviceAdded;
        public event EventHandler<IInputDevice> InputDeviceRemoved;
        
        public abstract void Initialize(InputManager inputManager);
        
        public abstract bool IsEnabled(GameContext gameContext);
        
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
        /// Calls <see cref="InputDeviceAdded"/> and adds the device to the list <see cref="InputDevices"/>
        /// </summary>
        /// <param name="device">The device</param>
        protected void RegisterDevice(IInputDevice device)
        {
            if (RegisteredInputDevices.Contains(device))
                throw new InvalidOperationException("Tried to use RegisterDevice on an input device twice");

            InputDeviceAdded?.Invoke(this, device);
            RegisteredInputDevices.Add(device);
        }

        /// <summary>
        /// Calls <see cref="InputDeviceRemoved"/> and removes the device from the list <see cref="InputDevices"/>
        /// </summary>
        /// <param name="device">The device</param>
        protected void UnregisterDevice(IInputDevice device)
        {
            if (!RegisteredInputDevices.Contains(device))
                throw new InvalidOperationException("Tried to use UnregisterDevice on an unregistered input device");

            InputDeviceRemoved?.Invoke(this, device);
            RegisteredInputDevices.Remove(device);
        }
    }
}