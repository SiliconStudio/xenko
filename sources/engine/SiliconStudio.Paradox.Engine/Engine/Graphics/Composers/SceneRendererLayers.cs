// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A Graphics Composer using layers.
    /// </summary>
    [DataContract("SceneRendererLayers")]
    [Display("Layers")]
    public sealed class SceneRendererLayers : SceneLayer, ISceneRenderer
    {
        private readonly Dictionary<SceneLayer, GraphicsLayerState> layerStates;
        private readonly Dictionary<SceneLayer, GraphicsLayerState> layersToDispose;
        private readonly HashSet<IGraphicsRenderer> previousRenderers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneRendererLayers"/> class.
        /// </summary>
        public SceneRendererLayers()
        {
            Layers = new GraphicsLayerCollection();
            Input = GraphicsLayerInputLayer.PreviousLayer();
            Output = new GraphicsComposerOutputMaster();

            // Initialize states of this layer
            layersToDispose = new Dictionary<SceneLayer, GraphicsLayerState>();
            layerStates = new Dictionary<SceneLayer, GraphicsLayerState>();
            previousRenderers = new HashSet<IGraphicsRenderer>();

            // TODO: Add Disposable for composer
        }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        [DataMember(45)]
        [Category]
        public GraphicsLayerCollection Layers { get; private set; }

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
            DrawLayer(context, this, true);
        }

        private void DrawLayer(RenderContext context, SceneLayer layer, bool isMaster)
        {
            if (!layer.Enabled || layer.Mode == null || layer.Output == null)
            {
                return;
            }

            // TODO: harcoded for now
            if (!(layer.Mode is GraphicsRenderingModeForward))
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
                context.Parameters.Set(RenderFrame.Current, context.Parameters.Get(RenderFrame.Master));
            }
            else
            {
                HandleOutput(context, layerState, layer);
            }

            // Handle Renderers
            // TODO: Renderer may be different based on the rendering model
            HandleRenderers(context, layerState, layer);
        }

        private void HandleOutput(RenderContext context, GraphicsLayerState layerState, SceneLayer layer)
        {
            if (layerState.Output != null && layerState.Output != layer.Output)
            {
                // If we have a new output 
                layerState.Output.Dispose();
            }
            layerState.Output = layer.Output;
            var renderFrame = layer.Output.GetRenderFrame(context);
            context.Parameters.Set(RenderFrame.Current, renderFrame);
        }

        private void HandleRenderers(RenderContext context, GraphicsLayerState layerState, SceneLayer layer)
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

            public IGraphicsComposerOutput Output { get; set; }

            public HashSet<IGraphicsRenderer> Renderers { get; private set; }
        }
    }
}