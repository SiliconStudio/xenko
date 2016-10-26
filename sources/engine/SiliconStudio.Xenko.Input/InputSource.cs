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
        protected List<IInputDevice> registeredInputDevices = new List<IInputDevice>();

        /// <summary>
        /// Unregisters all devices registered with <see cref="RegisterDevice"/> which have not been unregistered yet
        /// </summary>
        public virtual void Dispose()
        {
            // Unregister all devices
            foreach (var device in registeredInputDevices)
            {
                OnInputDeviceRemoved?.Invoke(this, device);
            }
            registeredInputDevices.Clear();
        }

        /// <inheritdoc />
        public EventHandler<IInputDevice> OnInputDeviceAdded { get; set; }

        /// <inheritdoc />
        public EventHandler<IInputDevice> OnInputDeviceRemoved { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<IInputDevice> InputDevices => registeredInputDevices;

        /// <inheritdoc />
        public abstract void Initialize(InputManager inputManager);

        /// <inheritdoc />
        public abstract bool IsEnabled(GameContext gameContext);

        /// <inheritdoc />
        public virtual void Update()
        {
            // Does nothing by default
        }

        /// <inheritdoc />
        public virtual void Pause()
        {
        }

        /// <inheritdoc />
        public virtual void Resume()
        {
        }

        /// <inheritdoc />
        public virtual void Scan()
        {
            // Does nothing by default
        }

        /// <summary>
        /// Calls <see cref="OnInputDeviceAdded"/> and adds the device to the list <see cref="InputDevices"/>
        /// </summary>
        /// <param name="device">The device</param>
        protected void RegisterDevice(IInputDevice device)
        {
            if (registeredInputDevices.Contains(device))
                throw new InvalidOperationException("Tried to use RegisterDevice on an input device twice");

            OnInputDeviceAdded?.Invoke(this, device);
            registeredInputDevices.Add(device);
        }

        /// <summary>
        /// Calls <see cref="OnInputDeviceRemoved"/> and removes the device from the list <see cref="InputDevices"/>
        /// </summary>
        /// <param name="device">The device</param>
        protected void UnregisterDevice(IInputDevice device)
        {
            if (!registeredInputDevices.Contains(device))
                throw new InvalidOperationException("Tried to use UnregisterDevice on an unregistered input device");

            OnInputDeviceRemoved?.Invoke(this, device);
            registeredInputDevices.Remove(device);
        }
    }
}