using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class Hmd : GameSystem
    {
        protected Hmd(IServiceRegistry registry) : base(registry)
        {
            DrawOrder = -10000;
            ViewScaling = Matrix.Scaling(1.0f);
            Game.GameSystems.Add(this);
        }

        public virtual Entity CameraRootEntity { get; set; }

        public virtual CameraComponent LeftCameraComponent { get; set; }

        public virtual CameraComponent RightCameraComponent { get; set; }

        public Matrix HeadPose { get; protected set; }

        public Matrix LeftEyePose { get; protected set; }

        public Matrix RightEyePose { get; protected set; }

        public abstract Size2 OptimalRenderFrameSize { get; }

        public abstract DirectRenderFrameProvider RenderFrameProvider { get; protected set; }

        public abstract Texture MirrorTexture { get; protected set; }

        public abstract float RenderFrameScaling { get; set; }

        public abstract Size2F RenderFrameSize { get; }

        public abstract DeviceState State { get; protected set; }

        public Matrix ViewScaling { get; set; }

        public static Hmd GetHmd(Game game, HmdApi[] preferredApis)
        {
            foreach (var hmdApi in preferredApis)
            {
                switch (hmdApi)
                {
                    case HmdApi.Oculus:
                        break;
                    case HmdApi.OpenVr:
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        return new OpenVrHmd(game.Services);
#endif
                    case HmdApi.Fove:
                        break;
                    case HmdApi.Google:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            throw new NoHmdDeviceException();
        }

        public virtual void Initialize(Entity cameraRoot, CameraComponent leftCamera, CameraComponent rightCamera)
        {
            CameraRootEntity = cameraRoot;
            LeftCameraComponent = leftCamera;
            RightCameraComponent = rightCamera;

            Visible = true;
        }

        public virtual void Recenter()
        {
        }

        protected override void Destroy()
        {
            Game.GameSystems.Remove(this);
            base.Destroy();
        }
    }
}
