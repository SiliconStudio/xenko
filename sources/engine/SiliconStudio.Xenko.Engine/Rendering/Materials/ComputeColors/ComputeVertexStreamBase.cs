// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// Class ComputeVertexStreamBase.
    /// </summary>
    public abstract class ComputeVertexStreamBase : ComputeNode, IComputeVertexStream
    {
        [DataMember(10)]
        [NotNull]
        [InlineProperty]
        public IVertexStreamDefinition Stream { get; set; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var channel = GetColorChannelAsString();
            return Stream == null || string.IsNullOrWhiteSpace(Stream.GetSemanticName()) ? new ShaderClassSource("ComputeColor") : new ShaderClassSource("ComputeColorFromStream", Stream.GetSemanticName(), channel);
        }

        protected abstract string GetColorChannelAsString();
    }
}