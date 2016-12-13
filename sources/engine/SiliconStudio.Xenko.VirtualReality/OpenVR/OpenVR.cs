using System;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using Valve.VR;

namespace SiliconStudio.Xenko.VirtualReality
{
    public enum ControllerState
    {
        Invalid,
        OutOfRange,
        Valid
    }

    public static class OpenVR
    {
        public class Controller
        {
            // This helper can be used in a variety of ways.  Beware that indices may change
            // as new devices are dynamically added or removed, controllers are physically
            // swapped between hands, arms crossed, etc.
            public enum DeviceRelation
            {
                First,
                // radially
                Leftmost,
                Rightmost,
                // distance
                FarthestLeft,
                FarthestRight,
            }

            public static int GetDeviceIndex(DeviceRelation relation) // use -1 for absolute tracking space
            {
                var result = -1;

                Matrix hmdPoseInvert;
                GetHeadPose(out hmdPoseInvert);
                hmdPoseInvert.Invert();
                var invForm = hmdPoseInvert;

                var system = Valve.VR.OpenVR.System;
                if (system == null)
                    return result;

                var best = -float.MaxValue;
                var currentIndex = 0;
                for (var i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++)
                {
                    if (system.GetTrackedDeviceClass((uint)i) != ETrackedDeviceClass.Controller)
                        continue;

                    var xenkoIndex = currentIndex;
                    currentIndex++;

                    if (relation == DeviceRelation.First)
                        return i;

                    float score;

                    Matrix devicePose;
                    if (GetControllerPose(xenkoIndex, out devicePose) != ControllerState.Valid)
                    {
                        return -1;
                    }

                    var pos = invForm * devicePose;
                    if (relation == DeviceRelation.FarthestRight)
                    {
                        score = pos.TranslationVector.X;
                    }
                    else if (relation == DeviceRelation.FarthestLeft)
                    {
                        score = -pos.TranslationVector.X;
                    }
                    else
                    {
                        var dir = new Vector3(pos.TranslationVector.X, 0.0f, pos.TranslationVector.Z);
                        dir.Normalize();
                        var dot = Vector3.Dot(dir, -Vector3.UnitZ);
                        var cross = Vector3.Cross(dir, -Vector3.UnitZ);
                        if (relation == DeviceRelation.Leftmost)
                        {
                            score = (cross.Y > 0.0f) ? 2.0f - dot : dot;
                        }
                        else
                        {
                            score = (cross.Y < 0.0f) ? 2.0f - dot : dot;
                        }
                    }

                    if (score > best)
                    {
                        result = xenkoIndex;
                        best = score;
                    }
                }

                return result;
            }

            public class ButtonMask
            {
                public const ulong System = (1ul << (int)EVRButtonId.k_EButton_System); // reserved
                public const ulong ApplicationMenu = (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu);
                public const ulong Grip = (1ul << (int)EVRButtonId.k_EButton_Grip);
                public const ulong Axis0 = (1ul << (int)EVRButtonId.k_EButton_Axis0);
                public const ulong Axis1 = (1ul << (int)EVRButtonId.k_EButton_Axis1);
                public const ulong Axis2 = (1ul << (int)EVRButtonId.k_EButton_Axis2);
                public const ulong Axis3 = (1ul << (int)EVRButtonId.k_EButton_Axis3);
                public const ulong Axis4 = (1ul << (int)EVRButtonId.k_EButton_Axis4);
                public const ulong Touchpad = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad);
                public const ulong Trigger = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);
            }

            public enum ButtonId
            {
                ButtonSystem = 0,
                ButtonApplicationMenu = 1,
                ButtonGrip = 2,
                ButtonDPadLeft = 3,
                ButtonDPadUp = 4,
                ButtonDPadRight = 5,
                ButtonDPadDown = 6,
                ButtonA = 7,
                ButtonProximitySensor = 31,
                ButtonAxis0 = 32,
                ButtonAxis1 = 33,
                ButtonAxis2 = 34,
                ButtonAxis3 = 35,
                ButtonAxis4 = 36,
                ButtonSteamVrTouchpad = 32,
                ButtonSteamVrTrigger = 33,
                ButtonDashboardBack = 2,
                ButtonMax = 64,
            }

            public Controller(int controllerIndex)
            {
                var currentIndex = 0;
                for (uint index = 0; index < DevicePoses.Length; index++)
                {
                    if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(index) == ETrackedDeviceClass.Controller)
                    {
                        if (currentIndex == controllerIndex)
                        {
                            ControllerIndex = index;
                            break;
                        }
                        currentIndex++;
                    }
                }
            }

            internal uint ControllerIndex;
            internal VRControllerState_t State;
            internal VRControllerState_t PreviousState;

            public bool GetPress(ulong buttonMask) { return (State.ulButtonPressed & buttonMask) != 0; }

            public bool GetPressDown(ulong buttonMask) { return (State.ulButtonPressed & buttonMask) != 0 && (PreviousState.ulButtonPressed & buttonMask) == 0; }

            public bool GetPressUp(ulong buttonMask) { return (State.ulButtonPressed & buttonMask) == 0 && (PreviousState.ulButtonPressed & buttonMask) != 0; }

            public bool GetPress(ButtonId buttonId) { return GetPress(1ul << (int)buttonId); }

            public bool GetPressDown(ButtonId buttonId) { return GetPressDown(1ul << (int)buttonId); }

            public bool GetPressUp(ButtonId buttonId) { return GetPressUp(1ul << (int)buttonId); }

            public bool GetTouch(ulong buttonMask) { return (State.ulButtonTouched & buttonMask) != 0; }

            public bool GetTouchDown(ulong buttonMask) { return (State.ulButtonTouched & buttonMask) != 0 && (PreviousState.ulButtonTouched & buttonMask) == 0; }

            public bool GetTouchUp(ulong buttonMask) { return (State.ulButtonTouched & buttonMask) == 0 && (PreviousState.ulButtonTouched & buttonMask) != 0; }

            public bool GetTouch(ButtonId buttonId) { return GetTouch(1ul << (int)buttonId); }

            public bool GetTouchDown(ButtonId buttonId) { return GetTouchDown(1ul << (int)buttonId); }

            public bool GetTouchUp(ButtonId buttonId) { return GetTouchUp(1ul << (int)buttonId); }

            public Vector2 GetAxis(ButtonId buttonId = ButtonId.ButtonSteamVrTouchpad)
            {               
                var axisId = (uint)buttonId - (uint)EVRButtonId.k_EButton_Axis0;
                switch (axisId)
                {
                    case 0: return new Vector2(State.rAxis0.x, State.rAxis0.y);
                    case 1: return new Vector2(State.rAxis1.x, State.rAxis1.y);
                    case 2: return new Vector2(State.rAxis2.x, State.rAxis2.y);
                    case 3: return new Vector2(State.rAxis3.x, State.rAxis3.y);
                    case 4: return new Vector2(State.rAxis4.x, State.rAxis4.y);
                }
                return Vector2.Zero;
            }

            public void Update()
            {
                PreviousState = State;
                Valve.VR.OpenVR.System.GetControllerState(ControllerIndex, ref State, (uint)Utilities.SizeOf<VRControllerState_t>());
            }
        }

        private static readonly TrackedDevicePose_t[] DevicePoses = new TrackedDevicePose_t[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];
        private static readonly TrackedDevicePose_t[] GamePoses = new TrackedDevicePose_t[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];

        static OpenVR()
        {
            NativeLibrary.PreloadLibrary("openvr_api.dll");
        }

        public static void Init()
        {
            var err = EVRInitError.None;
            Valve.VR.OpenVR.Init(ref err);
            if (err != EVRInitError.None)
            {
                throw new Exception("Failed to initialize OpenVR.");
            }
        }

        public static void Submit(int eyeIndex, Texture texture, RectangleF viewport)
        {
            var tex = new Texture_t
            {
                eType = EGraphicsAPIConvention.API_DirectX,
                eColorSpace = EColorSpace.Auto,
                handle = texture.NativeResource.NativePointer
            };
            var bounds = new VRTextureBounds_t
            {
                uMin = viewport.X,
                uMax = viewport.Width,
                vMin = viewport.Y,
                vMax = viewport.Height
            };

            Valve.VR.OpenVR.Compositor.Submit(eyeIndex == 0 ? EVREye.Eye_Left : EVREye.Eye_Right, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);
        }

        public static void GetEyeToHead(int eyeIndex, out Matrix pose)
        {
            GetEyeToHeadUnsafe(eyeIndex, out pose);
        }

        private static unsafe void GetEyeToHeadUnsafe(int eyeIndex, out Matrix pose)
        {
            pose = Matrix.Identity;
            var eye = eyeIndex == 0 ? EVREye.Eye_Left : EVREye.Eye_Right;
            var eyeToHead = Valve.VR.OpenVR.System.GetEyeToHeadTransform(eye);
            Utilities.CopyMemory((IntPtr)Interop.Fixed(ref pose), (IntPtr)Interop.Fixed(ref eyeToHead), Utilities.SizeOf<HmdMatrix34_t>());
        }

        public static void UpdatePoses()
        {
            Valve.VR.OpenVR.Compositor.WaitGetPoses(DevicePoses, GamePoses);
        }

        public static ControllerState GetControllerPose(int controllerIndex, out Matrix pose)
        {
            return GetControllerPoseUnsafe(controllerIndex, out pose);
        }

        private static unsafe ControllerState GetControllerPoseUnsafe(int controllerIndex, out Matrix pose)
        {
            var currentIndex = 0;
            pose = Matrix.Identity;
            for (uint index = 0; index < DevicePoses.Length; index++)
            {
                if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(index) == ETrackedDeviceClass.Controller)
                {
                    if (currentIndex == controllerIndex)
                    {
                        Utilities.CopyMemory((IntPtr)Interop.Fixed(ref pose), (IntPtr)Interop.Fixed(ref DevicePoses[index].mDeviceToAbsoluteTracking), Utilities.SizeOf<HmdMatrix34_t>());

                        var state = ControllerState.Invalid;
                        if (DevicePoses[index].bDeviceIsConnected && DevicePoses[index].bPoseIsValid)
                        {
                            state = ControllerState.Valid;
                        }
                        else if (DevicePoses[index].bDeviceIsConnected && !DevicePoses[index].bPoseIsValid && DevicePoses[index].eTrackingResult == ETrackingResult.Running_OutOfRange)
                        {
                            state = ControllerState.OutOfRange;
                        }

                        return state;
                    }
                    currentIndex++;
                }
            }

            return ControllerState.Invalid;
        }

        public static ControllerState GetHeadPose(out Matrix pose)
        {
            return GetHeadPoseUnsafe(out pose);
        }

        private static unsafe ControllerState GetHeadPoseUnsafe(out Matrix pose)
        {
            pose = Matrix.Identity;
            for (uint index = 0; index < DevicePoses.Length; index++)
            {
                if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(index) == ETrackedDeviceClass.HMD)
                {
                    Utilities.CopyMemory((IntPtr)Interop.Fixed(ref pose), (IntPtr)Interop.Fixed(ref DevicePoses[index].mDeviceToAbsoluteTracking), Utilities.SizeOf<HmdMatrix34_t>());

                    var state = ControllerState.Invalid;
                    if (DevicePoses[index].bDeviceIsConnected && DevicePoses[index].bPoseIsValid)
                    {
                        state = ControllerState.Valid;
                    }
                    else if (DevicePoses[index].bDeviceIsConnected && !DevicePoses[index].bPoseIsValid && DevicePoses[index].eTrackingResult == ETrackingResult.Running_OutOfRange)
                    {
                        state = ControllerState.OutOfRange;
                    }

                    return state;
                }
            }

            return ControllerState.Invalid;
        }

        public static void GetProjection(int eyeIndex, float near, float far, out Matrix projection)
        {
            GetProjectionUnsafe(eyeIndex, near, far, out projection);
        }

        private static unsafe void GetProjectionUnsafe(int eyeIndex, float near, float far, out Matrix projection)
        {
            projection = Matrix.Identity;
            var eye = eyeIndex == 0 ? EVREye.Eye_Left : EVREye.Eye_Right;
            var proj = Valve.VR.OpenVR.System.GetProjectionMatrix(eye, near, far, EGraphicsAPIConvention.API_DirectX);
            Utilities.CopyMemory((IntPtr)Interop.Fixed(ref projection), (IntPtr)Interop.Fixed(ref proj), Utilities.SizeOf<Matrix>());
        }
    }
}
