// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System.Collections.Generic;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A known gamepad that uses DirectInput as a driver
    /// </summary>
    public class GamePadDirectInput : GameControllerDirectInput, IGamePadDevice, IGamePadIndexAssignable
    {
        private GamePadState state = new GamePadState();
        private GamePadLayout layout;
        private InputManager inputManager;
        private List<InputEvent> sourceEvents = new List<InputEvent>();

        public GamePadState State => state;
        
        public GamePadDirectInput(InputManager inputManager, DirectInput directInput, DeviceInstance instance, GamePadLayout layout)
            : base(directInput, instance)
        {
            this.inputManager = inputManager;
            this.layout = layout;
        }

        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(sourceEvents);
            foreach (var sourceEvent in sourceEvents)
            {
                int lastEvents = inputEvents.Count;

                // Convert controller events into gamepad events
                layout.MapInputEvent(this, sourceEvent, inputEvents);

                // Send generated events to GamePadState to update it
                for (int i = lastEvents; i < inputEvents.Count; i++)
                {
                    state.Update(inputEvents[i]);
                }

                // Pool the original events, since we don't need those anymore
                inputManager.PoolInputEvent(sourceEvent);
            }
            sourceEvents.Clear();
        }

        public void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
        {
            // No vibration support in directinput gamepads
        }

        public new int Index
        {
            get { return IndexInternal; }
            set { IndexInternal = value; }
        }
    }
}
#endif