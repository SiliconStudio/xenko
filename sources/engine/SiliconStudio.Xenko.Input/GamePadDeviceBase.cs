// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    public abstract class GamePadDeviceBase : IGamePadDevice
    {
        public abstract string Name { get; }

        public abstract Guid Id { get; }

        public abstract Guid ProductId { get; }

        public abstract GamePadState State { get; }

        public int Priority { get; set; }

        public int Index { get; private set; }

        public abstract IInputSource Source { get; }

        public event EventHandler<GamePadIndexChangedEventArgs> IndexChanged;

        public abstract void Update(List<InputEvent> inputEvents);

        public abstract void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight);

        protected void SetIndexInternal(int index, bool isDeviceSideChange = true)
        {
            if (index != Index)
            {
                Index = index;
                IndexChanged?.Invoke(this, new GamePadIndexChangedEventArgs() { Index = index, IsDeviceSideChange = isDeviceSideChange });
            }
        }
    }
}