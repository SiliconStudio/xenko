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
        public override string DeviceName => deviceName;
        public override Guid Id => deviceId;
        public override IReadOnlyCollection<GamePadButtonInfo> ButtonInfos => buttonInfos;
        public override IReadOnlyCollection<GamePadAxisInfo> AxisInfos => axisInfos;
        public override IReadOnlyCollection<GamePadPovControllerInfo> PovControllerInfos => povControllerInfos;

        private readonly List<GamePadButtonInfo> buttonInfos = new List<GamePadButtonInfo>();
        private readonly List<GamePadAxisInfo> axisInfos = new List<GamePadAxisInfo>();
        private readonly List<GamePadPovControllerInfo> povControllerInfos = new List<GamePadPovControllerInfo>();

        private string deviceName;
        private Guid deviceId;
        private IntPtr joystick;

        public GamePadSDL(int deviceIndex)
        {
            joystick = SDL.SDL_JoystickOpen(deviceIndex);
            this.deviceId = SDL.SDL_JoystickGetGUID(joystick);
            deviceName = SDL.SDL_JoystickName(joystick);

            for (int i = 0; i < SDL.SDL_JoystickNumButtons(joystick); i++)
            {
                buttonInfos.Add(new GamePadButtonInfo { InstanceId = i, Name = "Buttons" });
            }
            for (int i = 0; i < SDL.SDL_JoystickNumAxes(joystick); i++)
            {
                axisInfos.Add(new GamePadAxisInfo { InstanceId = i, Name = "Axis" });
            }
            for (int i = 0; i < SDL.SDL_JoystickNumHats(joystick); i++)
            {
                povControllerInfos.Add(new GamePadPovControllerInfo { InstanceId = i, Name = "Hat" });
            }

            InitializeButtonStates();
        }

        public override void Update()
        {
            if (SDL.SDL_JoystickGetAttached(joystick) == SDL.SDL_bool.SDL_FALSE)
            {
                OnDisconnect?.Invoke(this, null);
                Dispose();
                return;
            }

            for (int i = 0; i < buttonInfos.Count; i++)
            {
                HandleButton(i, SDL.SDL_JoystickGetButton(joystick, i) != 0);
            }
            for (int i = 0; i < axisInfos.Count; i++)
            {
                float axis = (float)SDL.SDL_JoystickGetAxis(joystick, i)/0x7FFF;
                HandleAxis(i, axis);
            }
            for (int i = 0; i < povControllerInfos.Count; i++)
            {
                var hat = SDL.SDL_JoystickGetHat(joystick, i);
                GamePadButton buttons;
                bool hatEnabled = ConvertJoystickHat(hat, out buttons);
                HandlePovController(i, GamePadConversions.ButtonToPovController(buttons), hatEnabled);
            }

            base.Update();
        }

        bool ConvertJoystickHat(byte hat, out GamePadButton buttons)
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

        public override void Dispose()
        {
            SDL.SDL_JoystickClose(joystick);
            base.Dispose();
        }
    }
}
#endif