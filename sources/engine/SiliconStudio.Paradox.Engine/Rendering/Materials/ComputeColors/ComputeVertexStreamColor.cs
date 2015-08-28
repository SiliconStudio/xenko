// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A compute color producing a color from a stream.
    /// </summary>
    [DataContract("ComputeVertexStreamColor")]
    [Display("Vertex Stream")]
    public class ComputeVertexStreamColor : ComputeVertexStreamBase, IComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeVertexStreamColor"/> class.
        /// </summary>
        public ComputeVertexStreamColor()
        {
            Stream = new ColorVertexStreamDefinition();
        }

        protected override string GetColorChannelAsString()
        {
            // Use all channels
            return "rgba";
        }
    }
}