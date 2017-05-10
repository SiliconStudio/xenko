// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input
{
    public abstract class GamePadDeviceBase : IGamePadDevice
    {
        private readonly HashSet<GamePadButton> releasedButtons;
        private readonly HashSet<GamePadButton> pressedButtons;
        private readonly HashSet<GamePadButton> downButtons;
        public abstract string Name { get; }
        public abstract Guid Id { get; }
        public abstract Guid ProductId { get; }
        public abstract GamePadState State { get; }
        public int Priority { get; set; }
        public int Index { get; private set; }

        public IReadOnlySet<GamePadButton> PressedButtons { get; }
        public IReadOnlySet<GamePadButton> ReleasedButtons { get; }
        public IReadOnlySet<GamePadButton> DownButtons { get; }

        public abstract IInputSource Source { get; }

        public event EventHandler<GamePadIndexChangedEventArgs> IndexChanged;

        public abstract void Update(List<InputEvent> inputEvents);
        public abstract void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight);

        protected GamePadDeviceBase()
        {
            DownButtons = new ReadOnlySet<GamePadButton>(downButtons = new HashSet<GamePadButton>());
            PressedButtons = new ReadOnlySet<GamePadButton>(pressedButtons = new HashSet<GamePadButton>());
            ReleasedButtons = new ReadOnlySet<GamePadButton>(releasedButtons = new HashSet<GamePadButton>());
        }

        protected void SetIndexInternal(int index, bool isDeviceSideChange = true)
        {
            if (index != Index)
            {
                Index = index;
                IndexChanged?.Invoke(this, new GamePadIndexChangedEventArgs() { Index = index, IsDeviceSideChange = isDeviceSideChange });
            }
        }

        /// <summary>
        /// Clears previous Pressed/Released states
        /// </summary>
        protected void ClearButtonStates()
        {
            pressedButtons.Clear();
            releasedButtons.Clear();
        }

        /// <summary>
        /// Updates Pressed/Released/Down collections
        /// </summary>
        protected void UpdateButtonState(GamePadButtonEvent evt)
        {
            if (evt.IsDown && !downButtons.Contains(evt.Button))
            {
                pressedButtons.Add(evt.Button);
                downButtons.Add(evt.Button);
            }
            else if(!evt.IsDown && downButtons.Contains(evt.Button))
            {
                releasedButtons.Add(evt.Button);
                downButtons.Remove(evt.Button);
            }
        }
    }
}
