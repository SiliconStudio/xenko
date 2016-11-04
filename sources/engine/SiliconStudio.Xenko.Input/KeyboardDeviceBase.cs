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

            KeyboardInputEvents.Add(new KeyEvent(this) { State = ButtonState.Pressed, RepeatCount = repeatCount, Key = key });
        }

        public void HandleKeyUp(Keys key)
        {
            // Prevent duplicate up events
            if (!KeyRepeats.ContainsKey(key))
                return;

            KeyRepeats.Remove(key);
            KeyboardInputEvents.Add(new KeyEvent(this) { State = ButtonState.Released, Key = key });
        }
    }
}