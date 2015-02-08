// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Helper class to easily setup various predefined <see cref="RenderPipeline"/>, using <see cref="RenderPipeline.Pipeline"/>.
    /// </summary>
    public static class RenderPipelineFactory
    {
        public static void CreateSimple(IServiceRegistry serviceRegistry, string effectName, Color clearColor)
        {
            if (serviceRegistry == null) throw new ArgumentNullException("serviceRegistry");
            if (effectName == null) throw new ArgumentNullException("effectName");

            var renderSystem = serviceRegistry.GetSafeServiceAs<RenderSystem>();
            var graphicsService = serviceRegistry.GetSafeServiceAs<IGraphicsDeviceService>();

            var mainPipeline = renderSystem.Pipeline;

            mainPipeline.Renderers.Add(new CameraSetter(serviceRegistry));
            mainPipeline.Renderers.Add(new RenderTargetSetter(serviceRegistry)
            {
                ClearColor = clearColor,
                RenderTarget = graphicsService.GraphicsDevice.BackBuffer,
                DepthStencil = graphicsService.GraphicsDevice.DepthStencilBuffer
            });
            mainPipeline.Renderers.Add(new ModelRenderer(serviceRegistry, effectName));
            mainPipeline.Renderers.Add(new SpriteRenderer(serviceRegistry));
        }

        public static void CreateSimple(Game game, string effectName, Color clearColor)
        {
            CreateSimple(game.Services, effectName, clearColor);
        }
    }
}