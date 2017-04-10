#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using System;
using SharpDX.Direct3D11;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using Valve.VR;

namespace SiliconStudio.Xenko.VirtualReality
{
    public static class OpenVR
    {
        public class Controller
        {
            // This helper can be used in a variety of ways.  Beware that indices may change
            // as new devices are dynamically added or removed, controllers are physically
            // swapped between hands, arms crossed, etc.
            public enum Hand
            {
                Left,
                Right
            }

            public static int GetDeviceIndex(Hand hand)
            {
                var currentIndex = 0;
                for (uint index = 0; index < DevicePoses.Length; index++)
                {
                    if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(index) == ETrackedDeviceClass.Controller)
                    {
                        if (hand == Hand.Left && Valve.VR.OpenVR.System.GetControllerRoleForTrackedDeviceIndex(index) == ETrackedControllerRole.LeftHand)
                        {
                            return currentIndex;
                        }

                        if (hand == Hand.Right && Valve.VR.OpenVR.System.GetControllerRoleForTrackedDeviceIndex(index) == ETrackedControllerRole.RightHand)
                        {
                            return currentIndex;
                        }

                        currentIndex++;
                    }
                }

                return -1;
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

        public static bool InitDone = false;

        public static bool Init()
        {
            var err = EVRInitError.None;
            Valve.VR.OpenVR.Init(ref err);
            if (err != EVRInitError.None)
            {
                return false;
            }

            InitDone = true;

            //this makes the camera behave like oculus rift default!
            Valve.VR.OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);

            return true;
        }

        public static bool Submit(int eyeIndex, Texture texture, ref RectangleF viewport)
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

            return Valve.VR.OpenVR.Compositor.Submit(eyeIndex == 0 ? EVREye.Eye_Left : EVREye.Eye_Right, ref tex, ref bounds, EVRSubmitFlags.Submit_Default) == EVRCompositorError.None;
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

        public static DeviceState GetControllerPose(int controllerIndex, out Matrix pose, out Vector3 velocity, out Vector3 angVelocity)
        {
            return GetControllerPoseUnsafe(controllerIndex, out pose, out velocity, out angVelocity);
        }

        private static unsafe DeviceState GetControllerPoseUnsafe(int controllerIndex, out Matrix pose, out Vector3 velocity, out Vector3 angVelocity)
        {
            var currentIndex = 0;

            pose = Matrix.Identity;
            velocity = Vector3.Zero;
            angVelocity = Vector3.Zero;

            for (uint index = 0; index < DevicePoses.Length; index++)
            {
                if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(index) == ETrackedDeviceClass.Controller)
                {
                    if (currentIndex == controllerIndex)
                    {
                        Utilities.CopyMemory((IntPtr)Interop.Fixed(ref pose), (IntPtr)Interop.Fixed(ref DevicePoses[index].mDeviceToAbsoluteTracking), Utilities.SizeOf<HmdMatrix34_t>());
                        Utilities.CopyMemory((IntPtr)Interop.Fixed(ref velocity), (IntPtr)Interop.Fixed(ref DevicePoses[index].vVelocity), Utilities.SizeOf<HmdVector3_t>());
                        Utilities.CopyMemory((IntPtr)Interop.Fixed(ref angVelocity), (IntPtr)Interop.Fixed(ref DevicePoses[index].vAngularVelocity), Utilities.SizeOf<HmdVector3_t>());

                        var state = DeviceState.Invalid;
                        if (DevicePoses[index].bDeviceIsConnected && DevicePoses[index].bPoseIsValid)
                        {
                            state = DeviceState.Valid;
                        }
                        else if (DevicePoses[index].bDeviceIsConnected && !DevicePoses[index].bPoseIsValid && DevicePoses[index].eTrackingResult == ETrackingResult.Running_OutOfRange)
                        {
                            state = DeviceState.OutOfRange;
                        }

                        return state;
                    }
                    currentIndex++;
                }
            }

            return DeviceState.Invalid;
        }

        public static DeviceState GetHeadPose(out Matrix pose, out Vector3 linearVelocity, out Vector3 angularVelocity)
        {
            return GetHeadPoseUnsafe(out pose, out linearVelocity, out angularVelocity);
        }

        private static unsafe DeviceState GetHeadPoseUnsafe(out Matrix pose, out Vector3 linearVelocity, out Vector3 angularVelocity)
        {
            pose = Matrix.Identity;
            linearVelocity = Vector3.Zero;
            angularVelocity = Vector3.Zero;
            for (uint index = 0; index < DevicePoses.Length; index++)
            {
                if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(index) == ETrackedDeviceClass.HMD)
                {
                    Utilities.CopyMemory((IntPtr)Interop.Fixed(ref pose), (IntPtr)Interop.Fixed(ref DevicePoses[index].mDeviceToAbsoluteTracking), Utilities.SizeOf<HmdMatrix34_t>());
                    Utilities.CopyMemory((IntPtr)Interop.Fixed(ref linearVelocity), (IntPtr)Interop.Fixed(ref DevicePoses[index].vVelocity), Utilities.SizeOf<HmdVector3_t>());
                    Utilities.CopyMemory((IntPtr)Interop.Fixed(ref angularVelocity), (IntPtr)Interop.Fixed(ref DevicePoses[index].vAngularVelocity), Utilities.SizeOf<HmdVector3_t>());

                    var state = DeviceState.Invalid;
                    if (DevicePoses[index].bDeviceIsConnected && DevicePoses[index].bPoseIsValid)
                    {
                        state = DeviceState.Valid;
                    }
                    else if (DevicePoses[index].bDeviceIsConnected && !DevicePoses[index].bPoseIsValid && DevicePoses[index].eTrackingResult == ETrackingResult.Running_OutOfRange)
                    {
                        state = DeviceState.OutOfRange;
                    }

                    return state;
                }
            }

            return DeviceState.Invalid;
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

        public static void ShowMirror()
        {
            Valve.VR.OpenVR.Compositor.ShowMirrorWindow();
        }

        public static void HideMirror()
        {
            Valve.VR.OpenVR.Compositor.HideMirrorWindow();
        }

        public static Texture GetMirrorTexture(GraphicsDevice device, int eyeIndex)
        {
            var nativeDevice = device.NativeDevice.NativePointer;
            var eyeTexSrv = IntPtr.Zero;
            Valve.VR.OpenVR.Compositor.GetMirrorTextureD3D11(eyeIndex == 0 ? EVREye.Eye_Left : EVREye.Eye_Right, nativeDevice, ref eyeTexSrv);
            var tex = new Texture(device);
            tex.InitializeFromImpl(new ShaderResourceView(eyeTexSrv));
            return tex;
        }
    }
}

#endif