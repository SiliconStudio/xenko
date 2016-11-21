// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Collections.Generic;
using SDL2;

namespace SiliconStudio.Xenko.Input
{
    public class GameControllerSDL : GameControllerDeviceBase, IDisposable
    {
        private readonly List<GameControllerButtonInfo> buttonInfos = new List<GameControllerButtonInfo>();
        private readonly List<GameControllerAxisInfo> axisInfos = new List<GameControllerAxisInfo>();
        private readonly List<PovControllerInfo> povControllerInfos = new List<PovControllerInfo>();
        
        private IntPtr joystick;

        public GameControllerSDL(int deviceIndex)
        {
            joystick = SDL.SDL_JoystickOpen(deviceIndex);
            Id = Guid.NewGuid(); // Should be unique
            ProductId = SDL.SDL_JoystickGetGUID(joystick); // Will identify the type of controller
            DeviceName = SDL.SDL_JoystickName(joystick);
            
            for (int i = 0; i < SDL.SDL_JoystickNumButtons(joystick); i++)
            {
                buttonInfos.Add(new GameControllerButtonInfo { Index = i, Name = $"Button {i}" });
            }
            for (int i = 0; i < SDL.SDL_JoystickNumAxes(joystick); i++)
            {
                axisInfos.Add(new GameControllerAxisInfo { Index = i, Name = $"Axis {i}" });
            }
            for (int i = 0; i < SDL.SDL_JoystickNumHats(joystick); i++)
            {
                povControllerInfos.Add(new PovControllerInfo { Index = i, Name = $"Hat {i}" });
            }

            InitializeButtonStates();
        }
        
        public void Dispose()
        {
            SDL.SDL_JoystickClose(joystick);
            if (Disconnected == null)
                throw new InvalidOperationException("Something should handle controller disconnect");
            Disconnected.Invoke(this, null);
        }

        public override string DeviceName { get; }
        public override Guid Id { get; }
        public override Guid ProductId { get; }

        public override IReadOnlyList<GameControllerButtonInfo> ButtonInfos => buttonInfos;
        public override IReadOnlyList<GameControllerAxisInfo> AxisInfos => axisInfos;
        public override IReadOnlyList<PovControllerInfo> PovControllerInfos => povControllerInfos;

        public event EventHandler Disconnected;

        public override void Update(List<InputEvent> inputEvents)
        {
            if (SDL.SDL_JoystickGetAttached(joystick) == SDL.SDL_bool.SDL_FALSE)
            {
                Dispose();
                return;
            }

            for (int i = 0; i < buttonInfos.Count; i++)
            {
                HandleButton(i, SDL.SDL_JoystickGetButton(joystick, i) != 0);
            }
            for (int i = 0; i < axisInfos.Count; i++)
            {
                short input = SDL.SDL_JoystickGetAxis(joystick, i);
                float axis = (float)input / 0x7FFF;
                HandleAxis(i, axis);
            }
            for (int i = 0; i < povControllerInfos.Count; i++)
            {
                var hat = SDL.SDL_JoystickGetHat(joystick, i);
                GamePadButton buttons;
                bool hatEnabled = ConvertJoystickHat(hat, out buttons);
                HandlePovController(i, GameControllerUtils.ButtonToPovController(buttons), hatEnabled);
            }

            base.Update(inputEvents);
        }

        private bool ConvertJoystickHat(byte hat, out GamePadButton buttons)
        {
            buttons = 0;
            if (hat == SDL.SDL_HAT_CENTERED)
                return false;
            for (int j = 0; j < 4; j++)
            {
                int mask = 1 << j;
                if ((hat & mask) != 0)
                {
                    switch (mask)
                    {
                        case SDL.SDL_HAT_UP:
                            buttons |= GamePadButton.PadUp;
                            break;
                        case SDL.SDL_HAT_RIGHT:
                            buttons |= GamePadButton.PadRight;
                            break;
                        case SDL.SDL_HAT_DOWN:
                            buttons |= GamePadButton.PadDown;
                            break;
                        case SDL.SDL_HAT_LEFT:
                            buttons |= GamePadButton.PadLeft;
                            break;
                    }
                }
            }
            return true;
        }
    }
}
#endif