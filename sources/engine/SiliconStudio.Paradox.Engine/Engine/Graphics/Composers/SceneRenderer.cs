// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    public class SceneRenderer : Renderer
    {
        private readonly Dictionary<GraphicsLayer, GraphicsLayerState> layerStates;

        public SceneRenderer(IServiceRegistry services, EntitySystem entitySystem, SceneComponent sceneComponent)
            : base(services)
        {
            if (entitySystem == null) throw new ArgumentNullException("entitySystem");
            if (sceneComponent == null) throw new ArgumentNullException("sceneComponent");
            SceneSystem = services.GetSafeServiceAs<SceneSystem>();
            Scene = sceneComponent.Entity;
            SceneComponent = sceneComponent;
            EntitySystem = entitySystem;
            PipelineManager = new PipelineManager();
        }

        public Entity Scene { get; private set; }

        public SceneComponent SceneComponent { get; private set; }

        public SceneSystem SceneSystem { get; private set; }

        public EntitySystem EntitySystem { get; private set; }


        public PipelineManager PipelineManager { get; private set; }

        protected override void OnRendering(RenderContext context)
        {
            // TODO: Just hardcode support here, but we should have a pluggable composer here
            var graphicsComposer = SceneComponent.GraphicsComposer as GraphicsComposerLayer;
            if (graphicsComposer == null)
            {
                return;
            }

            foreach (var layer in graphicsComposer.Layers)
            {
                if (!layer.Enabled)
                {
                    continue;
                }

                // TODO: harcoded for now
                if (!(layer.Mode is GraphicsRenderingModeForward))
                {
                    continue;
                }

            }
        }

        private class GraphicsLayerState
        {
            public GraphicsLayerState()
            {
            }

            public RenderFrame CurrentRenderFrame { get; set; }

        }
    }
}