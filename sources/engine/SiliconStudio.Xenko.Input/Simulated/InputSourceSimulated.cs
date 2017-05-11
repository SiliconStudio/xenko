// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides a virtual mouse and keyboard that generate input events like a normal mouse/keyboard when any of the functions (Simulate...) are called
    /// </summary>
    public partial class InputSourceSimulated : InputSourceBase
    {
        /// <summary>
        /// Should simulated input be added to the input manager
        /// </summary>
        public static bool Enabled = false;

        /// <summary>
        /// The simulated input source
        /// </summary>
        public static InputSourceSimulated Instance;

        private bool keyboardConnected;
        private bool mouseConnected;

        private List<GamePadSimulated> gamePads = new List<GamePadSimulated>();
        private List<Tuple<IInputDevice, DeviceEventType>> deviceEvents = new List<Tuple<IInputDevice, DeviceEventType>>();

        public KeyboardSimulated Keyboard { get; private set; }

        public MouseSimulated Mouse { get; private set; }

        public IReadOnlyList<GamePadSimulated> GamePads => gamePads;


        public override void Initialize(InputManager inputManager)
        {
            Keyboard = new KeyboardSimulated();
            Mouse = new MouseSimulated();
            SetKeyboardConnected(true);
            SetMouseConnected(true);
            Instance = this;
        }

        public override void Dispose()
        {
            base.Dispose();
            Instance = null;
        }

        public override void Update()
        {
            base.Update();
            foreach (var evt in deviceEvents)
            {
                switch (evt.Item2)
                {
                    case DeviceEventType.Add:
                        RegisterDevice(evt.Item1);
                        break;
                    case DeviceEventType.Remove:
                        UnregisterDevice(evt.Item1);
                        break;
                }
            }
            deviceEvents.Clear();
        }

        public GamePadSimulated AddGamePad()
        {
            var gamePad = new GamePadSimulated(this);
            gamePads.Add(gamePad);
            deviceEvents.Add(new Tuple<IInputDevice, DeviceEventType>(gamePad, DeviceEventType.Add));
            return gamePad;
        }

        public void RemoveGamePad(GamePadSimulated gamePad)
        {
            if (!gamePads.Contains(gamePad))
                throw new InvalidOperationException("Simulated GamePad does not exist");
            deviceEvents.Add(new Tuple<IInputDevice, DeviceEventType>(gamePad, DeviceEventType.Remove));
            gamePads.Remove(gamePad);
        }

        public void RemoveAllGamePads()
        {
            foreach (var gamePad in gamePads)
                deviceEvents.Add(new Tuple<IInputDevice, DeviceEventType>(gamePad, DeviceEventType.Remove));
            gamePads.Clear();
        }

        public void SetKeyboardConnected(bool connected)
        {
            if (connected != keyboardConnected)
            {
                if (connected)
                {
                    RegisterDevice(Keyboard);
                }
                else
                {
                    UnregisterDevice(Keyboard);
                }

                keyboardConnected = connected;
            }
        }

        public void SetMouseConnected(bool connected)
        {
            if (connected != mouseConnected)
            {
                if (connected)
                {
                    RegisterDevice(Mouse);
                }
                else
                {
                    UnregisterDevice(Mouse);
                }

                mouseConnected = connected;
            }
        }

        private enum DeviceEventType
        {
            Add,
            Remove
        }
    }
}
