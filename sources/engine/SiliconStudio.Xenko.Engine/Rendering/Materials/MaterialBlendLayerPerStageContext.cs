// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Defines the context used by a stage for a layer.
    /// </summary>
    internal class MaterialBlendLayerPerStageContext
    {
        public MaterialBlendLayerPerStageContext()
        {
            ShaderSources = new List<ShaderSource>();
            StreamInitializers = new List<string>();
            Streams = new HashSet<string>();
        }

        public List<ShaderSource> ShaderSources { get; }

        public List<string> StreamInitializers { get; }

        public HashSet<string> Streams { get; }

        public void Reset()
        {
            ShaderSources.Clear();
            StreamInitializers.Clear();
            Streams.Clear();
        }

        /// <summary>
        /// Squash <see cref="ShaderSources"/> to a single ShaderSource (compatible with IComputeColor)
        /// </summary>
        /// <returns>The squashed <see cref="ShaderSource"/> or null if nothing to squash</returns>
        public ShaderSource ComputeShaderSource()
        {
            if (ShaderSources.Count == 0)
            {
                return null;
            }

            ShaderSource result;
            // If there is only a single op, don't generate a mixin
            if (ShaderSources.Count == 1)
            {
                result = ShaderSources[0];
            }
            else
            {
                var mixin = new ShaderMixinSource();
                result = mixin;
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceArray"));

                // Squash all operations into MaterialLayerArray
                foreach (var operation in ShaderSources)
                {
                    mixin.AddCompositionToArray("layers", operation);
                }
            }
            return result;
        }
    }
}