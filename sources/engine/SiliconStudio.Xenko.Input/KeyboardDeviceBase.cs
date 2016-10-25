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
        public abstract string DeviceName { get; }
        public abstract Guid Id { get; }
        public int Priority { get; set; }

        public EventHandler<KeyEvent> OnKey { get; set; }
        public readonly HashSet<Keys> DownKeys = new HashSet<Keys>();

        protected List<KeyEvent> KeyboardInputEvents = new List<KeyEvent>();

        public virtual void Update()
        {
            // Fire events
            foreach (var evt in KeyboardInputEvents)
            {
                OnKey?.Invoke(this, evt);
            }
            KeyboardInputEvents.Clear();
        }

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

        public virtual void Dispose()
        {
        }
    }
}