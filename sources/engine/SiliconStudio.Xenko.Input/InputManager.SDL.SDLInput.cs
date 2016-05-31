// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Collections.Generic;
using SDL2;

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
                    if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_FALSE)
                    {
                        // We have a joystick that is not recognized as a game controller, we will map it.
                        var jPtr = SDL.SDL_JoystickOpen(i);
                        if (jPtr != IntPtr.Zero)
                        {
                            var bytes = SDL.SDL_JoystickGetGUID(jPtr).ToByteArray();
                            // Convert or bytes into a string representation. This is important because SDL expects
                            // the string representation to represent the memory view without hyphen. Using
                            // Guid.ToString() will not do that.
                            string mapping = "";
                            foreach (var b in bytes)
                            {
                                mapping += b.ToString("X2");
                            }
                            mapping += "," + SDL.SDL_JoystickName(jPtr);
                            // Map various axes, buttons and hats.
                            int j, n;
                            n = SDL.SDL_JoystickNumAxes(jPtr);
                            for (j = 1; j <= n; j++)
                            {
                                switch (j)
                                {
                                    case 1: mapping += ",leftx:a0,lefty:a1"; break;
                                    case 2: mapping += ",rightx:a2,righty:a3"; break;
                                    case 3: mapping += ",lefttrigger:a4"; break;
                                    case 4: mapping += ",righttrigger:a5"; break;
                                }
                            }
                            n = SDL.SDL_JoystickNumButtons(jPtr);
                            for (j = 1; j <= n; j++)
                            {
                                switch (j)
                                {
                                    case 1: mapping += ",a:b0"; break;
                                    case 2: mapping += ",b:b1"; break;
                                    case 3: mapping += ",x:b2"; break;
                                    case 4: mapping += ",y:b3"; break;
                                    case 5: mapping += ",back:b4"; break;
                                    case 6: mapping += ",start:b5"; break;
                                    case 7: mapping += ",leftshoulder:b6"; break;
                                    case 8: mapping += ",rightshoulder:b7"; break;
                                    case 9: mapping += ",leftstick:b8"; break;
                                    case 10: mapping += ",rightstick:b9"; break;
                                    case 11: mapping += ",guide:b10"; break;
                                }
                            }
                            n = SDL.SDL_JoystickNumHats(jPtr);
                            if (n >= 1)
                            {
                                mapping += "dpdown:h0.4,dpleft:h0.8,dpright:h0.2,dpup:h0.1";
                            }
                            SDL.SDL_GameControllerAddMapping(mapping);
                        }
                    }
                    var ptr = SDL.SDL_GameControllerOpen(i);
                    if (ptr != IntPtr.Zero)
                        controllers.Add(ptr, ControllerGuids[i]);
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
