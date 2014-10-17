// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to compress a texture to a specified format
    /// </summary>
    internal class CompressingRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.Compressing; } }


        /// <summary>
        /// The format.
        /// </summary>
        public SiliconStudio.Paradox.Graphics.PixelFormat Format { get; private set; }

        /// <summary>
        /// Gets the quality.
        /// </summary>
        /// <value>The quality.</value>
        public TextureQuality Quality { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressingRequest"/> class.
        /// </summary>
        /// <param name="format">The compression format.</param>
        public CompressingRequest(SiliconStudio.Paradox.Graphics.PixelFormat format, TextureQuality quality = TextureQuality.Fast)
        {
            this.Format = format;
            this.Quality = quality;
        }
    }
}
