// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
