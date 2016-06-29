// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF) && !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    public partial class InputManager
    {
        /// <summary>
        /// Internal GamePad factory handling DirectInput gamepads.
        /// </summary>
        internal class DirectInputGamePadFactory : GamePadFactory
        {
            private readonly DirectInput directInput;

            public DirectInputGamePadFactory()
            {
                directInput = new DirectInput();
            }

            public override IEnumerable<GamePadKey> GetConnectedPads()
            {
                return directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices).
                    Where(x => !Native.DirectInput.XInputChecker.IsXInputDevice(ref x.ProductGuid)).
                    Select(deviceInstance => new GamePadKey(deviceInstance.InstanceGuid, this));
            }

            public override GamePad GetGamePad(Guid guid)
            {
                return new DirectInputGamePad(directInput, new GamePadKey(guid, this));
            }
        }

        /// <summary>
        /// Internal gamepad handling DirectInput gamepads.
        /// </summary>
        private class DirectInputGamePad : GamePad
        {
            #region Constants and Fields

            private readonly GamePadKey key;

            private readonly DirectInput directInput;

            private Joystick instance;

            private JoystickState joystickState;

            #endregion

            public DirectInputGamePad(DirectInput directInput, GamePadKey key) : base(key)
            {
                this.key = key;
                this.directInput = directInput;
                this.instance = new Joystick(directInput, key.Guid);
                joystickState = new JoystickState();
            }

            public override void Dispose()
            {
                if (instance != null)
                {
                    instance.Dispose();
                    instance = null;
                }
            }

            public override GamePadState GetState()
            {
                var gamePadState = new GamePadState();

                // Get the current joystick state
                try
                {
                    if (instance == null)
                    {
                        if (directInput.IsDeviceAttached(key.Guid))
                        {
                            instance = new Joystick(directInput, key.Guid);
                        }
                        else
                        {
                            return gamePadState;
                        }
                    }

                    // Acquire the joystick
                    instance.Acquire();

                    // Make sure that the latest state is up to date
                    instance.Poll();

                    // Get the state
                    instance.GetCurrentState(ref joystickState);
                }
                catch (SharpDX.SharpDXException)
                {
                    // If there was an exception, dispose the native instance 
                    try
                    {
                        if (instance != null)
                        {
                            instance.Dispose();
                            instance = null;
                        }

                        // Return a GamePadState that specify that it is not connected
                        return gamePadState;
                    }
                    catch (Exception)
                    {
                    }
                }

                //Console.WriteLine(joystickState);
                gamePadState.IsConnected = true;

                gamePadState.Buttons = GamePadButton.None;
                if (joystickState.Buttons[0])
                {
                    gamePadState.Buttons |= GamePadButton.X;
                }
                if (joystickState.Buttons[1])
                {
                    gamePadState.Buttons |= GamePadButton.A;
                }
                if (joystickState.Buttons[2])
                {
                    gamePadState.Buttons |= GamePadButton.B;
                }
                if (joystickState.Buttons[3])
                {
                    gamePadState.Buttons |= GamePadButton.Y;
                }
                if (joystickState.Buttons[4])
                {
                    gamePadState.Buttons |= GamePadButton.LeftShoulder;
                }
                if (joystickState.Buttons[5])
                {
                    gamePadState.Buttons |= GamePadButton.RightShoulder;
                }
                if (joystickState.Buttons[6])
                {
                    gamePadState.LeftTrigger = 1.0f;
                }
                if (joystickState.Buttons[7])
                {
                    gamePadState.RightTrigger = 1.0f;
                }

                if (joystickState.Buttons[8])
                {
                    gamePadState.Buttons |= GamePadButton.Back;
                }
                if (joystickState.Buttons[9])
                {
                    gamePadState.Buttons |= GamePadButton.Start;
                }

                if (joystickState.Buttons[10])
                {
                    gamePadState.Buttons |= GamePadButton.LeftThumb;
                }
                if (joystickState.Buttons[11])
                {
                    gamePadState.Buttons |= GamePadButton.RightThumb;
                }

                int dPadRawValue = joystickState.PointOfViewControllers[0];
                if (dPadRawValue >= 0)
                {
                    int dPadValue = dPadRawValue / 4500;
                    switch (dPadValue)
                    {
                        case 0:
                            gamePadState.Buttons |= GamePadButton.PadUp;
                            break;
                        case 1:
                            gamePadState.Buttons |= GamePadButton.PadUp;
                            gamePadState.Buttons |= GamePadButton.PadRight;
                            break;
                        case 2:
                            gamePadState.Buttons |= GamePadButton.PadRight;
                            break;
                        case 3:
                            gamePadState.Buttons |= GamePadButton.PadRight;
                            gamePadState.Buttons |= GamePadButton.PadDown;
                            break;
                        case 4:
                            gamePadState.Buttons |= GamePadButton.PadDown;
                            break;
                        case 5:
                            gamePadState.Buttons |= GamePadButton.PadDown;
                            gamePadState.Buttons |= GamePadButton.PadLeft;
                            break;
                        case 6:
                            gamePadState.Buttons |= GamePadButton.PadLeft;
                            break;
                        case 7:
                            gamePadState.Buttons |= GamePadButton.PadLeft;
                            gamePadState.Buttons |= GamePadButton.PadUp;
                            break;
                    }
                }

                // Left Thumb
                gamePadState.LeftThumb = new Vector2(2.0f * (joystickState.X / 65535.0f - 0.5f), -2.0f * (joystickState.Y / 65535.0f - 0.5f));
                gamePadState.LeftThumb.X = ClampDeadZone(gamePadState.LeftThumb.X, GamePadAxisDeadZone);
                gamePadState.LeftThumb.Y = ClampDeadZone(gamePadState.LeftThumb.Y, GamePadAxisDeadZone);

                // Right Thumb
                gamePadState.RightThumb = new Vector2(2.0f * (joystickState.Z / 65535.0f - 0.5f), -2.0f * (joystickState.RotationZ / 65535.0f - 0.5f));
                gamePadState.RightThumb.X = ClampDeadZone(gamePadState.RightThumb.X, GamePadAxisDeadZone);
                gamePadState.RightThumb.Y = ClampDeadZone(gamePadState.RightThumb.Y, GamePadAxisDeadZone);

                return gamePadState;
            }
        }
    }
}
#endif
