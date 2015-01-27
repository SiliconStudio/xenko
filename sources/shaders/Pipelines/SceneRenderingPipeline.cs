using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Effects.Renderers;
using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Effects.Skyboxes;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    // TODO: All this code is temporary
    public class SceneRenderingPipeline : PipelineBuilder	
    {
        private readonly CameraSetter cameraSetter;

        private readonly RenderTargetSetter rootRenderTargetSetter;

        private readonly ModelRenderer modelRenderer;

        private readonly DelegateRenderer postEffectRenderer;

        private readonly SkyboxBackgroundRenderer skyboxBackgroundRenderer;

        private readonly DirectLightForwardRenderProcessor directLightRenderRenderProcessor;

        private readonly SkyboxLightingRenderer skyboxLightingRenderer;

        private Texture renderTargetHDR;

        private bool useLighting;

        private bool useLightingChanged;

        private bool useHdr;

        private GraphicsDevice GraphicsDevice { get; set; }

        private readonly ImageEffectBundle postEffects;

        public SceneRenderingPipeline(IServiceRegistry serviceRegistry, RenderPipeline pipeline, string sceneEffect) : base(serviceRegistry, pipeline)
        {
            if (sceneEffect == null) throw new ArgumentNullException("sceneEffect");

            GraphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;

            // Add light processor
            Entities.Processors.Add(new LightProcessor());

            // Add processor for rendering the skybox
            Entities.Processors.Add(new SkyboxProcessor());

            cameraSetter = new CameraSetter(serviceRegistry);
            rootRenderTargetSetter = new RenderTargetSetter(serviceRegistry);

            Services.GetSafeServiceAs<IGame>().Window.ClientSizeChanged += Window_ClientSizeChanged;

            // TODO: This should come from a scene settings/camera settings...etc.
            postEffects = new ImageEffectBundle(serviceRegistry);
            postEffects.Bloom.Enabled = false;
            postEffects.BrightFilter.Enabled = false;
            postEffects.ColorTransform.Enabled = true;
            postEffects.ToneMap.AutoKeyValue = false;
            postEffects.ToneMap.Operator = new ToneMapU2FilmicOperator();

            skyboxBackgroundRenderer = new SkyboxBackgroundRenderer(Services);
            modelRenderer = new ModelRenderer(serviceRegistry, sceneEffect);
            directLightRenderRenderProcessor = new DirectLightForwardRenderProcessor(modelRenderer) { Enabled = false };
            skyboxLightingRenderer = new SkyboxLightingRenderer(modelRenderer) { Enabled = false };
            postEffectRenderer = new DelegateRenderer(Services) { Render = ApplyPostEffects };

            AddRenderer(new DelegateRenderer(Services) { Render = Update});
            AddRenderer(cameraSetter);
            AddRenderer(rootRenderTargetSetter);
            AddRenderer(skyboxBackgroundRenderer);
            AddRenderer(modelRenderer);
            AddRenderer(postEffectRenderer);
            // In all cases, we will setup back the default buffer and stencil
            AddRenderer(new RenderTargetSetter(Services) { EnableClearDepth = false, EnableClearStencil = false, EnableClearTarget = false, RenderTarget = GraphicsDevice.BackBuffer, DepthStencil = GraphicsDevice.DepthStencilBuffer });

            useLightingChanged = true;
        }

        public override void Unload()
        {
            Services.GetSafeServiceAs<IGame>().Window.ClientSizeChanged -= Window_ClientSizeChanged;
        }

        private void ApplyPostEffects(RenderContext obj)
        {
            // TODO allow posteffects on backbuffer
            if (useHdr)
            {
                postEffects.SetInput(renderTargetHDR);
                postEffects.SetOutput(GraphicsDevice.BackBuffer);
                postEffects.Draw();
            }
        }

        public Color ClearColor
        {
            get
            {
                // TODO: This should come from the camera
                return rootRenderTargetSetter.ClearColor;
            }
            set
            {
                // TODO: This should come from the camera
                rootRenderTargetSetter.ClearColor = value;
            }
        }

        public CameraComponent Camera
        {
            get
            {
                return cameraSetter.Camera;
            }
            set
            {
                cameraSetter.Camera = value;
            }
        }

        public bool UseHdr
        {
            get
            {
                // TODO: This should come from the camera
                return useHdr;
            }
            set
            {
                // TODO: This should come from the camera
                useHdr = value;
            }
        }

        public bool UseLighting
        {
            get
            {
                return useLighting;
            }
            set
            {
                if (useLighting != value)
                {
                    useLighting = value;
                    useLightingChanged = true;
                }
            }
        }

        private void Update(RenderContext renderContext)
        {
           // If Hdr
            if (useHdr)
            {
                if (renderTargetHDR == null)
                {
                    renderTargetHDR = Texture.New2D(GraphicsDevice, GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                }
                rootRenderTargetSetter.RenderTarget = renderTargetHDR;
            }
            else
            {
                Utilities.Dispose(ref renderTargetHDR);
            }

            // Set the rendertarget on the skybox
            skyboxBackgroundRenderer.Target = rootRenderTargetSetter.RenderTarget;

            // Upload lighting
            if (useLightingChanged)
            {
                directLightRenderRenderProcessor.Enabled = useLighting;
                skyboxLightingRenderer.Enabled = useLighting;

                LightingKeys.EnableFixedAmbientLight(GraphicsDevice.Parameters, !useLighting);
            }
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            Utilities.Dispose(ref renderTargetHDR);
        }
    }
}