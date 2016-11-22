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

    /// <summary>
    /// A <see cref="IGamePadDevice"/> from a <see cref="IGameControllerDevice"/> using a <see cref="GamePadLayout"/> to create a mapping between the two
    /// </summary>
    public abstract class GamePadFromLayout : GamePadDeviceBase
    {
        protected InputManager InputManager;
        protected GamePadLayout Layout;
        protected IGameControllerDevice GameControllerDevice;

        private GamePadState state = new GamePadState();

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePadFromLayout"/> class.
        /// </summary>
        protected GamePadFromLayout(InputManager inputManager, IGameControllerDevice controller, GamePadLayout layout)
        {
            InputManager = inputManager;
            Layout = layout;
            GameControllerDevice = controller;
        }

        public override GamePadState State
        {
            get { return state; }
        }

        public override void Update(List<InputEvent> inputEvents)
        {
            // Wrap the controller device and turn it's events into gamepad events
            List<InputEvent> controllerEvents = new List<InputEvent>();
            GameControllerDevice.Update(controllerEvents);

            int eventStart = inputEvents.Count;
            foreach (var evt in controllerEvents)
            {
                Layout.MapInputEvent(this, GameControllerDevice, evt, inputEvents);
                InputManager.PoolInputEvent(evt); // Put event back into event pool
            }

            // Apply events to gamepad state
            for (int i = eventStart; i < inputEvents.Count;)
            {
                if (!state.Update(inputEvents[i]))
                {
                    // Discard event, since it didn't affect the state
                    InputManager.PoolInputEvent(inputEvents[i]); // Put event back into event pool
                    inputEvents.RemoveAt(i);
                }
                else
                    i++;
            }
        }
    }
}