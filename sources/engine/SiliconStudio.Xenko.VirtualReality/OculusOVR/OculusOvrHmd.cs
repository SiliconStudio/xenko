#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using System;
using SharpDX.Direct3D11;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OculusOvrHmd : Hmd
    {
        private static bool initDone;
        //private static readonly Guid dx12ResourceGuid = new Guid("696442be-a72e-4059-bc79-5b5c98040fad");
        private static readonly Guid Dx11Texture2DGuid = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

        private IntPtr ovrSession;
        private Texture[] textures;

        public OculusOvrHmd(IServiceRegistry registry) : base(registry)
        {         
        }

        public override void Initialize(Entity cameraRoot, CameraComponent leftCamera, CameraComponent rightCamera, bool requireMirror = false)
        {
            long adapterId;
            ovrSession = OculusOvr.CreateSessionDx(out adapterId);
            //Game.GraphicsDeviceManager.RequiredAdapterUid = adapterId.ToString();

            int texturesCount;
            if (!OculusOvr.CreateTexturesDx(ovrSession, GraphicsDevice.NativeDevice.NativePointer, out texturesCount, RenderFrameScaling, requireMirror ? RenderFrameSize.Width : 0, requireMirror ? RenderFrameSize.Height : 0))
            {
                throw new Exception(OculusOvr.GetError());
            }

            if (requireMirror)
            {
                var mirrorTex = OculusOvr.GetMirrorTexture(ovrSession, Dx11Texture2DGuid);
                MirrorTexture = new Texture(GraphicsDevice);
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

                textures[i] = new Texture(GraphicsDevice);
                textures[i].InitializeFrom(new Texture2D(ptr), false);
            }

            RenderFrameProvider = new DirectRenderFrameProvider(RenderFrame.FromTexture(Texture.New2D(GraphicsDevice, textures[0].Width, textures[1].Height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource)));

            var compositor = (SceneGraphicsCompositorLayers)Game.SceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
            compositor.Master.Add(new SceneDelegateRenderer((x, y) =>
            {
                var index = OculusOvr.GetCurrentTargetIndex(ovrSession);
                x.CommandList.Copy(RenderFrameProvider.RenderFrame.RenderTargets[0], textures[index]);

                OculusOvr.CommitFrame(ovrSession, null, 0);
            }));

            leftCamera.UseCustomProjectionMatrix = true;
            rightCamera.UseCustomProjectionMatrix = true;
            leftCamera.UseCustomViewMatrix = true;
            rightCamera.UseCustomViewMatrix = true;
            leftCamera.NearClipPlane *= ViewScaling;
            rightCamera.NearClipPlane *= ViewScaling;

            base.Initialize(cameraRoot, leftCamera, rightCamera, requireMirror);
        }

        public override void Draw(GameTime gameTime)
        {
            var frameProperties = new OculusOvr.FrameProperties
            {
                Near = LeftCameraComponent.NearClipPlane,
                Far = LeftCameraComponent.FarClipPlane
            };
            OculusOvr.PrepareRender(ovrSession, ref frameProperties);

            Vector3 scale, camPos;
            Quaternion camRot;

            //have to make sure it's updated now
            CameraRootEntity.Transform.UpdateWorldMatrix();
            CameraRootEntity.Transform.WorldMatrix.Decompose(out scale, out camRot, out camPos);

            LeftCameraComponent.ProjectionMatrix = frameProperties.ProjLeft;

            var posL = camPos + Vector3.Transform(frameProperties.PosLeft * ViewScaling, camRot);
            var rotL = Matrix.RotationQuaternion(frameProperties.RotLeft) * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotL);
            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotL);
            var view = Matrix.LookAtRH(posL, posL + finalForward, finalUp);
            LeftCameraComponent.ViewMatrix = view;

            RightCameraComponent.ProjectionMatrix = frameProperties.ProjRight;

            var posR = camPos + Vector3.Transform(frameProperties.PosRight * ViewScaling, camRot);
            var rotR = Matrix.RotationQuaternion(frameProperties.RotRight) * Matrix.Scaling(ViewScaling) *  Matrix.RotationQuaternion(camRot);
            finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotR);
            finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotR);
            view = Matrix.LookAtRH(posR, posR + finalForward, finalUp);
            RightCameraComponent.ViewMatrix = view;

            base.Draw(gameTime);
        }

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);

        public override DirectRenderFrameProvider RenderFrameProvider { get; protected set; }

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
    }
}

#endif