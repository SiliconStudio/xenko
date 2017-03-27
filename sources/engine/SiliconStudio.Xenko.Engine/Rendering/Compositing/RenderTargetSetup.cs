// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Represents how we setup the graphics pipeline output targets.
    /// </summary>
    public sealed class RenderTargetSetup
    {
        private readonly Dictionary<int, ShaderSourceCollection> shaderSourceCache = new Dictionary<int, ShaderSourceCollection>();

        private readonly FastList<RenderTarget> renderTargetDescriptions = new FastList<RenderTarget>();

        private Texture[] composition;

        public IReadOnlyList<RenderTarget> RenderTargetDescriptions => renderTargetDescriptions;

        public Texture[] RenderTargets
        {
            get
            {
                if (composition == null)
                {
                    composition = renderTargetDescriptions.Select(x => x.Texture).ToArray();
                }

                return composition;
            }
        }

        public ShaderSourceCollection ShaderSources
        {
            get
            {
                int shaderSourceHash = 0;

                // start at slot 1, to mirror what XenkoMRT.xkfx expects.
                for (int renderTargetIndex = 1; renderTargetIndex < renderTargetDescriptions.Count; renderTargetIndex++)
                {
                    if (RenderTargetDescriptions[renderTargetIndex].Description.Semantic == null)
                        continue;

                    string classToMixin = RenderTargetDescriptions[renderTargetIndex].Description.Semantic.ShaderClass;
                    int nameHash = classToMixin.GetHashCode();
                    shaderSourceHash = HashCombine(shaderSourceHash, nameHash);
                }

                ShaderSourceCollection shaderSources;
                shaderSourceCache.TryGetValue(shaderSourceHash, out shaderSources);
                if (shaderSources == null)
                {
                    shaderSources = new ShaderSourceCollection();
                    for (int renderTargetIndex = 1; renderTargetIndex < renderTargetDescriptions.Count; renderTargetIndex++)
                    {
                        string shaderSource = RenderTargetDescriptions[renderTargetIndex].Description.Semantic?.ShaderClass;
                        shaderSources.Add(shaderSource != null ? new ShaderClassSource(shaderSource) : null);
                    }
                    shaderSourceCache.Add(shaderSourceHash, shaderSources);
                }

                return shaderSources;
            }
        }

        /// <summary>
        /// Resets the composition (unplug all targets).
        /// </summary>
        public void Clear()
        {
            renderTargetDescriptions.Clear();
            composition = null;
        }

        public void AddTarget(RenderTarget target)
        {
            renderTargetDescriptions.Add(target);
            if (renderTargetDescriptions.Count - 1 != FindIndexBySemantic(target.Description.Semantic.GetType()))
                throw new InvalidOperationException("Uniquely inserted semantics invariant would be broken");

            composition = null;
        }

        public void Copy(RenderTargetSetup source)
        {
            Clear();

            foreach (var sourceTarget in source.renderTargetDescriptions)
                AddTarget(sourceTarget);
        }

        /// <summary>
        /// Shortcut to avoid copying the full render target struct to mutate just one field.
        /// </summary>
        /// <param name="semantic">lookup key</param>
        public void SetTextureParams(Type semantic, RenderTargetTextureCreationParams creationParams)
        {
            int index = FindIndexBySemantic(semantic);
            if (index == -1)
                return;

            renderTargetDescriptions.Items[index].Description.RenderTargetTextureParams = creationParams;
        }

        public void SetTextureParams<T>(RenderTargetTextureCreationParams creationParams)
        {
            SetTextureParams(typeof(T), creationParams);
        }

        /// <summary>
        /// Shortcut to avoid copying the full render target struct to mutate just one field.
        /// </summary>
        /// <param name="semantic">lookup key</param>
        public void SetTexture(Type semantic, Texture texture)
        {
            int index = FindIndexBySemantic(semantic);
            if (index == -1)
                throw new InvalidOperationException($"Semantic {semantic} is not defined");

            renderTargetDescriptions.Items[index].Texture = texture;
        }

        /// <summary>
        /// Queries the mapping between the given semantic and the shader source class name.
        /// </summary>
        /// <param name="semantic">type of the semantic you are looking for</param>
        /// <returns>string containing the mixin name</returns>
        public string GetShaderClass(Type semantic)
        {
            return GetRenderTarget(semantic).Description.Semantic?.ShaderClass;
        }

        public bool IsActive(Type semantic)
        {
            return FindIndexBySemantic(semantic) != -1;
        }

        /// <param name="semantic">type of the semantic you are looking for</param>
        /// <returns>A render target with its full description. throws if not found.</returns>
        /// <remarks>Note that the returned type is a value type, so write access will not be reflected.</remarks>
        public RenderTarget GetRenderTarget(Type semantic)
        {
            var index = FindIndexBySemantic(semantic);
            if (index == -1)
                throw new InvalidOperationException($"Semantic {semantic} is not defined");

            return renderTargetDescriptions[index];
        }

        private int FindIndexBySemantic(Type semanticType)
        {
            for (int index = 0; index < renderTargetDescriptions.Count; ++index)
            {
                if (renderTargetDescriptions[index].Description.Semantic.GetType() == semanticType)
                    return index;
            }

            return -1;
        }

        private static int HashCombine(int hash1, int hash2)
        {
            return (hash1 * 397) ^ hash2;
        }
    }
}
