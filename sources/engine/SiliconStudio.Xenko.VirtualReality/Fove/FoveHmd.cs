using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class FoveHmd : Hmd
    {
        private Texture nonSrgbFrame;
        private readonly Matrix referenceMatrix = Matrix.RotationZ(MathUtil.Pi);
        private readonly Matrix referenceMatrixInv = Matrix.RotationZ(MathUtil.Pi);

        private const float HalfIpd = 0.06f;
        private const float EyeHeight = 0.08f;
        private const float EyeForward = -0.04f;

        public FoveHmd(IServiceRegistry registry) : base(registry)
        {
            referenceMatrixInv.Invert();
        }

        public override void Initialize(Entity cameraRoot, CameraComponent leftCamera, CameraComponent rightCamera, bool requireMirror = false)
        {
            RenderFrameProvider = new DirectRenderFrameProvider(RenderFrame.FromTexture(Texture.New2D(GraphicsDevice, RenderFrameSize.Width, RenderFrameSize.Height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource)));
            nonSrgbFrame = Texture.New2D(GraphicsDevice, RenderFrameSize.Width, RenderFrameSize.Height, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);

            var compositor = (SceneGraphicsCompositorLayers)Game.SceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
            compositor.Master.Add(new SceneDelegateRenderer((x, y) =>
            {
                x.CommandList.Copy(RenderFrameProvider.RenderFrame.RenderTargets[0], nonSrgbFrame);
                //send to hmd
                var bounds0 = new Vector4(0.0f, 1.0f, 0.5f, 0.0f);
                var bounds1 = new Vector4(0.5f, 1.0f, 1.0f, 0.0f);
                if (!Fove.Submit(nonSrgbFrame.NativeResource.NativePointer, ref bounds0, 0) ||
                    !Fove.Submit(nonSrgbFrame.NativeResource.NativePointer, ref bounds1, 1))
                {
                    //failed...
                }

                Fove.Commit();
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
            var properties = new Fove.FrameProperties
            {
                Near = LeftCameraComponent.NearClipPlane,
                Far = LeftCameraComponent.FarClipPlane
            };
            Fove.PrepareRender(ref properties);

            properties.ProjLeft.Transpose();
            properties.ProjRight.Transpose();

            Vector3 scale, camPos;
            Quaternion camRot;

            //have to make sure it's updated now
            CameraRootEntity.Transform.UpdateWorldMatrix();
            CameraRootEntity.Transform.WorldMatrix.Decompose(out scale, out camRot, out camPos);

            LeftCameraComponent.ProjectionMatrix = properties.ProjLeft;

            var pos = camPos + (new Vector3(-HalfIpd * 0.5f, EyeHeight, EyeForward) * ViewScaling);
            var posV = pos + Vector3.Transform(properties.Pos * ViewScaling, camRot);
            var rotV = referenceMatrix * Matrix.RotationQuaternion(properties.Rot) * referenceMatrixInv * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotV);
            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotV);
            var view = Matrix.LookAtRH(posV, posV + finalForward, finalUp);
            LeftCameraComponent.ViewMatrix = view;

            RightCameraComponent.ProjectionMatrix = properties.ProjRight;

            pos = camPos + (new Vector3(HalfIpd * 0.5f, EyeHeight, EyeForward) * ViewScaling);
            posV = pos + Vector3.Transform(properties.Pos * ViewScaling, camRot);
            rotV = referenceMatrix * Matrix.RotationQuaternion(properties.Rot) * referenceMatrixInv * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
            finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotV);
            finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotV);
            view = Matrix.LookAtRH(posV, posV + finalForward, finalUp);
            RightCameraComponent.ViewMatrix = view;

            base.Draw(gameTime);
        }

        public override Size2 OptimalRenderFrameSize => new Size2(2560, 1440);

        public override DirectRenderFrameProvider RenderFrameProvider { get; protected set; }

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.2f;

        public override DeviceState State => DeviceState.Valid; //to improve and verify with Fove api.

        public override bool CanInitialize => Fove.Startup() && Fove.IsHardwareReady();
    }
}
