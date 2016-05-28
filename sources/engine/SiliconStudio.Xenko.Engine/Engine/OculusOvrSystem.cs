using System;
using SharpDX.Direct3D11;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Native;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract]
    public class OculusOvrSystemPost : GameSystem
    {
        private readonly OculusOvrSystem system;

        public OculusOvrSystemPost(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(OculusOvrSystemPost), this);
            system = registry.GetServiceAs<OculusOvrSystem>();
            Enabled = true;
            Visible = true;
            DrawOrder = 1000;
        }

        public override void Draw(GameTime gameTime)
        {
            system.Commit();
        }
    }

    public class OculusOvrSystem : GameSystem
    {
        private readonly Logger logger = GlobalLogger.GetLogger("OculusOvr");

        private SceneGraphicsCompositorLayers currentCompositor;

        private IntPtr sessionPtr = IntPtr.Zero;

        private Texture[] textures;
        private int texturesCount;

        private Texture depthBuffer;

        private DirectRenderFrameProvider[] frameProviders;

        private readonly SceneCameraRenderer[] sceneCameraRenderers = new SceneCameraRenderer[2];
        private ClearRenderFrameRenderer clearRenderFrameRenderer;
        private CameraComponent cameraLeft;
        private CameraComponent cameraRight;

        private Texture2D mirrorTexture;

        public OculusOvrSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(OculusOvrSystem), this);
            Enabled = true;
            Visible = true;
            DrawOrder = -1000;
        }

        public override void Initialize()
        {
#if DEBUG
            Game.ConsoleLogMode = ConsoleLogMode.Always;
            logger.Info("Initialize");
#endif

            if (!NativeInvoke.OculusOvr.Startup())
            {
                throw new Exception(NativeInvoke.OculusOvr.GetError());
            }

            long luid;
            sessionPtr = NativeInvoke.OculusOvr.CreateSessionDx(out luid);
            if (sessionPtr == IntPtr.Zero)
            {
                throw new Exception(NativeInvoke.OculusOvr.GetError());
            }

            Game.GraphicsDeviceManager.RequiredAdapterUid = luid.ToString();
        }

        protected override void LoadContent()
        {
            if (!NativeInvoke.OculusOvr.CreateTexturesDx(sessionPtr, GraphicsDevice.NativeDevice.NativePointer, out texturesCount))
            {
                throw new Exception(NativeInvoke.OculusOvr.GetError());
            }

            textures = new Texture[texturesCount];
            frameProviders = new DirectRenderFrameProvider[texturesCount];
            for (var i = 0; i < texturesCount; i++)
            {
                var ptr = NativeInvoke.OculusOvr.GetTextureDx(sessionPtr, new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"), i);
                if (ptr == IntPtr.Zero)
                {
                    throw new Exception(NativeInvoke.OculusOvr.GetError());
                }
                var dxTex = new Texture2D(ptr);
                textures[i] = new Texture(GraphicsDevice);
                textures[i].InitializeFrom(dxTex, true);
                //try release Dx obj?

                if (depthBuffer == null)
                {
                    depthBuffer = Texture.New2D(GraphicsDevice, textures[0].Width, textures[0].Height, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);
                }

                frameProviders[i] = new DirectRenderFrameProvider(RenderFrame.FromTexture(textures[i], depthBuffer));
            }

            var mirrorPtr = NativeInvoke.OculusOvr.GetMirrorTexture(sessionPtr, new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"));
            mirrorTexture = new Texture2D(mirrorPtr);
        }

        protected override void Destroy()
        {
            if(sessionPtr != IntPtr.Zero) NativeInvoke.OculusOvr.DestroySession(sessionPtr);
            NativeInvoke.OculusOvr.Shutdown();
        }

        public override void Draw(GameTime gameTime)
        {
            var compositor = (SceneGraphicsCompositorLayers)Game.SceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
            if (compositor != currentCompositor)
            {
                clearRenderFrameRenderer = (ClearRenderFrameRenderer)compositor.Master.Renderers[0];

                sceneCameraRenderers[0] = (SceneCameraRenderer)compositor.Master.Renderers[1];
                sceneCameraRenderers[1] = (SceneCameraRenderer)compositor.Master.Renderers[2];

                cameraLeft = compositor.Cameras.GetCamera(0);
                cameraLeft.UseCustomProjectionMatrix = true;
                cameraRight = compositor.Cameras.GetCamera(1);
                cameraRight.UseCustomProjectionMatrix = true;

                currentCompositor = compositor;
            }

            DrawEyes();
        }

        internal unsafe void DrawEyes()
        {
            Matrix leftProj, rightProj;
            NativeInvoke.OculusOvr.PrepareRender(sessionPtr, cameraLeft.NearClipPlane, cameraLeft.FarClipPlane, (float*)&leftProj, (float*)&rightProj);

            cameraLeft.ProjectionMatrix = leftProj;
            cameraRight.ProjectionMatrix = rightProj;

            var index = NativeInvoke.OculusOvr.GetCurrentTargetIndex(sessionPtr);

            clearRenderFrameRenderer.Output = frameProviders[index];

            sceneCameraRenderers[0].Output = frameProviders[index];
            sceneCameraRenderers[1].Output = frameProviders[index];
        }

        public void Commit()
        {
            NativeInvoke.OculusOvr.CommitFrame(sessionPtr);

            GraphicsDevice.NativeDeviceContext.CopyResource(mirrorTexture, GraphicsDevice.Presenter.BackBuffer.NativeResource);
        }
    }
}