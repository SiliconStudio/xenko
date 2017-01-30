using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class Hmd : IDisposable
    {
        protected Hmd()
        {
            ViewScaling = 1.0f;
        }

        public abstract Size2 OptimalRenderFrameSize { get; }

        public abstract Texture RenderFrame { get; protected set; }

        public abstract Texture RenderFrameDepthStencil { get; protected set; }

        public abstract Texture MirrorTexture { get; protected set; }

        public abstract float RenderFrameScaling { get; set; }

        public virtual Size2 RenderFrameSize
        {
            get
            {
                var width = (int)(OptimalRenderFrameSize.Width * RenderFrameScaling);
                width += width % 2;
                var height = (int)(OptimalRenderFrameSize.Height * RenderFrameScaling);
                height += height % 2;
                return new Size2(width, height);
            }
        }

        public abstract DeviceState State { get; }

        /// <summary>
        /// Allows you to scale the view, effectively it will change the size of the player in respect to the world, turning it into a giant or a tiny ant.
        /// </summary>
        /// <remarks>This will reduce the near clip plane of the cameras, it might induce depth issues.</remarks>
        public float ViewScaling { get; set; }

        public static Hmd GetHmd(HmdApi[] preferredApis)
        {
            foreach (var hmdApi in preferredApis)
            {
                switch (hmdApi)
                {
                    case HmdApi.Oculus:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new OculusOvrHmd();
                        if (device.CanInitialize) return device;
                        device.Dispose();
#endif
                    }
                        break;
                    case HmdApi.OpenVr:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new OpenVrHmd();
                        if (device.CanInitialize) return device;
                        device.Dispose();
#endif
                    }
                        break;
                    case HmdApi.Fove:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new FoveHmd();
                        if (device.CanInitialize) return device;
                        device.Dispose();
#endif
                    }
                        break;
                    case HmdApi.Google:
                    {
#if SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID
                        var device = new GoogleVrHmd();
                        if (device.CanInitialize) return device;
                        device.Dispose();
#endif
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            throw new NoHmdDeviceException();
        }

        public abstract bool CanInitialize { get; }

        public virtual void Initialize(GraphicsDevice device, bool depthStencilResource = false, bool requireMirror = false)
        {
        }

        public virtual void Recenter()
        {
        }

        public virtual void Dispose()
        {
        }

        public abstract void UpdateEyeParameters(ref Matrix cameraMatrix);

        public abstract void ReadEyeParameters(int eyeIndex, float near, float far, out Matrix view, out Matrix projection);

        public abstract void Commit(CommandList commandList);
    }
}
