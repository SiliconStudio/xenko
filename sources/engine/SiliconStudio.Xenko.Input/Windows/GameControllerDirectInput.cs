// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    public class GameControllerDirectInput : GameControllerDeviceBase, IDisposable
    {
        private readonly List<GameControllerButtonInfo> buttonInfos = new List<GameControllerButtonInfo>();
        private readonly List<GameControllerAxisInfo> axisInfos = new List<GameControllerAxisInfo>();
        private readonly List<PovControllerInfo> povControllerInfos = new List<PovControllerInfo>();

        private CustomGamePad gamepad;
        private CustomGamePadState state = new CustomGamePadState();

        public GameControllerDirectInput(DirectInput directInput, DeviceInstance instance)
        {
            DeviceName = instance.InstanceName.TrimEnd('\0');
            Id = instance.InstanceGuid;
            ProductId = instance.ProductGuid;
            gamepad = new CustomGamePad(directInput, instance.InstanceGuid);
            var objects = gamepad.GetObjects();
            foreach (var obj in objects)
            {
                var objectId = obj.ObjectId;
                GameControllerObjectInfo objInfo = null;
                if (objectId.HasAnyFlag(DeviceObjectTypeFlags.Button | DeviceObjectTypeFlags.PushButton | DeviceObjectTypeFlags.ToggleButton))
                {
                    if (buttonInfos.Count == CustomGamePadStateRaw.MaxSupportedButtons)
                    {
                        // Maximum amount of supported buttons reached, don't register any more
                        continue;
                    }
                    var btn = new GameControllerButtonInfo();
                    btn.Type = objectId.HasFlags(DeviceObjectTypeFlags.ToggleButton) ? GameControllerButtonType.ToggleButton : GameControllerButtonType.PushButton;
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

                    var axis = new GameControllerAxisInfo();
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
                    var pov = new PovControllerInfo();
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
        }
        
        public void Dispose()
        {
            gamepad.Dispose();
            if(Disconnected == null)
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
                    HandleAxis(i, GameControllerUtils.ClampDeadZone(state.Axes[i] * 2.0f - 1.0f, InputManager.GameControllerAxisDeadZone));
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