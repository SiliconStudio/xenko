// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for keyboard devices
    /// </summary>
    public abstract class KeyboardDeviceBase : IKeyboardDevice
    {
        private readonly List<Keys> downKeys = new List<Keys>();

        protected readonly List<KeyEvent> Events = new List<KeyEvent>();

        public readonly Dictionary<Keys, int> KeyRepeats = new Dictionary<Keys, int>();

        public IReadOnlyList<Keys> DownKeys => downKeys;

        public abstract string Name { get; }

        public abstract Guid Id { get; }

        public int Priority { get; set; }

        public abstract IInputSource Source { get; }

        public virtual void Update(List<InputEvent> inputEvents)
        {
            // Fire events
            foreach (var evt in Events)
            {
                inputEvents.Add(evt);
            }
            Events.Clear();
        }
        
        public virtual bool IsKeyDown(Keys key)
        {
            return KeyRepeats.ContainsKey(key);
        }

        public void HandleKeyDown(Keys key)
        {
            // Increment repeat count on subsequent down events
            int repeatCount;
            if (KeyRepeats.TryGetValue(key, out repeatCount))
            {
                KeyRepeats[key] = ++repeatCount;
            }
            else
            {
                KeyRepeats.Add(key, repeatCount);
                downKeys.Add(key);
            }

            var keyEvent = InputEventPool<KeyEvent>.GetOrCreate(this);
            keyEvent.IsDown = true;
            keyEvent.Key = key;
            keyEvent.RepeatCount = repeatCount;
            Events.Add(keyEvent);
        }

        public void HandleKeyUp(Keys key)
        {
            // Prevent duplicate up events
            if (!KeyRepeats.ContainsKey(key))
                return;

            KeyRepeats.Remove(key);
            downKeys.Remove(key);
            var keyEvent = InputEventPool<KeyEvent>.GetOrCreate(this);
            keyEvent.IsDown = false;
            keyEvent.Key = key;
            keyEvent.RepeatCount = 0;
            Events.Add(keyEvent);
        }
    }
}