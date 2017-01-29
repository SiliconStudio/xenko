#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OpenVrHmd : Hmd
    {
        private RectangleF leftView = new RectangleF(0.0f, 0.0f, 0.5f, 1.0f);
        private RectangleF rightView = new RectangleF(0.5f, 0.0f, 1.0f, 1.0f);
        private Texture bothEyesMirror;
        private Texture leftEyeMirror;
        private Texture rightEyeMirror;
        private DeviceState state;

        internal OpenVrHmd(IServiceRegistry registry) : base(registry)
        { 
        }

        public override bool CanInitialize => OpenVR.InitDone || OpenVR.Init();

        public override void Initialize(Entity cameraRoot, CameraComponent leftCamera, CameraComponent rightCamera, bool requireMirror = false)
        {
            var size = RenderFrameSize;
            var width = size.Width;
            var height = size.Height;
            RenderFrame = Texture.New2D(GraphicsDevice, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);

            if (requireMirror)
            {
                bothEyesMirror = Texture.New2D(GraphicsDevice, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            }

//            var compositor = (SceneGraphicsCompositorLayers)Game.SceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
//            compositor.Master.Add(new SceneDelegateRenderer((x, y) =>
//            {
//                OpenVR.Submit(0, RenderFrameProvider.RenderFrame.RenderTargets[0], ref leftView);
//                OpenVR.Submit(1, RenderFrameProvider.RenderFrame.RenderTargets[0], ref rightView);
//
//                //copy mirror
//                if (!requireMirror) return;
//
//                var wholeRegion = new ResourceRegion(0, 0, 0, width, height, 1);
//                x.CommandList.CopyRegion(leftEyeMirror, 0, wholeRegion, bothEyesMirror, 0);
//                x.CommandList.CopyRegion(rightEyeMirror, 0, wholeRegion, bothEyesMirror, 0, width/2);
//            }));

            leftEyeMirror = OpenVR.GetMirrorTexture(Game.GraphicsDevice, 0);
            rightEyeMirror = OpenVR.GetMirrorTexture(Game.GraphicsDevice, 1);
            MirrorTexture = bothEyesMirror;

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
            Vector3 pos, scale, camPos;
            Matrix rot, camRot;
            Matrix leftEye, rightEye, head, leftProj, rightProj;

            OpenVR.UpdatePoses();

            //have to make sure it's updated now
            CameraRootEntity.Transform.UpdateWorldMatrix();

            OpenVR.GetEyeToHead(0, out leftEye);
            OpenVR.GetEyeToHead(1, out rightEye);

            state = OpenVR.GetHeadPose(out head);

            OpenVR.GetProjection(0, LeftCameraComponent.NearClipPlane, LeftCameraComponent.FarClipPlane, out leftProj);
            OpenVR.GetProjection(1, LeftCameraComponent.NearClipPlane, LeftCameraComponent.FarClipPlane, out rightProj);

            CameraRootEntity.Transform.WorldMatrix.Decompose(out scale, out camRot, out camPos);

            LeftCameraComponent.ProjectionMatrix = leftProj;

            var eyeMat = leftEye * head * Matrix.Scaling(ViewScaling) * Matrix.Translation(camPos) * camRot;          
            eyeMat.Decompose(out scale, out rot, out pos);
            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rot);
            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rot);
            var view = Matrix.LookAtRH(pos, pos + finalForward, finalUp);
            LeftCameraComponent.ViewMatrix = view;

            RightCameraComponent.ProjectionMatrix = rightProj;

            eyeMat = rightEye * head * Matrix.Scaling(ViewScaling) * Matrix.Translation(camPos) * camRot;  
            eyeMat.Decompose(out scale, out rot, out pos);
            finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rot);
            finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rot);
            view = Matrix.LookAtRH(pos, pos + finalForward, finalUp);
            RightCameraComponent.ViewMatrix = view;

            base.Draw(gameTime);
        }

        public override DeviceState State => state;

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.4f;

        public override Texture RenderFrame { get; protected set; }

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);
    }
}

#endif