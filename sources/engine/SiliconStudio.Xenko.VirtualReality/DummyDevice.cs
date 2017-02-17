using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public class DummyDevice : VRDevice
    {
        private const float HalfIpd = 0.06f;
        private const float EyeHeight = 0.08f;
        private const float EyeForward = -0.04f;

        public override Size2 OptimalRenderFrameSize => new Size2(2160, 1200);

        public override Size2 ActualRenderFrameSize { get; protected set; }

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; }

        public override DeviceState State => DeviceState.Valid;

        public override Vector3 HeadPosition => Vector3.Zero;

        public override Quaternion HeadRotation => Quaternion.Identity;

        public override Vector3 HeadLinearVelocity => Vector3.Zero;

        public override Vector3 HeadAngularVelocity => Vector3.Zero;

        public override TouchController LeftHand => null;

        public override TouchController RightHand => null;

        public override bool CanInitialize => true;

        public override void Enable(GraphicsDevice device, GraphicsDeviceManager graphicsDeviceManager, bool requireMirror, int mirrorWidth, int mirrorHeight)
        {
            ActualRenderFrameSize = OptimalRenderFrameSize;
            MirrorTexture = Texture.New2D(device, ActualRenderFrameSize.Width, ActualRenderFrameSize.Height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
        }

        public override void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection)
        {
            projection = new Matrix(1.19034183f, 0, 0, 0, 0, 0.999788344f, 0, 0, 0.148591548f, -0.110690169f, -1.0001f, -1, 0, 0, -0.10001f, 0); //oculus proj

            var pos = cameraPosition + new Vector3((eye == Eyes.Left ? -HalfIpd : HalfIpd) * 0.5f, EyeHeight, EyeForward) * ViewScaling;
            var posV = pos + Vector3.Transform(HeadPosition * ViewScaling, HeadRotation);
            var rotV = Matrix.RotationQuaternion(HeadRotation) * Matrix.Scaling(ViewScaling) * cameraRotation;
            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotV);
            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotV);
            view = Matrix.LookAtRH(posV, posV + finalForward, finalUp);
        }

        public override void Commit(CommandList commandList, Texture renderFrame)
        {
            commandList.Copy(renderFrame, MirrorTexture);
        }

        public override void Update(GameTime gameTime)
        {
            //nothing needed
        }

        public override void Draw(GameTime gameTime)
        {
            //nothing needed
        }
    }
}