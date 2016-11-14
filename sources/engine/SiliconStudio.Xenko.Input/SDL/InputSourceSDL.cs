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
        private readonly HashSet<Guid> devicesToRemove = new HashSet<Guid>();
        private GameContext<Window> context;
        private Window uiControl;
        private MouseSDL mouse;
        private KeyboardSDL keyboard;
        private InputManager inputManager;

        public override void Dispose()
        {
            // Dispose all the gamepads
            foreach (var pair in InputDevices)
            {
                pair.Value.Dispose();
            }

            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);

            base.Dispose();
        }

        public override void Initialize(InputManager inputManager)
        {
            this.inputManager = inputManager;
            context = inputManager.Game.Context as GameContext<Window>;
            uiControl = context.Control;

            SDL.SDL_InitSubSystem(SDL.SDL_INIT_JOYSTICK);

            mouse = new MouseSDL(inputManager.Game, uiControl);
            keyboard = new KeyboardSDL(uiControl);

            RegisterDevice(mouse);
            RegisterDevice(keyboard);

            // Scan for gamepads
            Scan();
        }

        public override void Update()
        {
            // Notify event listeners of device removals
            foreach (var deviceIdToRemove in devicesToRemove)
            {
                var gamePad = InputDevices[deviceIdToRemove] as GamePadSDL;
                UnregisterDevice(gamePad);

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
                if (!InputDevices.ContainsKey(joystickId))
                {
                    OpenDevice(i);
                }
            }
        }

        public void OpenDevice(int deviceIndex)
        {
            var joystickId = SDL.SDL_JoystickGetDeviceGUID(deviceIndex);
            if (InputDevices.ContainsKey(joystickId))
                throw new InvalidOperationException($"SDL GamePad already opened {deviceIndex}/{joystickId}");

            var newGamepad = new GamePadSDL(deviceIndex);
            newGamepad.Disconnected += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(newGamepad.Id);
            };
            RegisterDevice(newGamepad);
        }
    }
}

#endif