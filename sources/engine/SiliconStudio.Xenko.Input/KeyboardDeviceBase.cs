// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
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
        public readonly Dictionary<Keys, int> KeyRepeats = new Dictionary<Keys, int>();
        protected List<KeyEvent> KeyboardInputEvents = new List<KeyEvent>();
        
        public virtual void Dispose()
        {
        }
        
        public abstract string DeviceName { get; }
        public abstract Guid Id { get; }
        public int Priority { get; set; }
        
        public virtual void Update(List<InputEvent> inputEvents)
        {
            // Fire events
            foreach (var evt in KeyboardInputEvents)
            {
                inputEvents.Add(evt);
            }
            KeyboardInputEvents.Clear();
        }
        
        public virtual bool IsKeyDown(Keys key)
        {
            return KeyRepeats.ContainsKey(key);
        }

        public void HandleKeyDown(Keys key)
        {
            // Increment repeat count on subsequent down events
            int repeatCount = 0;
            if (KeyRepeats.TryGetValue(key, out repeatCount))
                KeyRepeats[key] = ++repeatCount;
            else
                KeyRepeats.Add(key, repeatCount);

            var keyEvent = InputEventPool<KeyEvent>.GetOrCreate(this);
            keyEvent.State = ButtonState.Pressed;
            keyEvent.Key = key;
            KeyboardInputEvents.Add(keyEvent);
        }

        public void HandleKeyUp(Keys key)
        {
            // Prevent duplicate up events
            if (!KeyRepeats.ContainsKey(key))
                return;

            KeyRepeats.Remove(key);
            var keyEvent = InputEventPool<KeyEvent>.GetOrCreate(this);
            keyEvent.State = ButtonState.Released;
            keyEvent.Key = key;
            KeyboardInputEvents.Add(keyEvent);
        }
    }
}