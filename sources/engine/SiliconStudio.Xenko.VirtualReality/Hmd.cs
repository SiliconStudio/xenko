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
            ViewScaling = 1.0f;
            Game.GameSystems.Add(this);
        }

        public virtual Entity CameraRootEntity { get; set; }

        public virtual CameraComponent LeftCameraComponent { get; set; }

        public virtual CameraComponent RightCameraComponent { get; set; }

        public abstract Size2 OptimalRenderFrameSize { get; }

        public abstract DirectRenderFrameProvider RenderFrameProvider { get; protected set; }

        public abstract Texture MirrorTexture { get; protected set; }

        public abstract float RenderFrameScaling { get; set; }

        public abstract Size2 RenderFrameSize { get; }

        public abstract DeviceState State { get; }

        /// <summary>
        /// Allows you to scale the view, effectively it will change the size of the player in respect to the world, turning it into a giant or a tiny ant.
        /// </summary>
        /// <remarks>This will reduce the near clip plane of the cameras, it might induce depth issues.</remarks>
        public float ViewScaling { get; set; }

        public static Hmd GetHmd(Game game, HmdApi[] preferredApis)
        {
            foreach (var hmdApi in preferredApis)
            {
                switch (hmdApi)
                {
                    case HmdApi.Oculus:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new OculusOvrHmd(game.Services);
                        if (device.CanInitialize) return device;
                        device.Destroy();
                        break;
#endif
                    }
                    case HmdApi.OpenVr:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new OpenVrHmd(game.Services);
                        if (device.CanInitialize) return device;
                        device.Destroy();
                        break;
#endif
                    }
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

        public abstract bool CanInitialize { get; }

        public virtual void Initialize(Entity cameraRoot, CameraComponent leftCamera, CameraComponent rightCamera, bool requireMirror = false)
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
