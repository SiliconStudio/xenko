using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.VirtualReality
{
    public class VRDeviceSystem : GameSystemBase
    {
        public VRDeviceSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(VRDeviceSystem), this);
            EnabledChanged += OnEnabledChanged;

            DrawOrder = -100;
            UpdateOrder = -100;
        }

        public VRApi[] PreferredApis;

        public VRDevice VRDevice { get; private set; }

        public bool DepthStencilAsResource;

        public bool RequireMirror;

        private void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            if (Enabled && VRDevice == null)
            {
                if (PreferredApis == null)
                {
                    return;
                }

                foreach (var hmdApi in PreferredApis)
                {
                    switch (hmdApi)
                    {
                        case VRApi.Oculus:
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                            VRDevice = new OculusOvrHmd();
                                
#endif
                        }
                            break;
                        case VRApi.OpenVr:
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                            VRDevice = new OpenVrHmd();
#endif
                        }
                            break;
                        case VRApi.Fove:
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                            VRDevice = new FoveHmd();
#endif
                        }
                            break;
                        case VRApi.Google:
                        {
#if SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID
                                VRDevice = new GoogleVrHmd();
#endif
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (VRDevice != null)
                    {
                        VRDevice.Game = Game;

                        if (VRDevice != null && !VRDevice.CanInitialize)
                        {
                            VRDevice.Dispose();
                            VRDevice = null;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                var deviceManager = (GraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
                VRDevice?.Enable(GraphicsDevice, deviceManager, DepthStencilAsResource, RequireMirror);
            }
        }

        public override void Update(GameTime gameTime)
        {
            VRDevice?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            VRDevice?.Draw(gameTime);
        }
    }
}