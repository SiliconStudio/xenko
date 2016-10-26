// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for keyboard devices
    /// </summary>
    public abstract class KeyboardDeviceBase : IKeyboardDevice
    {
        public readonly HashSet<Keys> DownKeys = new HashSet<Keys>();
        protected List<KeyEvent> KeyboardInputEvents = new List<KeyEvent>();

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <inheritdoc />
        public abstract string DeviceName { get; }
        /// <inheritdoc />
        public abstract Guid Id { get; }
        /// <inheritdoc />
        public int Priority { get; set; }
        /// <inheritdoc />
        public EventHandler<KeyEvent> OnKey { get; set; }

        /// <inheritdoc />
        public virtual void Update()
        {
            // Fire events
            foreach (var evt in KeyboardInputEvents)
            {
                OnKey?.Invoke(this, evt);
            }
            KeyboardInputEvents.Clear();
        }

        /// <inheritdoc />
        public virtual bool IsKeyDown(Keys key)
        {
            return DownKeys.Contains(key);
        }

        public void HandleKeyDown(Keys key)
        {
            DownKeys.Add(key);
            KeyboardInputEvents.Add(new KeyEvent(key, KeyEventType.Pressed));
        }

        public void HandleKeyUp(Keys key)
        {
            DownKeys.Remove(key);
            KeyboardInputEvents.Add(new KeyEvent(key, KeyEventType.Released));
        }
    }
}