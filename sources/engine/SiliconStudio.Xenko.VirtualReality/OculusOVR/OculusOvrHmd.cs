#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using System;
using SharpDX.Direct3D11;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using CommandList = SiliconStudio.Xenko.Graphics.CommandList;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OculusOvrHmd : VRDevice
    {
        private static bool initDone;
        //private static readonly Guid dx12ResourceGuid = new Guid("696442be-a72e-4059-bc79-5b5c98040fad");
        private static readonly Guid Dx11Texture2DGuid = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

        private IntPtr ovrSession;
        private Texture[] textures;

        private OculusTouchController leftHandController;
        private OculusTouchController rightHandController;

        internal OculusOvrHmd(IServiceRegistry registry) : base(registry)
        {
        }

        public override void Initialize(GraphicsDevice device, bool depthStencilResource = false, bool requireMirror = false)
        {
            long adapterId;
            ovrSession = OculusOvr.CreateSessionDx(out adapterId);
            //Game.GraphicsDeviceManager.RequiredAdapterUid = adapterId.ToString();

            int texturesCount;
            if (!OculusOvr.CreateTexturesDx(ovrSession, device.NativeDevice.NativePointer, out texturesCount, RenderFrameScaling, requireMirror ? RenderFrameSize.Width : 0, requireMirror ? RenderFrameSize.Height : 0))
            {
                throw new Exception(OculusOvr.GetError());
            }

            if (requireMirror)
            {
                var mirrorTex = OculusOvr.GetMirrorTexture(ovrSession, Dx11Texture2DGuid);
                MirrorTexture = new Texture(device);
                MirrorTexture.InitializeFrom(new Texture2D(mirrorTex), false);
            }

            textures = new Texture[texturesCount];
            for (var i = 0; i < texturesCount; i++)
            {

                var ptr = OculusOvr.GetTextureDx(ovrSession, Dx11Texture2DGuid, i);
                if (ptr == IntPtr.Zero)
                {
                    throw new Exception(OculusOvr.GetError());
                }

                textures[i] = new Texture(device);
                textures[i].InitializeFrom(new Texture2D(ptr), false);
            }

            RenderFrame = Texture.New2D(device, textures[0].Width, textures[1].Height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);

            leftHandController = new OculusTouchController(TouchControllerHand.Left);
            rightHandController = new OculusTouchController(TouchControllerHand.Right);

            base.Initialize(device, requireMirror);
        }

        private OculusOvr.PosesProperties currentPoses;

        public override void Draw(GameTime gameTime)
        {
            OculusOvr.Update(ovrSession);
            OculusOvr.GetPosesProperties(ovrSession, ref currentPoses);
            leftHandController.Update(ref currentPoses);
            rightHandController.Update(ref currentPoses);
        }

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);

        public override Texture RenderFrame { get; protected set; }

        public override Texture RenderFrameDepthStencil { get; protected set; }

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.4f;

        public override DeviceState State
        {
            get
            {
                var deviceStatus = OculusOvr.GetStatus(ovrSession);
                if(deviceStatus.DisplayLost || !deviceStatus.HmdPresent) return DeviceState.Invalid;
                if(deviceStatus.HmdMounted && deviceStatus.IsVisible) return DeviceState.Valid;
                return DeviceState.OutOfRange;
            }
        }

        public override Vector3 HeadPosition => currentPoses.PosHead;

        public override Quaternion HeadRotation => currentPoses.RotHead;

        public override Vector3 HeadLinearVelocity => currentPoses.LinearVelocityHead;

        public override Vector3 HeadAngularVelocity => currentPoses.AngularVelocityHead;

        public override TouchController LeftHand => leftHandController;

        public override TouchController RightHand => rightHandController;

        public override bool CanInitialize
        {
            get
            {
                if (initDone) return true;
                initDone = OculusOvr.Startup();
                if (initDone)
                {
                    long deviceId;
                    var tempSession = OculusOvr.CreateSessionDx(out deviceId);
                    if (tempSession != IntPtr.Zero)
                    {
                        OculusOvr.DestroySession(tempSession);
                        initDone = true;
                    }
                    else
                    {
                        initDone = false;
                    }
                }
                return initDone;
            }
        }

        public override void Recenter()
        {
            OculusOvr.Recenter(ovrSession);
        }

        public override void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection)
        {
            var frameProperties = new OculusOvr.FrameProperties
            {
                Near = near,
                Far = far
            };
            OculusOvr.GetFrameProperties(ovrSession, ref frameProperties);

            var camRot = Quaternion.RotationMatrix(cameraRotation);

            if (eye == Eyes.Left)
            {
                projection = frameProperties.ProjLeft;

                var posL = cameraPosition + Vector3.Transform(frameProperties.PosLeft * ViewScaling, camRot);
                var rotL = Matrix.RotationQuaternion(frameProperties.RotLeft) * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
                var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotL);
                var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotL);
                view = Matrix.LookAtRH(posL, posL + finalForward, finalUp);
            }
            else
            {
                projection = frameProperties.ProjRight;

                var posL = cameraPosition + Vector3.Transform(frameProperties.PosRight * ViewScaling, camRot);
                var rotL = Matrix.RotationQuaternion(frameProperties.RotRight) * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
                var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotL);
                var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotL);
                view = Matrix.LookAtRH(posL, posL + finalForward, finalUp);
            }
        }

        public override void Commit(CommandList commandList)
        {
            var index = OculusOvr.GetCurrentTargetIndex(ovrSession);
            commandList.Copy(RenderFrame, textures[index]);
            OculusOvr.CommitFrame(ovrSession, null, 0);
        }
    }
}

#endif