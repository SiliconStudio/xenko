// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Effects.Renderers;
using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    public static class RenderPipelineLightingFactory
    {
        #region Public methods

        /// <summary>
        /// Creates a basic forward rendering pipeline.
        /// </summary>
        /// <param name="serviceRegistry">The IServiceRegistry.</param>
        /// <param name="effectName">The name of the main effect.</param>
        /// <param name="clearColor">The clear color of the final frame buffer.</param>
        /// <param name="useShadows">A flag stating if shadows are available in this pipeline.</param>
        /// <param name="ui">A flag stating if a UI renderer should be added to the pipeline.</param>
        /// <param name="backgroundName">The name of the background texture.</param>
        public static void CreateDefaultForward(IServiceRegistry serviceRegistry, string effectName, Color clearColor, bool useShadows, bool ui, string backgroundName)
        {
            if (serviceRegistry == null) throw new ArgumentNullException("serviceRegistry");
            if (effectName == null) throw new ArgumentNullException("effectName");

            var renderSystem = serviceRegistry.GetSafeServiceAs<RenderSystem>();
            var graphicsService = serviceRegistry.GetSafeServiceAs<IGraphicsDeviceService>();

            // Adds a light processor that will track all the entities that have a light component.
            // This will also handle the shadows (allocation, activation etc.).
            AddLightProcessor(serviceRegistry, graphicsService.GraphicsDevice, useShadows);

            var mainPipeline = renderSystem.Pipeline;
            
            // Adds a camera setter that will automatically fill the parameters from the camera (matrices, fov etc.).
            mainPipeline.Renderers.Add(new CameraSetter(serviceRegistry));

            // Adds a recursive pass to render the shadow maps
            // This will render all the meshes with a different effect for shadow casting.
            if (useShadows)
                AddShadowMap(serviceRegistry, mainPipeline, effectName);

            // Sets the render targets and clear them.
            mainPipeline.Renderers.Add(new RenderTargetSetter(serviceRegistry)
            {
                ClearColor = clearColor,
                RenderTarget = graphicsService.GraphicsDevice.BackBuffer,
                DepthStencil = graphicsService.GraphicsDevice.DepthStencilBuffer
            });

            // Draws a background from a texture.
            if (backgroundName != null)
                mainPipeline.Renderers.Add(new BackgroundRenderer(serviceRegistry, backgroundName));

            // Renders all the meshes with the correct lighting.
            mainPipeline.Renderers.Add(new ModelRenderer(serviceRegistry, effectName).AddLightForwardSupport());

            // Renders the UI.
            if (ui)
                mainPipeline.Renderers.Add(new UIRenderer(serviceRegistry));
        }

        /// <summary>
        /// Creates a basic forward rendering pipeline.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="effectName">The name of the main effect.</param>
        /// <param name="clearColor">The clear color of the final frame buffer.</param>
        /// <param name="useShadows">A flag stating if shadows are available in this pipeline.</param>
        /// <param name="ui">A flag stating if a UI renderer should be added to the pipeline.</param>
        /// <param name="backgroundName">The name of the background texture.</param>
        public static void CreateDefaultForward(Game game, string effectName, Color clearColor, bool useShadows, bool ui, string backgroundName = null)
        {
            CreateDefaultForward(game.Services, effectName, clearColor, useShadows, ui, backgroundName);
        }

        /// <summary>
        /// Destroys the forward rendering pipeline.
        /// </summary>
        /// <param name="game">The game.</param>
        public static void DestroyDefaultForwardPipeline(Game game)
        {
            if (game == null) throw new ArgumentNullException("game");
            
            var serviceRegistry = game.Services;
            if (serviceRegistry == null)
                return;

            var renderSystem = serviceRegistry.GetSafeServiceAs<RenderSystem>();
            var entitySystem = serviceRegistry.GetServiceAs<EntitySystem>();
            
            var mainPipeline = renderSystem.Pipeline;

            /** TODO rewrite this
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<UIRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<LightForwardModelRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<BackgroundRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<RenderTargetSetter>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<ShadowMapRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<CameraSetter>());
            entitySystem.Processors.Remove(entitySystem.GetProcessor<LightShadowProcessor>());
             */
        }

        /// <summary>
        /// Creates a basic deferred rendering pipeline.
        /// </summary>
        /// <param name="serviceRegistry">The IServiceRegistry.</param>
        /// <param name="effectName">The name of the main effect.</param>
        /// <param name="prepassEffectName">The name of the light prepass effect.</param>
        /// <param name="clearColor">The clear color of the final frame buffer.</param>
        /// <param name="useShadows">A flag stating if shadows are available in this pipeline.</param>
        /// <param name="ui">A flag stating if a UI renderer should be added to the pipeline.</param>
        /// <param name="backgroundName">The name of the background texture.</param>
        public static void CreateDefaultDeferred(IServiceRegistry serviceRegistry, string effectName, string prepassEffectName, Color clearColor, bool useShadows, bool ui, string backgroundName)
        {
            if (serviceRegistry == null) throw new ArgumentNullException("serviceRegistry");
            if (effectName == null) throw new ArgumentNullException("effectName");

            var renderSystem = serviceRegistry.GetSafeServiceAs<RenderSystem>();
            var graphicsService = serviceRegistry.GetSafeServiceAs<IGraphicsDeviceService>();

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
            var width = graphicsService.GraphicsDevice.DepthStencilBuffer.Width;
            var height = graphicsService.GraphicsDevice.DepthStencilBuffer.Height;

            // On OpenGL, intermediate texture are flipped and we cannot create a framebuffer with a user-generated attachment and a default one.
            // So we will render everything into a intermediate texture and draw it on screen at the end.
            var finalRenderTexture = Texture.New2D(graphicsService.GraphicsDevice, width, height, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            var finalDepthBuffer = Texture.New2D(graphicsService.GraphicsDevice, width, height, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
#else
            var finalRenderTexture = graphicsService.GraphicsDevice.BackBuffer;
            var finalDepthBuffer = graphicsService.GraphicsDevice.DepthStencilBuffer;
#endif

            var readOnlyDepthBuffer = finalDepthBuffer.ToDepthStencilReadOnlyTexture();

            // Adds a light processor that will track all the entities that have a light component.
            // This will also handle the shadows (allocation, activation etc.).
            AddLightProcessor(serviceRegistry, graphicsService.GraphicsDevice, useShadows);

            // Create Main pass
            var mainPipeline = renderSystem.Pipeline;

            // Adds a camera setter that will automatically fill the parameters from the camera (matrices, fov etc.).
            mainPipeline.Renderers.Add(new CameraSetter(serviceRegistry));

            // Adds a recursive pass to render the shadow maps
            // This will render all the meshes with a different effect for shadow casting.
            if (useShadows)
                AddShadowMap(serviceRegistry, mainPipeline, effectName);

            // Create G-buffer pass
            var gbufferPipeline = new RenderPipeline("GBuffer");

            // Renders the G-buffer for opaque geometry.
            gbufferPipeline.Renderers.Add(new ModelRenderer(serviceRegistry, effectName + ".ParadoxGBufferShaderPass").AddOpaqueFilter());
            var gbufferProcessor = new GBufferRenderProcessor(serviceRegistry, gbufferPipeline, finalDepthBuffer, false);

            // Add sthe G-buffer pass to the pipeline.
            mainPipeline.Renderers.Add(gbufferProcessor);

            // Performs the light prepass on opaque geometry.
            // Adds this pass to the pipeline.
            var lightDeferredProcessor = new LightingPrepassRenderer(serviceRegistry, prepassEffectName, finalDepthBuffer, gbufferProcessor.GBufferTexture);
            mainPipeline.Renderers.Add(lightDeferredProcessor);

            // Sets the render targets and clear them. Also sets the viewport.
            mainPipeline.Renderers.Add(new RenderTargetSetter(serviceRegistry)
            {
                ClearColor = clearColor,
                EnableClearDepth = false,
                RenderTarget = finalRenderTexture,
                DepthStencil = finalDepthBuffer,
                Viewport = new Viewport(0, 0, finalRenderTexture.ViewWidth, finalRenderTexture.ViewHeight)
            });

            // Draws a background from a texture.
            if (backgroundName != null)
                mainPipeline.Renderers.Add(new BackgroundRenderer(serviceRegistry, backgroundName));

            // Prevents depth write since depth was already computed in G-buffer pas.
            mainPipeline.Renderers.Add(new RenderStateSetter(serviceRegistry) { DepthStencilState = graphicsService.GraphicsDevice.DepthStencilStates.DepthRead });
            mainPipeline.Renderers.Add(new ModelRenderer(serviceRegistry, effectName).AddOpaqueFilter());
            mainPipeline.Renderers.Add(new RenderTargetSetter(serviceRegistry)
            {
                EnableClearDepth = false,
                EnableClearTarget = false,
                RenderTarget = finalRenderTexture,
                DepthStencil = finalDepthBuffer,
                Viewport = new Viewport(0, 0, finalRenderTexture.ViewWidth, finalRenderTexture.ViewHeight)
            });

            // Renders transparent geometry. Depth stencil state is determined by the object to draw.
            mainPipeline.Renderers.Add(new RenderStateSetter(serviceRegistry) { DepthStencilState = graphicsService.GraphicsDevice.DepthStencilStates.DepthRead });
            mainPipeline.Renderers.Add(new ModelRenderer(serviceRegistry, effectName).AddTransparentFilter());

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
            // on OpenGL, draw the final texture to the framebuffer
            mainPipeline.Renderers.Add(new RenderStateSetter(serviceRegistry)
            {
                DepthStencilState = graphicsService.GraphicsDevice.DepthStencilStates.None,
                BlendState = graphicsService.GraphicsDevice.BlendStates.Opaque
            });
            mainPipeline.Renderers.Add(new RenderTargetSetter(serviceRegistry)
            {
                ClearColor = clearColor,
                EnableClearDepth = false,
                EnableClearStencil = false,
                EnableClearTarget = false,
                RenderTarget = graphicsService.GraphicsDevice.BackBuffer,
                DepthStencil = null,
                Viewport = new Viewport(0, 0, graphicsService.GraphicsDevice.BackBuffer.ViewWidth, graphicsService.GraphicsDevice.BackBuffer.ViewHeight)
            });
            mainPipeline.Renderers.Add(new DelegateRenderer(serviceRegistry) { Render = (context => graphicsService.GraphicsDevice.DrawTexture(finalRenderTexture, true))});
#endif
            // Renders the UI.
            if (ui)
                mainPipeline.Renderers.Add(new UIRenderer(serviceRegistry));

            graphicsService.GraphicsDevice.Parameters.Set(RenderingParameters.UseDeferred, true);
        }

        /// <summary>
        /// Creates a basic deferred rendering pipeline.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="effectName">The name of the main effect.</param>
        /// <param name="prepassEffectName">The name of the light prepass effect.</param>
        /// <param name="clearColor">The clear color.</param>
        /// <param name="useShadows">A flag stating if shadows are available in this pipeline.</param>
        /// <param name="backgroundName">The name of the background texture.</param>
        public static void CreateDefaultDeferred(Game game, string effectName, string prepassEffectName, Color clearColor, bool useShadows, bool ui, string backgroundName = null)
        {
            CreateDefaultDeferred(game.Services, effectName, prepassEffectName, clearColor, useShadows, ui, backgroundName);
        }

        /// <summary>
        /// Destroys the deferred rendering pipeline.
        /// </summary>
        /// <param name="game">The game.</param>
        public static void DestroyDefaultDeferredPipeline(Game game)
        {
            if (game == null) throw new ArgumentNullException("game");

            var serviceRegistry = game.Services;
            if (serviceRegistry == null)
                return;

            var renderSystem = serviceRegistry.GetSafeServiceAs<RenderSystem>();
            var entitySystem = serviceRegistry.GetServiceAs<EntitySystem>();

            var mainPipeline = renderSystem.Pipeline;

            /*TODO REVIEW THIS CLASS
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<UIRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<TransparentModelRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<RenderStateSetter>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<RenderTargetSetter>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<OpaqueModelRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<RenderStateSetter>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<BackgroundRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<RenderTargetSetter>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<LightingPrepassRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<GBufferRenderProcessor>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<ShadowMapRenderer>());
            mainPipeline.Renderers.Remove(mainPipeline.GetProcessor<CameraSetter>());
             */
            entitySystem.Processors.Remove(entitySystem.GetProcessor<LightShadowProcessor>());
        }

        #endregion

        #region Private methods

        private static ShadowMapRenderer AddShadowMap(IServiceRegistry serviceRegistry, RenderPipeline pipeline, string effectName)
        {
            var shadowMapPipeline = new RenderPipeline("ShadowMap");
            shadowMapPipeline.Renderers.Add(new ModelRenderer(serviceRegistry, effectName + ".ShadowMapCaster").AddContextActiveLayerFilter().AddShadowCasterFilter());

            var shadowMapRenderer = new ShadowMapRenderer(serviceRegistry, shadowMapPipeline);
            pipeline.Renderers.Add(shadowMapRenderer);

            return shadowMapRenderer;
        }

        private static void AddLightProcessor(IServiceRegistry serviceRegistry, GraphicsDevice graphicsDevice, bool useShadows)
        {
            var entitySystem = serviceRegistry.GetServiceAs<EntitySystem>();
            if (entitySystem != null)
            {
                var lightProcessor = entitySystem.GetProcessor<LightShadowProcessor>();
                if (lightProcessor == null)
                    entitySystem.Processors.Add(new DynamicLightShadowProcessor(graphicsDevice, useShadows));
            }
        }

        #endregion
    }
}
