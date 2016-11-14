// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Threading;
using SharpDX;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    public class GamePadDirectInput : GamePadDeviceBase
    {
        private readonly List<GamePadButtonInfo> buttonInfos = new List<GamePadButtonInfo>();
        private readonly List<GamePadAxisInfo> axisInfos = new List<GamePadAxisInfo>();
        private readonly List<GamePadPovControllerInfo> povControllerInfos = new List<GamePadPovControllerInfo>();

        private CustomGamePad gamepad;
        private CustomGamePadState state = new CustomGamePadState();

        public GamePadDirectInput(DirectInput directInput, DeviceInstance instance)
        {
            DeviceName = instance.InstanceName.TrimEnd('\0');
            Id = instance.InstanceGuid;
            ProductId = instance.ProductGuid;
            gamepad = new CustomGamePad(directInput, instance.InstanceGuid);
            var objects = gamepad.GetObjects();
            foreach (var obj in objects)
            {
                var objectId = obj.ObjectId;
                GamePadObjectInfo objInfo = null;
                if (objectId.HasAnyFlag(DeviceObjectTypeFlags.Button | DeviceObjectTypeFlags.PushButton | DeviceObjectTypeFlags.ToggleButton))
                {
                    if (buttonInfos.Count == CustomGamePadStateRaw.MaxSupportedButtons)
                    {
                        // Maximum amount of supported buttons reached, don't register any more
                        continue;
                    }
                    var btn = new GamePadButtonInfo();
                    if (objectId.HasFlags(DeviceObjectTypeFlags.ToggleButton))
                        btn.Type = GamePadButtonType.ToggleButton;
                    else
                        btn.Type = GamePadButtonType.PushButton;
                    objInfo = btn;
                    buttonInfos.Add(btn);
                }
                else if (objectId.HasAnyFlag(DeviceObjectTypeFlags.Axis | DeviceObjectTypeFlags.AbsoluteAxis | DeviceObjectTypeFlags.RelativeAxis))
                {
                    if (axisInfos.Count == CustomGamePadStateRaw.MaxSupportedAxes)
                    {
                        // Maximum amount of supported axes reached, don't register any more
                        continue;
                    }

                    var axis = new GamePadAxisInfo();
                    objInfo = axis;
                    axisInfos.Add(axis);
                }
                else if (objectId.HasFlags(DeviceObjectTypeFlags.PointOfViewController))
                {
                    if (povControllerInfos.Count == CustomGamePadStateRaw.MaxSupportedPovControllers)
                    {
                        // Maximum amount of supported pov controllers reached, don't register any more
                        continue;
                    }
                    var pov = new GamePadPovControllerInfo();
                    objInfo = pov;
                    povControllerInfos.Add(pov);
                }

                if (objInfo != null)
                {
                    objInfo.Index = obj.ObjectId.InstanceNumber;
                    objInfo.Name = obj.Name.TrimEnd('\0');
                }
            }

            buttonInfos.Sort((a, b) => a.Index.CompareTo(b.Index));
            axisInfos.Sort((a, b) => a.Index.CompareTo(b.Index));
            povControllerInfos.Sort((a, b) => a.Index.CompareTo(b.Index));

            // Allocate storage on state
            state.Buttons = new bool[buttonInfos.Count];
            state.Axes = new float[axisInfos.Count];
            state.PovControllers = new int[povControllerInfos.Count];
            
            InitializeButtonStates();
            InitializeLayout();
        }
        
        public override void Dispose()
        {
            base.Dispose();
            gamepad.Dispose();
        }

        public override string DeviceName { get; }
        public override Guid Id { get; }
        public Guid ProductId { get; }

        public override IReadOnlyList<GamePadButtonInfo> ButtonInfos => buttonInfos;
        public override IReadOnlyList<GamePadAxisInfo> AxisInfos => axisInfos;
        public override IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos => povControllerInfos;

        public override void Update(List<InputEvent> inputEvents)
        {
            try
            {
                gamepad.Acquire();
                gamepad.Poll();
                gamepad.GetCurrentState(ref state);
                for (int i = 0; i < buttonInfos.Count; i++)
                {
                    HandleButton(i, state.Buttons[i]);
                }
                for (int i = 0; i < axisInfos.Count; i++)
                {
                    if(axisInfos[i].IsBiDirectional)
                        HandleAxis(i, GamePadUtils.ClampDeadZone(state.Axes[i] * 2.0f - 1.0f, InputManager.GamePadAxisDeadZone));
                    else
                        HandleAxis(i, GamePadUtils.ClampDeadZone(state.Axes[i], InputManager.GamePadAxisDeadZone));
                }
                for (int i = 0; i < povControllerInfos.Count; i++)
                {
                    float v = state.PovControllers[i]/36000.0f;
                    HandlePovController(i, v, state.PovControllers[i] >= 0);
                }
            }
            catch (SharpDXException)
            {
                Dispose();
            }

            base.Update(inputEvents);
        }
    }
}
#endif