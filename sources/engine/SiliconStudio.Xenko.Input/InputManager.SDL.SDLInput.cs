// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Collections.Generic;
using SDL2;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Xenko.Input
{
    public partial class InputManager
    {
        /// <summary>
        /// Internal GamePad factory handling XInput gamepads.
        /// </summary>
        internal class SdlInputGamePadFactory : GamePadFactory
        {
            private static readonly int SdlInputGamePadCount;

            private static readonly Guid[] ControllerGuids;

            private readonly Dictionary<IntPtr, Guid> controllers;

            /// <summary>
            /// Initializes static members of the <see cref="System.Windows.Input.InputManager" /> class.
            /// </summary>
            static SdlInputGamePadFactory()
            {
                var n = SDL.SDL_NumJoysticks();
                SdlInputGamePadCount = n;
                ControllerGuids = new  Guid[n];
                for (int i = 0; i < n; i++)
                {
                    ControllerGuids[i] = new Guid(i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
            }

            public SdlInputGamePadFactory()
            {
                controllers = new Dictionary<IntPtr, Guid>(SdlInputGamePadCount);
                for (int i = 0; i < SdlInputGamePadCount; i++)
                {
                    if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
                    {
                        var ptr = SDL.SDL_GameControllerOpen(i);
                        controllers.Add(ptr, ControllerGuids[i]);
                    }
                }
            }

            public override IEnumerable<GamePadKey> GetConnectedPads()
            {
                foreach (var xinputController in controllers)
                {
                    if (SDL.SDL_GameControllerGetAttached(xinputController.Key) == SDL.SDL_bool.SDL_TRUE)
                        yield return new GamePadKey(xinputController.Value, this);
                }
            }

            public override GamePad GetGamePad(Guid guid)
            {
                foreach (var xinputController in controllers)
                {
                    if (xinputController.Value == guid)
                    {
                        return new SdlInputGamePad(xinputController.Key, new GamePadKey(guid, this));
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Internal gamepad handling SdlInput gamepads.
        /// </summary>
        private class SdlInputGamePad : GamePad
        {
#region Constants and Fields

            private IntPtr instance;

            #endregion

            public SdlInputGamePad(IntPtr instance, GamePadKey key) : base(key)
            {
                this.instance = instance;
            }

            public override void Dispose()
            {
                instance = IntPtr.Zero;
            }

            public override GamePadState GetState()
            {
                var gamePadState = new GamePadState();

                if (SDL.SDL_GameControllerGetAttached(instance) == SDL.SDL_bool.SDL_TRUE)
                {
                    const float LeftThumbDeadZone = 7849.0f/32768.0f;
                    const float RightThumbDeadZone = 8689.0f/32768.0f;

                    gamePadState.IsConnected = true;
                    // Iterate through all buttons
                    GamePadButton state = GamePadButton.None;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) == 1)
                        state |= GamePadButton.A;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) == 1)
                        state |= GamePadButton.B;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) == 1)
                        state |= GamePadButton.Back;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) == 1)
                        state |= GamePadButton.PadDown;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) == 1)
                        state |= GamePadButton.PadUp;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) == 1)
                        state |= GamePadButton.PadLeft;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) == 1)
                        state |= GamePadButton.PadRight;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) == 1)
                        state |= GamePadButton.X;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) == 1)
                        state |= GamePadButton.Y;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) == 1)
                        state |= GamePadButton.LeftShoulder;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) == 1)
                        state |= GamePadButton.RightShoulder;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) == 1)
                        state |= GamePadButton.LeftThumb;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) == 1)
                        state |= GamePadButton.RightThumb;
                    if (SDL.SDL_GameControllerGetButton(instance, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) == 1)
                        state |= GamePadButton.Start;

                    gamePadState.Buttons = state;

                    var leftThumbX = SDL.SDL_GameControllerGetAxis(instance, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX)/32768.0f;
                    var leftThumbY = SDL.SDL_GameControllerGetAxis(instance, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY)/32768.0f;

                    gamePadState.LeftThumb.X = ClampDeadZone(leftThumbX, LeftThumbDeadZone);
                    gamePadState.LeftThumb.Y = ClampDeadZone(leftThumbY, LeftThumbDeadZone);

                    var rightThumbX = SDL.SDL_GameControllerGetAxis(instance, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX)/32768.0f;
                    var rightThumbY = SDL.SDL_GameControllerGetAxis(instance, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY)/32768.0f;

                    gamePadState.RightThumb.X = ClampDeadZone(rightThumbX, RightThumbDeadZone);
                    gamePadState.RightThumb.Y = ClampDeadZone(rightThumbY, RightThumbDeadZone);

                    gamePadState.LeftTrigger = SDL.SDL_GameControllerGetAxis(instance, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT)/32768.0f;
                    gamePadState.RightTrigger = SDL.SDL_GameControllerGetAxis(instance, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT)/32768.0f;

                }
                return gamePadState;
            }
        }
    }
}

#endif
