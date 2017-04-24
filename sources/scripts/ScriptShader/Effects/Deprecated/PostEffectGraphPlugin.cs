// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Xenko.Graphics;

using QuickGraph;
using QuickGraph.Algorithms;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Posteffect manager.
    /// </summary>
    public class PostEffectGraphPlugin : PostEffectPlugin, IRenderPassPluginSource, IRenderPassPluginTarget
    {
        BidirectionalGraph<EffectMesh, PostEffectEdge> graph = new BidirectionalGraph<EffectMesh, PostEffectEdge>();
        private Dictionary<TextureDescription, List<RenderTarget>> textures = new Dictionary<TextureDescription, List<RenderTarget>>();

        public PostEffectGraphPlugin() : this(null)
        {
        }
        
        public PostEffectGraphPlugin(string name) : base(name)
        {
        }

        public IEnumerable<EffectMesh> Meshes
        {
            get
            {
                return graph.Vertices;
            }
        }

        public void AddMesh(EffectMesh mesh)
        {
            if (!graph.ContainsVertex(mesh))
                graph.AddVertex(mesh);
        }

        public void AddLink(EffectMesh source, ParameterKey<RenderTarget> sourceKey, EffectMesh target, ParameterKey<Texture> targetKey, RenderTarget texture)
        {
            if (!graph.ContainsVertex(source))
                graph.AddVertex(source);

            if (!graph.ContainsVertex(target))
                graph.AddVertex(target);

            graph.AddEdge(new PostEffectEdge(source, sourceKey, target, targetKey, texture));
        }

        public void AddLink(EffectMesh source, ParameterKey<RenderTarget> sourceKey, EffectMesh target, ParameterKey<Texture> targetKey, TextureDescription? textureDescription = null)
        {
            if (!graph.ContainsVertex(source))
                graph.AddVertex(source);

            if (!graph.ContainsVertex(target))
                graph.AddVertex(target);

            graph.AddEdge(new PostEffectEdge(source, sourceKey, target, targetKey, textureDescription));
        }

        /// <summary>
        /// Resolve the dependency graph for the resources and create the necessary RenderTarget2D.
        /// It will take care of reusing resources (if possible) and creating them if more are necessaries.
        /// </summary>
        public void Resolve()
        {
            var currentTextures = new Dictionary<RenderTarget, int>();

            // Topological sort in order to have nodes in their dependency order
            foreach (var node in graph.TopologicalSort())
            {
                var inEdges = graph.InEdges(node);

                // Plug the effect pass into the render pass
                node.EffectPass.RenderPass = RenderPass;

                // Group nodes by source key, because each (Source,SourceKey) will be a single output of the current node and 
                // will correspond to one shared resource.
                foreach (var outEdges in graph.OutEdges(node).GroupBy(x => x.SourceKey))
                {
                    // TODO: First, execute action stored in EffectMesh to compute required resource type
                    
                    // Otherwise, just try to instantiate a render target same size as first texture input.
                    var resource = outEdges.Where(x => x.ProvidedTexture != null).Select(x => x.ProvidedTexture).FirstOrDefault();

                    if (resource == null)
                    {
                        TextureDescription? textureDescription = null;

                        // 1/ First, try to check if user forced texture description
                        var forcedTextureDescriptions = outEdges.Where(x => x.TextureDescription != null).Select(x => x.TextureDescription);

                        // TODO: Warning if more than one forcedTextureDescriptions
                        var forcedTextureDescription = forcedTextureDescriptions.FirstOrDefault();

                        if (forcedTextureDescription != null)
                        {
                            textureDescription = forcedTextureDescription;
                        }

                        // 2/ Otherwise, try to resolve using input textures
                        // TODO: More advanced logic (allowing use of customizable lambda functions)
                        if (textureDescription == null)
                        {
                            foreach (var inEdge in inEdges)
                            {
                                textureDescription = ((Texture) node.Parameters.GetObject(inEdge.TargetKey)).Description;
                                if (textureDescription != null) break;
                            }
                        }

                        // 3/ Otherwise, default texture
                        if (textureDescription == null)
                        {
                            // If nothing found, try to create a default render target.
                            // TODO add parameters for Width, Height, Format
                            textureDescription = new TextureDescription {Width = 1024, Height = 768, Format = PixelFormat.R8G8B8A8_UNorm};
                        }

                        // Either try to find a currently unused resources that matches this description
                        List<RenderTarget> matchingTextureList;
                        if (!textures.TryGetValue(textureDescription.Value, out matchingTextureList))
                            textures[textureDescription.Value] = matchingTextureList = new List<RenderTarget>();

                        resource = matchingTextureList.FirstOrDefault(x => !currentTextures.ContainsKey(x));

                        // If no available resource was found, create a new one.
                        if (resource == null)
                        {
                            resource = Texture.New2D(GraphicsDevice, textureDescription.Value).ToRenderTarget();
                            matchingTextureList.Add(resource);
                        }
                    }

                    // Add the resource as output of current node
                    node.Parameters.SetObject(outEdges.Key, resource);

                    foreach (var outEdge in outEdges)
                    {
                        // Add the resources as input of next nodes
                        outEdge.Target.Parameters.SetObject(outEdge.TargetKey, resource.Texture);
                        outEdge.Texture = resource;
                    }
                    if (!currentTextures.ContainsKey(resource))
                        currentTextures[resource] = outEdges.Count();
                    else
                        currentTextures[resource] += outEdges.Count();
                }

                foreach (var inEdge in inEdges)
                {
                    var resource = inEdge.Texture;
                    if (--currentTextures[resource] == 0)
                        currentTextures.Remove(resource);
                }
            }
        }

        private class PostEffectEdge : IEdge<EffectMesh>
        {
            public PostEffectEdge(EffectMesh source, ParameterKey<RenderTarget> sourceKey, EffectMesh target, ParameterKey<Texture> targetKey, RenderTarget texture)
            {
                Source = source;
                Target = target;
                SourceKey = sourceKey;
                TargetKey = targetKey;
                ProvidedTexture = texture;
            }

            public PostEffectEdge(EffectMesh source, ParameterKey<RenderTarget> sourceKey, EffectMesh target, ParameterKey<Texture> targetKey, TextureDescription? textureDescription = null)
            {
                Source = source;
                Target = target;
                SourceKey = sourceKey;
                TargetKey = targetKey;
                TextureDescription = textureDescription;
            }

            public EffectMesh Source { get; private set; }

            public EffectMesh Target { get; private set; }

            public ParameterKey SourceKey { get; private set; }

            public ParameterKey TargetKey { get; private set; }

            public RenderTarget Texture { get; set; }

            public RenderTarget ProvidedTexture { get; set; }

            public TextureDescription? TextureDescription { get; set; }
        }
    }
}
