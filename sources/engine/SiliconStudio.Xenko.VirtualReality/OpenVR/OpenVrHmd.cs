// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OpenVRHmd : VRDevice
    {
        private RectangleF leftView = new RectangleF(0.0f, 0.0f, 0.5f, 1.0f);
        private RectangleF rightView = new RectangleF(0.5f, 0.0f, 1.0f, 1.0f);
        private Texture bothEyesMirror;
        private Texture leftEyeMirror;
        private Texture rightEyeMirror;
        private DeviceState state;
        private OpenVRTouchController leftHandController;
        private OpenVRTouchController rightHandController;
        private bool needsMirror;
        private Matrix currentHead;
        private Vector3 currentHeadPos, currentHeadLinearVelocity, currentHeadAngularVelocity;
        private Quaternion currentHeadRot;

        public override bool CanInitialize => OpenVR.InitDone || OpenVR.Init();

        public OpenVRHmd()
        {
            VRApi = VRApi.OpenVR;
            SupportsOverlays = true;
        }

        public override void Enable(GraphicsDevice device, GraphicsDeviceManager graphicsDeviceManager, bool requireMirror, int mirrorWidth, int mirrorHeight)
        {
            var width = (int)(OptimalRenderFrameSize.Width * RenderFrameScaling);
            width += width % 2;
            var height = (int)(OptimalRenderFrameSize.Height * RenderFrameScaling);
            height += height % 2;

            ActualRenderFrameSize = new Size2(width, height);

            needsMirror = requireMirror;

            if (needsMirror)
            {
                bothEyesMirror = Texture.New2D(device, width, height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            }

            leftEyeMirror = OpenVR.GetMirrorTexture(device, 0);
            rightEyeMirror = OpenVR.GetMirrorTexture(device, 1);
            MirrorTexture = bothEyesMirror;

            leftHandController = new OpenVRTouchController(TouchControllerHand.Left);
            rightHandController = new OpenVRTouchController(TouchControllerHand.Right);
        }

        public override VROverlay CreateOverlay(int width, int height, int mipLevels, int sampleCount)
        {
            var overlay = new OpenVROverlay();
            return overlay;
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

        public override void Commit(CommandList commandList, Texture renderFrame)
        {
            OpenVR.Submit(0, renderFrame, ref leftView);
            OpenVR.Submit(1, renderFrame, ref rightView);

            if (needsMirror)
            {
                var wholeRegion = new ResourceRegion(0, 0, 0, ActualRenderFrameSize.Width, ActualRenderFrameSize.Height, 1);
                commandList.CopyRegion(leftEyeMirror, 0, wholeRegion, bothEyesMirror, 0);
                commandList.CopyRegion(rightEyeMirror, 0, wholeRegion, bothEyesMirror, 0, ActualRenderFrameSize.Width / 2);
            }
        }

        public override DeviceState State => state;

        public override Vector3 HeadPosition => currentHeadPos;

        public override Quaternion HeadRotation => currentHeadRot;

        public override Vector3 HeadLinearVelocity => currentHeadLinearVelocity;

        public override Vector3 HeadAngularVelocity => currentHeadAngularVelocity;

        public override TouchController LeftHand => leftHandController;

        public override TouchController RightHand => rightHandController;

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.4f;

        public override Size2 ActualRenderFrameSize { get; protected set; }

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);

        public override void Dispose()
        {
            OpenVR.Shutdown();
        }
    }
}

#endif
