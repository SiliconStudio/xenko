// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A compute scalar producing a scalar from a stream.
    /// </summary>
    [DataContract("ComputeVertexStreamScalar")]
    [Display("Vertex Stream")]
    public class ComputeVertexStreamScalar : ComputeVertexStreamBase, IComputeScalar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeVertexStreamScalar"/> class.
        /// </summary>
        public ComputeVertexStreamScalar()
        {
            Stream = new ColorVertexStreamDefinition();
            Channel = ColorChannel.R;
        }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>The channel.</value>
        /// <userdoc>Selects the RGBA channel to sample from the texture.</userdoc>
        [DataMember(20)]
        [DefaultValue(ColorChannel.R)]
        public ColorChannel Channel { get; set; }

        protected override string GetColorChannelAsString()
        {
            return MaterialUtility.GetAsShaderString(Channel);
        }
    }
}