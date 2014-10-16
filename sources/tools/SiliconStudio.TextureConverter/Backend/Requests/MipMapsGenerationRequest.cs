// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to generate the mipmap chain on a texture (3d texture mipmap generation not yet supported)
    /// </summary>
    internal class MipMapsGenerationRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.MipMapsGeneration; } }


        /// <summary>
        /// The filter to be used when rescaling to create the mipmaps of lower level.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public Filter.MipMapGeneration Filter { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MipMapsGenerationRequest"/> class.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public MipMapsGenerationRequest(Filter.MipMapGeneration filter)
        {
            Filter = filter;
        }
    }
}
