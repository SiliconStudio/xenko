#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OpenVrHmd : VRDevice
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

        public override void Initialize(GraphicsDevice device, bool depthStencilResource = false, bool requireMirror = false)
        {
            var size = RenderFrameSize;
            var width = size.Width;
            var height = size.Height;
            RenderFrame = Texture.New2D(device, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            RenderFrameDepthStencil = Texture.New2D(device, width, height, PixelFormat.D24_UNorm_S8_UInt, depthStencilResource ? TextureFlags.DepthStencil | TextureFlags.ShaderResource : TextureFlags.DepthStencil);

            if (requireMirror)
            {
                bothEyesMirror = Texture.New2D(device, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
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

            leftEyeMirror = OpenVR.GetMirrorTexture(device, 0);
            rightEyeMirror = OpenVR.GetMirrorTexture(device, 1);
            MirrorTexture = bothEyesMirror;

            base.Initialize(device, requireMirror);
        }

        private Matrix currentHead;
        private Vector3 currentHeadPos, currentHeadLinearVelocity, currentHeadAngularVelocity;
        private Quaternion currentHeadRot;
        
        public override void Draw(GameTime gameTime)
        {
            OpenVR.UpdatePoses();
            state = OpenVR.GetHeadPose(out currentHead, out currentHeadLinearVelocity, out currentHeadAngularVelocity);
            Vector3 scale;
            currentHead.Decompose(out scale, out currentHeadRot, out currentHeadPos);
        }

        public override void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection)
        {
            Matrix eyeMat, rot;
            Vector3 pos, scale;

            OpenVR.GetEyeToHead(eye == Eyes.Left ? 0 : 1, out eyeMat);
            OpenVR.GetProjection(eye == Eyes.Left ? 0 : 1, near, far, out projection);

            eyeMat = eyeMat * currentHead * Matrix.Scaling(ViewScaling) * Matrix.Translation(cameraPosition) * cameraRotation;
            eyeMat.Decompose(out scale, out rot, out pos);
            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rot);
            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rot);
            view = Matrix.LookAtRH(pos, pos + finalForward, finalUp);
        }

        public override void Commit(CommandList commandList)
        {
            OpenVR.Submit(0, RenderFrame, ref leftView);
            OpenVR.Submit(1, RenderFrame, ref rightView);
            //            commandList.
            //
            //            OpenVR.Submit(0, RenderFrame, ref leftView);
            //            OpenVR.Submit(1, RenderFrame, ref rightView);
            //            
            //            //copy mirror
            //            if (!requireMirror) return;
            //            
            //            var wholeRegion = new ResourceRegion(0, 0, 0, width, height, 1);
            //            x.CommandList.CopyRegion(leftEyeMirror, 0, wholeRegion, bothEyesMirror, 0);
            //            x.CommandList.CopyRegion(rightEyeMirror, 0, wholeRegion, bothEyesMirror, 0, width/2);
        }

        //        public override void Draw(GameTime gameTime)
        //        {
        //            Vector3 pos, scale, camPos;
        //            Matrix rot, camRot;
        //            Matrix leftEye, rightEye, head, leftProj, rightProj;
        //
        //            OpenVR.UpdatePoses();
        //
        //            //have to make sure it's updated now
        //            CameraRootEntity.Transform.UpdateWorldMatrix();
        //
        //            OpenVR.GetEyeToHead(0, out leftEye);
        //            OpenVR.GetEyeToHead(1, out rightEye);
        //
        //            state = OpenVR.GetHeadPose(out head);
        //
        //            OpenVR.GetProjection(0, LeftCameraComponent.NearClipPlane, LeftCameraComponent.FarClipPlane, out leftProj);
        //            OpenVR.GetProjection(1, LeftCameraComponent.NearClipPlane, LeftCameraComponent.FarClipPlane, out rightProj);
        //
        //            CameraRootEntity.Transform.WorldMatrix.Decompose(out scale, out camRot, out camPos);
        //
        //            LeftCameraComponent.ProjectionMatrix = leftProj;
        //
        //            var eyeMat = leftEye * head * Matrix.Scaling(ViewScaling) * Matrix.Translation(camPos) * camRot;          
        //            eyeMat.Decompose(out scale, out rot, out pos);
        //            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rot);
        //            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rot);
        //            var view = Matrix.LookAtRH(pos, pos + finalForward, finalUp);
        //            LeftCameraComponent.ViewMatrix = view;
        //
        //            RightCameraComponent.ProjectionMatrix = rightProj;
        //
        //            eyeMat = rightEye * head * Matrix.Scaling(ViewScaling) * Matrix.Translation(camPos) * camRot;  
        //            eyeMat.Decompose(out scale, out rot, out pos);
        //            finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rot);
        //            finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rot);
        //            view = Matrix.LookAtRH(pos, pos + finalForward, finalUp);
        //            RightCameraComponent.ViewMatrix = view;
        //
        //            base.Draw(gameTime);
        //        }

        public override DeviceState State => state;

        public override Vector3 HeadPosition => currentHeadPos;

        public override Quaternion HeadRotation => currentHeadRot;

        public override Vector3 HeadLinearVelocity => currentHeadLinearVelocity;

        public override Vector3 HeadAngularVelocity => currentHeadAngularVelocity;

        public override Texture RenderFrameDepthStencil { get; protected set; }

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.4f;

        public override Texture RenderFrame { get; protected set; }

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);
    }
}

#endif