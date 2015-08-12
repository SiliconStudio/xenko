// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A user defined stream.
    /// </summary>
    /// <userdoc>
    /// A user defined stream.
    /// </userdoc>
    [DataContract("VertexUserStreamDefinition")]
    [Display("Custom Vertex Stream")]
    public class VertexUserStreamDefinition : VertexStreamDefinitionBase
    {
        /// <summary>
        /// Name of the semantic of the stream to read data from.
        /// </summary>
        /// <userdoc>
        /// Semantic name of the stream to read data from.
        /// </userdoc>
        [DataMember(10)]
        public string Name { get; set; }

        public override string GetSemanticName()
        {
            return Name;
        }
    }
}