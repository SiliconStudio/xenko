using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class VRDevice : GameSystemBase
    {
        protected VRDevice(IServiceRegistry registry) : base(registry)
        {
            Visible = true;
            Enabled = true;            
            DrawOrder = -100;
            UpdateOrder = -100;

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

        public abstract Vector3 HeadPosition { get; }

        public abstract Quaternion HeadRotation { get; }

        public abstract Vector3 HeadLinearVelocity { get; }

        public abstract Vector3 HeadAngularVelocity { get; }

        public abstract TouchController LeftHand { get; }

        public abstract TouchController RightHand { get; }

        /// <summary>
        /// Allows you to scale the view, effectively it will change the size of the player in respect to the world, turning it into a giant or a tiny ant.
        /// </summary>
        /// <remarks>This will reduce the near clip plane of the cameras, it might induce depth issues.</remarks>
        public float ViewScaling { get; set; }

        public static VRDevice GetVRDevice(IServiceRegistry registry, VRApi[] preferredApis)
        {
            var existingDevice = registry.GetService(typeof(VRDevice));
            if (existingDevice != null)
                return (VRDevice)existingDevice;

            foreach (var hmdApi in preferredApis)
            {
                switch (hmdApi)
                {
                    case VRApi.Oculus:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new OculusOvrHmd(registry);
                        if (device.CanInitialize)
                        {                               
                            registry.AddService(typeof(VRDevice), device);
                            device.Game.GameSystems.Add(device);
                            return device;
                        }
                        device.Dispose();
#endif
                    }
                        break;
                    case VRApi.OpenVr:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new OpenVrHmd(registry);
                        if (device.CanInitialize)
                        {
                            registry.AddService(typeof(VRDevice), device);
                            device.Game.GameSystems.Add(device);
                            return device;
                        }
                        device.Dispose();
#endif
                    }
                        break;
                    case VRApi.Fove:
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                        var device = new FoveHmd(registry);
                        if (device.CanInitialize)
                        {
                            registry.AddService(typeof(VRDevice), device);
                            device.Game.GameSystems.Add(device);
                            return device;
                        }
                        device.Dispose();
#endif
                    }
                        break;
                    case VRApi.Google:
                    {
#if SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID
                        var device = new GoogleVrHmd();
                        if (device.CanInitialize)
                        {
                            registry.AddService(typeof(VRDevice), device);
                            device.Game.GameSystems.Add(device);
                            return device;
                        }
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

        protected override void Destroy()
        {
        }

        public abstract void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection);

        public abstract void Commit(CommandList commandList);
    }
}
