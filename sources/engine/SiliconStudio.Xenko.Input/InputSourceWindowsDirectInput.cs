// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DirectInput;
using SiliconStudio.Xenko.Native.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    public static class DeviceObjectIdExtensions
    {
        public static bool HasFlags(this DeviceObjectId objectId, DeviceObjectTypeFlags flags)
        {
            return ((int)objectId.Flags & (int)flags) == (int)flags;
        }
        public static bool HasAnyFlag(this DeviceObjectId objectId, DeviceObjectTypeFlags flags)
        {
            return ((int)objectId.Flags & (int)flags) != 0;
        }
    }

    public class GamePadDirectInput : GamePadDeviceBase
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
        private CustomGamePad gamepad;
        private CustomGamePadState state = new CustomGamePadState();

        public GamePadDirectInput(DirectInput directInput, DeviceInstance instance)
        {
            deviceName = instance.InstanceName.TrimEnd('\0');
            deviceId = instance.InstanceGuid;
            gamepad = new CustomGamePad(directInput, instance.InstanceGuid);
            var objects = gamepad.GetObjects();
            foreach(var obj in objects)
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
                    if(objectId.HasFlags(DeviceObjectTypeFlags.ToggleButton))
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

            // Allocate storage on state
            state.Buttons = new bool[buttonInfos.Count];
            state.Axes = new float[axisInfos.Count];
            state.PovControllers = new float[povControllerInfos.Count];
            InitializeButtonStates();
        }
        public override void Update()
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
                    HandleAxis(i, state.Axes[i]);
                }
                for (int i = 0; i < povControllerInfos.Count; i++)
                {
                    HandlePovController(i, state.PovControllers[i]);
                }
            }
            catch (SharpDXException ex)
            {
                // TODO: Disconnect
                OnDisconnect?.Invoke(this, null);
                Dispose();
            }

            base.Update();
        }
        public override void Dispose()
        {
            base.Dispose();
            gamepad.Dispose();
        }
    }

    public class InputSourceWindowsDirectInput : InputSourceBase
    {
        private DirectInput directInput;

        // TODO: Merge with InputSourceBase maybe
        private Dictionary<Guid, GamePadDirectInput> registeredDevices = new Dictionary<Guid, GamePadDirectInput>();
        private HashSet<Guid> devicesToRemove = new HashSet<Guid>();

        public override void Initialize(InputManager inputManager)
        {
            directInput = new DirectInput();
            ScanDevices();
        }

        public override void Update()
        {
            // Notify event listeners of device removals
            foreach (var deviceIdToRemove in devicesToRemove)
            {
                var gamePad = registeredDevices[deviceIdToRemove];
                UnregisterDevice(gamePad);
                registeredDevices.Remove(deviceIdToRemove);

                if(gamePad.Connected)
                    gamePad.Dispose();
            }
            devicesToRemove.Clear();

            // Scan for new devices
            // TODO: move to hardware change detection event
            ScanDevices();
        }

        /// <summary>
        /// Scans for new devices
        /// </summary>
        public void ScanDevices()
        {
            var connectedDevices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            foreach(var device in connectedDevices)
            {
                if (!registeredDevices.ContainsKey(device.InstanceGuid))
                {
                    OpenGamePad(device);
                }
            }
        }

        /// <summary>
        /// Opens a new gamepad
        /// </summary>
        /// <param name="instance">The gamepad</param>
        public void OpenGamePad(DeviceInstance instance)
        {
            // Ignore XInput devices since they are handled by XInput
            if (XInputChecker.IsXInputDevice(ref instance.InstanceGuid))
                return;

            if (registeredDevices.ContainsKey(instance.InstanceGuid))
                throw new InvalidOperationException($"GamePad already opened {instance.InstanceGuid}/{instance.InstanceName}");

            var newGamepad = new GamePadDirectInput(directInput, instance);
            newGamepad.OnDisconnect += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(newGamepad.Id);
            };
            registeredDevices.Add(newGamepad.Id, newGamepad);
            RegisterDevice(newGamepad);
        }

        public override void Dispose()
        {
            base.Dispose();

            // Dispose all the gamepads
            foreach (var pair in registeredDevices)
            {
                pair.Value.Dispose();
            }

            // Dispose DirectInput
            directInput.Dispose();
        }
    }
}
#endif