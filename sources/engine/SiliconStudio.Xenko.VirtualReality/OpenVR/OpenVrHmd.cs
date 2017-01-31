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
        private OpenVrTouchController leftHandController;
        private OpenVrTouchController rightHandController;
        private bool needsMirror;
        private Matrix currentHead;
        private Vector3 currentHeadPos, currentHeadLinearVelocity, currentHeadAngularVelocity;
        private Quaternion currentHeadRot;
        private int width;
        private int height;

        internal OpenVrHmd(IServiceRegistry registry) : base(registry)
        {
        }

        public override bool CanInitialize => OpenVR.InitDone || OpenVR.Init();

        public override void Initialize(GraphicsDevice device, bool depthStencilResource = false, bool requireMirror = false)
        {
            needsMirror = requireMirror;
            var size = RenderFrameSize;
            width = size.Width;
            height = size.Height;
            RenderFrame = Texture.New2D(device, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            RenderFrameDepthStencil = Texture.New2D(device, width, height, PixelFormat.D24_UNorm_S8_UInt, depthStencilResource ? TextureFlags.DepthStencil | TextureFlags.ShaderResource : TextureFlags.DepthStencil);

            if (needsMirror)
            {
                bothEyesMirror = Texture.New2D(device, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            }

            leftEyeMirror = OpenVR.GetMirrorTexture(device, 0);
            rightEyeMirror = OpenVR.GetMirrorTexture(device, 1);
            MirrorTexture = bothEyesMirror;

            leftHandController = new OpenVrTouchController(TouchControllerHand.Left);
            rightHandController = new OpenVrTouchController(TouchControllerHand.Right);

            base.Initialize(device, requireMirror);
        }

        public override void Draw(GameTime gameTime)
        {
            OpenVR.UpdatePoses();
            state = OpenVR.GetHeadPose(out currentHead, out currentHeadLinearVelocity, out currentHeadAngularVelocity);
            Vector3 scale;
            currentHead.Decompose(out scale, out currentHeadRot, out currentHeadPos);
        }

        public override void Update(GameTime gameTime)
        {
            LeftHand.Update(gameTime);
            RightHand.Update(gameTime);
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

            if (needsMirror)
            {
                var wholeRegion = new ResourceRegion(0, 0, 0, width, height, 1);
                commandList.CopyRegion(leftEyeMirror, 0, wholeRegion, bothEyesMirror, 0);
                commandList.CopyRegion(rightEyeMirror, 0, wholeRegion, bothEyesMirror, 0, width/2);
            }
        }

        public override DeviceState State => state;

        public override Vector3 HeadPosition => currentHeadPos;

        public override Quaternion HeadRotation => currentHeadRot;

        public override Vector3 HeadLinearVelocity => currentHeadLinearVelocity;

        public override Vector3 HeadAngularVelocity => currentHeadAngularVelocity;

        public override TouchController LeftHand => leftHandController;

        public override TouchController RightHand => rightHandController;

        public override Texture RenderFrameDepthStencil { get; protected set; }

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.4f;

        public override Texture RenderFrame { get; protected set; }

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);
    }
}

#endif