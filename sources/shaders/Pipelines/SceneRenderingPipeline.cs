using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Effects.Skyboxes;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

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

        private readonly InputManager Input;

        private Texture renderTargetHDR;

        private Texture depthStencilMSAA;
        private bool useLighting;

        private bool useLightingChanged;

        private bool useHdr;

        private GraphicsDevice GraphicsDevice { get; set; }

        private readonly ImageEffectBundle postEffects;

        public SceneRenderingPipeline(IServiceRegistry serviceRegistry, RenderPipeline pipeline, string sceneEffect) : base(serviceRegistry, pipeline)
        {
            if (sceneEffect == null) throw new ArgumentNullException("sceneEffect");

            GraphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;

            Input = Services.GetSafeServiceAs<InputManager>();

            RenderTarget = GraphicsDevice.BackBuffer;
            DepthStencilBuffer = GraphicsDevice.DepthStencilBuffer; 

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

            // TODO: Add support for Push/Pop of DepthStencil/RenderTarget/States into the GraphicsDevice
            AddRenderer(new DelegateRenderer(Services) { Render = Update});
            AddRenderer(cameraSetter);
            AddRenderer(rootRenderTargetSetter);
            AddRenderer(skyboxBackgroundRenderer);
            AddRenderer(new DelegateRenderer(Services) { Render = ResetTargets});
            AddRenderer(modelRenderer);
            AddRenderer(postEffectRenderer);
            // In all cases, we will setup back the default buffer and stencil
            AddRenderer(new RenderTargetSetter(Services) { EnableClearDepth = false, EnableClearStencil = false, EnableClearTarget = false, RenderTarget = RenderTarget, DepthStencil = DepthStencilBuffer });
            AddRenderer(new SpriteRenderer(Services));

            // TODO: Multisample is not working yet as we need to resolve the MSAA depth buffer to a non MSAA
            // IsMultiSample = true;

            useLightingChanged = true;
        }

        private void ResetTargets(RenderContext renderContext)
        {
            GraphicsDevice.SetDepthAndRenderTarget(rootRenderTargetSetter.DepthStencil, rootRenderTargetSetter.RenderTarget);
        }

        public override void Unload()
        {
            Services.GetSafeServiceAs<IGame>().Window.ClientSizeChanged -= Window_ClientSizeChanged;
        }

        public Texture RenderTarget { get; set; }

        public Texture DepthStencilBuffer { get; set; }

        private void ApplyPostEffects(RenderContext obj)
        {
            Texture msaaRenderTargetRersolve = null;

            if (Input.IsKeyReleased(Keys.L))
            {
                postEffects.Antialiasing.Enabled = !postEffects.Antialiasing.Enabled;
            }

            // Resolve multisampling 
            if (IsMultiSample)
            {
                var descNoMsaa = renderTargetHDR.Description;
                descNoMsaa.MultiSampleLevel = MSAALevel.None;
                msaaRenderTargetRersolve = postEffects.Context.Allocator.GetTemporaryTexture(descNoMsaa);

                GraphicsDevice.CopyMultiSample(renderTargetHDR, 0, msaaRenderTargetRersolve, 0);
                GraphicsDevice.Clear(DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            }

            // TODO allow posteffects on backbuffer
            if (useHdr)
            {
                postEffects.SetInput(msaaRenderTargetRersolve ?? renderTargetHDR);
                postEffects.SetOutput(RenderTarget);
                postEffects.Draw();
            }

            // Release the temporary texture for MSAA resolve
            postEffects.Context.Allocator.ReleaseReference(msaaRenderTargetRersolve);
        }

        public bool IsMultiSample { get; set; }

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
                    Utilities.Dispose(ref renderTargetHDR);
                    Utilities.Dispose(ref depthStencilMSAA);

                    var desc = TextureDescription.New2D(RenderTarget.Width, RenderTarget.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                    if (IsMultiSample)
                    {
                        desc.MultiSampleLevel = MSAALevel.X4;
                    }
                    renderTargetHDR = Texture.New(GraphicsDevice, desc);

                    if (IsMultiSample)
                    {
                        desc = DepthStencilBuffer.Description;
                        desc.MultiSampleLevel = MSAALevel.X4;
                        desc.Width = RenderTarget.Width;
                        desc.Height = RenderTarget.Height;
                        depthStencilMSAA = Texture.New(GraphicsDevice, desc);
                    }
                }
                rootRenderTargetSetter.RenderTarget = renderTargetHDR;
                rootRenderTargetSetter.DepthStencil = IsMultiSample ? depthStencilMSAA : DepthStencilBuffer;
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