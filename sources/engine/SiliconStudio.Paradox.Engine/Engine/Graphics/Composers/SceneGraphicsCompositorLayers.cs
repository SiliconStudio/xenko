// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A Graphics Composer using layers.
    /// </summary>
    [DataContract("SceneGraphicsCompositorLayers")]
    [Display("Layers")]
    public sealed class SceneGraphicsCompositorLayers : ISceneGraphicsCompositor
    {
        private readonly Dictionary<SceneGraphicsLayer, GraphicsLayerState> layerStates;
        private readonly Dictionary<SceneGraphicsLayer, GraphicsLayerState> layersToDispose;
        private readonly HashSet<IGraphicsRenderer> previousRenderers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsCompositorLayers"/> class.
        /// </summary>
        public SceneGraphicsCompositorLayers()
        {
            Layers = new SceneGraphicsLayerCollection();
            Master = new SceneGraphicsLayer()
            {
                Input = SceneGraphicsLayerInputLayer.PreviousLayer(),
                Output = new SceneGraphicsComposerOutputMaster(),
            };

            // Initialize states of this layer
            layersToDispose = new Dictionary<SceneGraphicsLayer, GraphicsLayerState>();
            layerStates = new Dictionary<SceneGraphicsLayer, GraphicsLayerState>();
            previousRenderers = new HashSet<IGraphicsRenderer>();
            // TODO: Add Disposable for composer
        }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        [DataMember(10)]
        [Category]
        public SceneGraphicsLayerCollection Layers { get; private set; }

        /// <summary>
        /// Gets the master layer.
        /// </summary>
        /// <value>The master layer.</value>
        [DataMember(20)]
        [Category]
        public SceneGraphicsLayer Master { get; private set; }

        public void Draw(RenderContext context)
        {
            // Save the list of previous layers
            layersToDispose.Clear();
            foreach (var layerKeyState in layerStates)
            {
                layersToDispose.Add(layerKeyState.Key, layerKeyState.Value);
            }

            // Process new layers
            foreach (var layer in Layers)
            {
                DrawLayer(context, layer, false);

                // Remove the layer from the previousLayerStates
                layersToDispose.Remove(layer);
            }

            // Process layers that are removed
            foreach (var layerKeyState in layersToDispose)
            {
                var layerState = layerKeyState.Value;

                // Dispose their output
                if (layerState.Output != null)
                {
                    layerState.Output.Dispose();
                }

                // Dispose their renderers
                foreach (var renderer in layerState.Renderers)
                {
                    renderer.Unload(context);
                    renderer.Dispose();
                }
            }
            layersToDispose.Clear();

            // Draw master part
            DrawLayer(context, Master, true);
        }

        private void DrawLayer(RenderContext context, SceneGraphicsLayer layer, bool isMaster)
        {
            if (!layer.Enabled || layer.Output == null)
            {
                return;
            }

            GraphicsLayerState layerState;
            if (!layerStates.TryGetValue(layer, out layerState))
            {
                layerState = new GraphicsLayerState();
                layerStates.Add(layer, layerState);
            }

            // Handle Input
            // TODO

            // Handle Output
            if (isMaster)
            {
                // Master is always going to use the Master frame for the current frame.
                context.Tags.Set(RenderFrame.Current, context.Tags.Get(RenderFrame.Master));
            }
            else
            {
                HandleOutput(context, layerState, layer);
            }

            // Handle Renderers
            // TODO: Renderer may be different based on the rendering model
            HandleRenderers(context, layerState, layer);
        }

        private void HandleOutput(RenderContext context, GraphicsLayerState layerState, SceneGraphicsLayer layer)
        {
            if (layerState.Output != null && layerState.Output != layer.Output)
            {
                // If we have a new output 
                layerState.Output.Dispose();
            }
            layerState.Output = layer.Output;
            var renderFrame = layer.Output.GetRenderFrame(context);
            context.Tags.Set(RenderFrame.Current, renderFrame);
        }

        private void HandleRenderers(RenderContext context, GraphicsLayerState layerState, SceneGraphicsLayer layer)
        {
            // Collect previous renderers
            previousRenderers.Clear();
            foreach (var renderer in layerState.Renderers)
            {
                previousRenderers.Add(renderer);
            }
            layerState.Renderers.Clear();

            // Iterate on new renderers
            foreach (var renderer in layer.Renderers)
            {
                // If renderer is new, then load it
                if (!previousRenderers.Contains(renderer))
                {
                    renderer.Load(context);
                }

                // Draw the renderer
                renderer.Draw(context);

                // Add it to the list of previous renderers
                layerState.Renderers.Add(renderer);

                // Remove this renderer from the previous list
                previousRenderers.Remove(renderer);
            }

            // The renderers in previousRenderers are renderers that were removed, so we need to unload and dispose them 
            foreach (var previousRenderer in previousRenderers)
            {
                previousRenderer.Unload(context);
                previousRenderer.Dispose();
            }
        }

        private class GraphicsLayerState
        {
            public GraphicsLayerState()
            {
                Renderers = new HashSet<IGraphicsRenderer>();
            }

            public ISceneGraphicsComposerOutput Output { get; set; }

            public HashSet<IGraphicsRenderer> Renderers { get; private set; }
        }
    }
}