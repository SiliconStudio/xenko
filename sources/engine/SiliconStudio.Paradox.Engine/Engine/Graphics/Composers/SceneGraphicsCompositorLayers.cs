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

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsCompositorLayers"/> class.
        /// </summary>
        public SceneGraphicsCompositorLayers()
        {
            Layers = new SceneGraphicsLayerCollection();
            Master = new SceneGraphicsLayer()
            {
                Output = new GraphicsLayerOutputMaster(),
            };

            // Initialize states of this layer
            layersToDispose = new Dictionary<SceneGraphicsLayer, GraphicsLayerState>();
            layerStates = new Dictionary<SceneGraphicsLayer, GraphicsLayerState>();
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
                var layer = layerKeyState.Key;
                var layerState = layerKeyState.Value;

                // Dispose their output
                if (layerState.Output != null)
                {
                    layerState.Output.Dispose();
                }

                // Dispose their renderers
                layer.Renderers.Unload();
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

            // Sets the input of the layer (== last Current)
            var currentRenderFrame = context.Tags.Get(RenderFrame.Current);
            context.Tags.Set(SceneGraphicsLayer.CurrentInput, currentRenderFrame);

            // Sets the output of the layer 
            // Master is always going to use the Master frame for the current frame.
            if (isMaster)
            {
                // Master is always going to use the Master frame for the current frame.
                context.Tags.Set(RenderFrame.Current, context.Tags.Get(SceneGraphicsLayer.Master));
            }
            else
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

            layer.Renderers.Draw(context);
        }

        private class GraphicsLayerState
        {
            public IGraphicsLayerOutput Output { get; set; }
        }
    }
}