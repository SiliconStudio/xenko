// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Collections.Generic;
using SDL2;

namespace SiliconStudio.Xenko.Input
{
    public class GamePadSDL : GamePadDeviceBase
    {
        private readonly List<GamePadButtonInfo> buttonInfos = new List<GamePadButtonInfo>();
        private readonly List<GamePadAxisInfo> axisInfos = new List<GamePadAxisInfo>();
        private readonly List<GamePadPovControllerInfo> povControllerInfos = new List<GamePadPovControllerInfo>();
        
        private IntPtr joystick;

        public GamePadSDL(int deviceIndex)
        {
            joystick = SDL.SDL_JoystickOpen(deviceIndex);
            this.Id = SDL.SDL_JoystickGetGUID(joystick);
            DeviceName = SDL.SDL_JoystickName(joystick);
            
            for (int i = 0; i < SDL.SDL_JoystickNumButtons(joystick); i++)
            {
                buttonInfos.Add(new GamePadButtonInfo { Index = i, Name = $"Button {i}" });
            }
            for (int i = 0; i < SDL.SDL_JoystickNumAxes(joystick); i++)
            {
                axisInfos.Add(new GamePadAxisInfo { Index = i, Name = $"Axis {i}" });
            }
            for (int i = 0; i < SDL.SDL_JoystickNumHats(joystick); i++)
            {
                povControllerInfos.Add(new GamePadPovControllerInfo { Index = i, Name = $"Hat {i}" });
            }

            InitializeButtonStates();
            InitializeLayout();
        }
        
        public override void Dispose()
        {
            SDL.SDL_JoystickClose(joystick);
            base.Dispose();
        }

        public override string DeviceName { get; }
        public override Guid Id { get; }

        public override IReadOnlyList<GamePadButtonInfo> ButtonInfos => buttonInfos;
        public override IReadOnlyList<GamePadAxisInfo> AxisInfos => axisInfos;
        public override IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos => povControllerInfos;

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
                float axis;
                if (!axisInfos[i].IsBiDirectional)
                {
                    int inputUnsigned = input + 0x7FFF;
                    axis = (float)inputUnsigned / 0xFFFF;
                }
                else
                    axis = (float)input / 0x7FFF;
                HandleAxis(i, axis);
            }
            for (int i = 0; i < povControllerInfos.Count; i++)
            {
                var hat = SDL.SDL_JoystickGetHat(joystick, i);
                GamePadButton buttons;
                bool hatEnabled = ConvertJoystickHat(hat, out buttons);
                HandlePovController(i, GamePadUtils.ButtonToPovController(buttons), hatEnabled);
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