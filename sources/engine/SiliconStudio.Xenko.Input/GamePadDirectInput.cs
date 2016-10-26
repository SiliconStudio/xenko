// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DirectInput;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GamePadDirectInput : GamePadDeviceBase
    {
        public override string DeviceName => deviceName;
        public override Guid Id => deviceId;

        public override IReadOnlyList<GamePadButtonInfo> ButtonInfos => buttonInfos;
        public override IReadOnlyList<GamePadAxisInfo> AxisInfos => axisInfos;
        public override IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos => povControllerInfos;

        private readonly List<GamePadButtonInfo> buttonInfos = new List<GamePadButtonInfo>();
        private readonly List<GamePadAxisInfo> axisInfos = new List<GamePadAxisInfo>();
        private readonly List<GamePadPovControllerInfo> povControllerInfos = new List<GamePadPovControllerInfo>();

        private string deviceName;
        private Guid deviceId;
        private Guid productId;
        private CustomGamePad gamepad;
        private CustomGamePadState state = new CustomGamePadState();

        public GamePadDirectInput(DirectInput directInput, DeviceInstance instance)
        {
            deviceName = instance.InstanceName.TrimEnd('\0');
            deviceId = instance.InstanceGuid;
            productId = instance.ProductGuid;
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
                    if (objectId.HasFlags(DeviceObjectTypeFlags.AbsoluteAxis))
                        axis.Type = GamePadAxisType.AbsoluteAxis;
                    else
                        axis.Type = GamePadAxisType.RelativeAxis;
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
        }

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
                OnDisconnect?.Invoke(this, null);
                Dispose();
            }

            base.Update(inputEvents);
        }

        public override void Dispose()
        {
            base.Dispose();
            gamepad.Dispose();
        }

        // TODO: Move controller specific mappings to class
        private static Guid guidDS4 = new Guid(0x05c4054c, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Dictionary<int, GamePadButton> ds4ButtonMapping = new Dictionary<int, GamePadButton>
        {
            { 9, GamePadButton.Start },
            { 8, GamePadButton.Back },
            { 10, GamePadButton.LeftThumb },
            { 11, GamePadButton.RightThumb },
            { 4, GamePadButton.LeftShoulder },
            { 5, GamePadButton.RightShoulder },
            { 1, GamePadButton.A },
            { 2, GamePadButton.B },
            { 0, GamePadButton.X },
            { 3, GamePadButton.Y },
        };

        private List<int> ds4AxisMapping = new List<int>
        {
            3,
            2, // L Stick
            1,
            0, // R Stick 
            5,
            4 // Triggers
        };

        public override bool GetGamePadState(ref GamePadState state)
        {
            // DS4 mapping
            if (CompareProductId(productId, guidDS4))
            {
                // Provide default GamePadState mapping
                state.Buttons = 0;

                // Pov controller 0 as DPad
                state.Buttons |= this.GetDPad(0);

                // Map buttons using ds4ButtonMap
                foreach (var map in ds4ButtonMapping)
                {
                    if (GetButton(map.Key))
                    {
                        state.Buttons |= map.Value;
                    }
                }

                // Convert axes while clamping deadzone
                state.LeftThumb = new Vector2(GamePadUtils.ClampDeadZone(GetAxis(ds4AxisMapping[0]), InputManager.GamePadAxisDeadZone), GamePadUtils.ClampDeadZone(-GetAxis(ds4AxisMapping[1]), InputManager.GamePadAxisDeadZone));
                state.RightThumb = new Vector2(GamePadUtils.ClampDeadZone(GetAxis(ds4AxisMapping[2]), InputManager.GamePadAxisDeadZone), GamePadUtils.ClampDeadZone(-GetAxis(ds4AxisMapping[3]), InputManager.GamePadAxisDeadZone));
                state.LeftTrigger = GamePadUtils.ClampDeadZone(this.GetTrigger(ds4AxisMapping[4]), InputManager.GamePadAxisDeadZone);
                state.RightTrigger = GamePadUtils.ClampDeadZone(this.GetTrigger(ds4AxisMapping[5]), InputManager.GamePadAxisDeadZone);

                return true;
            }

            // No mapping available
            return false;
        }

        private bool CompareProductId(Guid a, Guid b)
        {
            byte[] aBytes = a.ToByteArray();
            byte[] bBytes = b.ToByteArray();
            for (int i = 0; i < 4; i++)
                if (aBytes[i] != bBytes[i]) return false;
            return true;
        }
    }
}
#endif