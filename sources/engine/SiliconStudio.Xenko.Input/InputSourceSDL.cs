// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Collections.Generic;
using SDL2;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.SDL;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides support for mouse/keyboard/gamepads using SDL
    /// </summary>
    public class InputSourceSDL : InputSourceBase
    {
        private Dictionary<Guid, GamePadSDL> registeredDevices = new Dictionary<Guid, GamePadSDL>();
        private HashSet<Guid> devicesToRemove = new HashSet<Guid>();

        private GameContext<Window> context;
        private Window uiControl;
        private MouseSDL mouse;
        private KeyboardSDL keyboard;

        public override void Initialize(InputManager inputManager)
        {
            context = inputManager.Game.Context as GameContext<Window>;
            uiControl = context.Control;

            SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK);

            mouse = new MouseSDL(inputManager.Game, uiControl);
            keyboard = new KeyboardSDL(uiControl);

            RegisterDevice(mouse);
            RegisterDevice(keyboard);

            // Scan for gamepads
            Scan();
        }

        public override bool IsEnabled(GameContext gameContext)
        {
            return gameContext is GameContext<Window>;
        }

        public override void Update()
        {
            // Notify event listeners of device removals
            foreach (var deviceIdToRemove in devicesToRemove)
            {
                var gamePad = registeredDevices[deviceIdToRemove];
                UnregisterDevice(gamePad);
                registeredDevices.Remove(deviceIdToRemove);

                if (gamePad.Connected)
                    gamePad.Dispose();
            }
            devicesToRemove.Clear();
        }

        public override void Scan()
        {
            for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
            {
                var joystickId = SDL.SDL_JoystickGetDeviceGUID(i);
                if (!registeredDevices.ContainsKey(joystickId))
                {
                    OpenDevice(i);
                }
            }
        }

        public void OpenDevice(int deviceIndex)
        {
            var joystickId = SDL.SDL_JoystickGetDeviceGUID(deviceIndex);
            if (registeredDevices.ContainsKey(joystickId))
                throw new InvalidOperationException($"SDL GamePad already opened {deviceIndex}/{joystickId}");

            var newGamepad = new GamePadSDL(deviceIndex);
            newGamepad.OnDisconnect += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(newGamepad.Id);
            };
            registeredDevices.Add(newGamepad.Id, newGamepad);
            RegisterDevice(newGamepad);
        }

        public override void Dispose()
        {
            base.Dispose();

            // Dispose all the gamepads
            foreach (var pair in registeredDevices)
            {
                pair.Value.Dispose();
            }
            registeredDevices.Clear();
        }
    }
}

#endif