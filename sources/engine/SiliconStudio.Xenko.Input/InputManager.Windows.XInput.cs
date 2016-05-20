// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP  || SILICONSTUDIO_PLATFORM_WINDOWS_STORE
using System;
using System.Collections.Generic;

using SharpDX.XInput;

namespace SiliconStudio.Xenko.Input
{
    public partial class InputManager
    {
        /// <summary>
        /// Internal GamePad factory handling XInput gamepads.
        /// </summary>
        internal class XInputGamePadFactory : GamePadFactory
        {
            private const int XInputGamePadCount = 4;

            private static readonly Guid[] ControllerGuids = new Guid[XInputGamePadCount];

            private readonly Controller[] controllers;

            /// <summary>
            /// Initializes static members of the <see cref="System.Windows.Input.InputManager" /> class.
            /// </summary>
            static XInputGamePadFactory()
            {
                // Prebuild fake GUID
                for (int i = 0; i < XInputGamePadCount; i++)
                {
                    ControllerGuids[i] = new Guid(i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
            }

            public XInputGamePadFactory()
            {
                controllers = new Controller[XInputGamePadCount];
                for (int i = 0; i < controllers.Length; i++)
                {
                    controllers[i] = new Controller((UserIndex)i);
                }
            }

            public override IEnumerable<GamePadKey> GetConnectedPads()
            {
                // Check that the XInput.dll is present, if not reset the controllers.
                try
                {
                    // ReSharper disable once UnusedVariable
                    var toto = controllers[0].IsConnected;
                }
                catch (DllNotFoundException ex)
                {
                    for (int i = 0; i < XInputGamePadCount; i++)
                        controllers[i] = null;

                    Logger.Warning("XInput dll was not found on the computer. GamePad detection will not fully work for the current game instance. " +
                                   "To fix the problem, please install or repair DirectX installation. [Exception details: {0}]", ex.Message);

                    yield break;
                }
                
                // Return only connected controllers
                foreach (var xinputController in controllers)
                {
                    if (xinputController.IsConnected)
                        yield return new GamePadKey(ControllerGuids[(int)xinputController.UserIndex], this);
                }
            }

            public override GamePad GetGamePad(Guid guid)
            {
                for (int i = 0; i < XInputGamePadCount; i++)
                {
                    if (controllers[i] != null && ControllerGuids[i] == guid)
                    {
                        return new XInputGamePad(controllers[i], new GamePadKey(guid, this));
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Internal gamepad handling XInput gamepads.
        /// </summary>
        private class XInputGamePad : GamePad
        {
#region Constants and Fields

            private Controller instance;

            #endregion

            public XInputGamePad(Controller instance, GamePadKey key) : base(key)
            {
                this.instance = instance;
            }

            public override void Dispose()
            {
                instance = null;
            }

            public override GamePadState GetState()
            {
                var gamePadState = new GamePadState();

                State xinputState;
                if (instance.GetState(out xinputState))
                {
                    gamePadState.IsConnected = true;
                    gamePadState.Buttons = (GamePadButton)xinputState.Gamepad.Buttons;

                    gamePadState.LeftTrigger = xinputState.Gamepad.LeftTrigger / 255.0f;
                    gamePadState.RightTrigger = xinputState.Gamepad.RightTrigger / 255.0f;

                    gamePadState.LeftThumb.X = ClampDeadZone(xinputState.Gamepad.LeftThumbX / 32768.0f, Gamepad.LeftThumbDeadZone / 32768.0f);
                    gamePadState.LeftThumb.Y = ClampDeadZone(xinputState.Gamepad.LeftThumbY / 32768.0f, Gamepad.LeftThumbDeadZone / 32768.0f);

                    gamePadState.RightThumb.X = ClampDeadZone(xinputState.Gamepad.RightThumbX / 32768.0f, Gamepad.RightThumbDeadZone / 32768.0f);
                    gamePadState.RightThumb.Y = ClampDeadZone(xinputState.Gamepad.RightThumbY / 32768.0f, Gamepad.RightThumbDeadZone / 32768.0f);
                }

                return gamePadState;
            }
        }
    }
}

#endif
