// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    internal class GameControllerDirectInput : GameControllerDeviceBase, IDisposable
    {
        private static readonly Dictionary<Guid, int> GuidToAxisOffsets =new Dictionary<Guid, int>
        {
            [ObjectGuid.XAxis] = 0,
            [ObjectGuid.YAxis] = 1,
            [ObjectGuid.ZAxis] = 2,
            [ObjectGuid.RxAxis] = 3,
            [ObjectGuid.RyAxis] = 4,
            [ObjectGuid.RzAxis] = 5,
            [ObjectGuid.Slider] = 6,
        };

        private readonly List<GameControllerButtonInfo> buttonInfos = new List<GameControllerButtonInfo>();
        private readonly List<DirectInputAxisInfo> axisInfos = new List<DirectInputAxisInfo>();
        private readonly List<PovControllerInfo> povControllerInfos = new List<PovControllerInfo>();
        
        //private DirectInputGameController gamepad;
        private readonly DirectInputJoystick joystick;
        private DirectInputState state = new DirectInputState();

        public GameControllerDirectInput(InputSourceWindowsDirectInput source, DirectInput directInput, DeviceInstance instance)
        {
            Source = source;
            Name = instance.InstanceName.TrimEnd('\0');
            Id = instance.InstanceGuid;
            ProductId = instance.ProductGuid;
            joystick = new DirectInputJoystick(directInput, instance.InstanceGuid);
            var objects = joystick.GetObjects();

            int sliderCount = 0;
            foreach (var obj in objects)
            {
                var objectId = obj.ObjectId;
                string objectName = obj.Name.TrimEnd('\0');
                
                GameControllerObjectInfo objInfo = null;
                if (objectId.HasAnyFlag(DeviceObjectTypeFlags.Button | DeviceObjectTypeFlags.PushButton | DeviceObjectTypeFlags.ToggleButton))
                {
                    var btn = new GameControllerButtonInfo();
                    btn.Type = objectId.HasFlags(DeviceObjectTypeFlags.ToggleButton) ? GameControllerButtonType.ToggleButton : GameControllerButtonType.PushButton;
                    objInfo = btn;
                    buttonInfos.Add(btn);
                }
                else if (objectId.HasAnyFlag(DeviceObjectTypeFlags.Axis | DeviceObjectTypeFlags.AbsoluteAxis | DeviceObjectTypeFlags.RelativeAxis))
                {
                    var axis = new DirectInputAxisInfo();
                    if (!GuidToAxisOffsets.TryGetValue(obj.ObjectType, out axis.Offset))
                    {
                        // Axis that should not be used, since it does not map to a valid object guid
                        continue;
                    }

                    // All objects after x/y/z and x/y/z rotation are sliders
                    if (obj.ObjectType == ObjectGuid.Slider)
                        axis.Offset += sliderCount++;
                    
                    objInfo = axis;
                    axisInfos.Add(axis);
                }
                else if (objectId.HasFlags(DeviceObjectTypeFlags.PointOfViewController))
                {
                    var pov = new PovControllerInfo();
                    objInfo = pov;
                    povControllerInfos.Add(pov);
                }

                if (objInfo != null)
                {
                    objInfo.Name = objectName;
                }
            }
            
            // Sort axes, buttons and hats do not need to be sorted
            axisInfos.Sort((a, b) => a.Offset.CompareTo(b.Offset));

            InitializeButtonStates();
        }

        public void Dispose()
        {
            joystick.Dispose();
            if (Disconnected == null)
                throw new InvalidOperationException("Something should handle controller disconnect");

            Disconnected.Invoke(this, null);
        }

        public override string Name { get; }

        public override Guid Id { get; }

        public override Guid ProductId { get; }

        public override IReadOnlyList<GameControllerButtonInfo> ButtonInfos => buttonInfos;

        public override IReadOnlyList<GameControllerAxisInfo> AxisInfos => axisInfos;

        public override IReadOnlyList<PovControllerInfo> PovControllerInfos => povControllerInfos;

        public override IInputSource Source { get; }

        public event EventHandler Disconnected;
        
        /// <summary>
        /// Applies a deadzone to an axis input value
        /// </summary>
        /// <param name="value">The axis input value</param>
        /// <param name="deadZone">The deadzone treshold</param>
        /// <returns>The axis value with the applied deadzone</returns>
        public static float ClampDeadZone(float value, float deadZone)
        {
            if (value > 0.0f)
            {
                value -= deadZone;
                if (value < 0.0f)
                {
                    value = 0.0f;
                }
            }
            else
            {
                value += deadZone;
                if (value > 0.0f)
                {
                    value = 0.0f;
                }
            }

            // Renormalize the value according to the dead zone
            value = value / (1.0f - deadZone);
            return value < -1.0f ? -1.0f : value > 1.0f ? 1.0f : value;
        }

        public override void Update(List<InputEvent> inputEvents)
        {
            try
            {
                joystick.Acquire();
                joystick.Poll();
                joystick.GetCurrentState(ref state);

                for (int i = 0; i < buttonInfos.Count; i++)
                {
                    HandleButton(i, state.Buttons[i]);
                }

                for (int i = 0; i < axisInfos.Count; i++)
                {
                    HandleAxis(i, ClampDeadZone(state.Axes[axisInfos[i].Offset] * 2.0f - 1.0f, InputManager.GameControllerAxisDeadZone));
                }

                for (int i = 0; i < povControllerInfos.Count; i++)
                {
                    int povController = state.PovControllers[i];
                    float v = povController / 36000.0f;
                    HandlePovController(i, v, povController >= 0);
                }
            }
            catch (SharpDXException)
            {
                Dispose();
            }

            base.Update(inputEvents);
        }

        public class DirectInputAxisInfo : GameControllerAxisInfo
        {
            public int Offset;
        }
    }
}

#endif