// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to premultiply the alpha on the texture
    /// </summary>
    class ColorKeyRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.ColorKey; } }

        /// <summary>
        /// Gets or sets the color key.
        /// </summary>
        /// <value>The color key.</value>
        public Color ColorKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorKeyRequest"/> class.
        /// </summary>
        public ColorKeyRequest(Color colorKey)
        {
            ColorKey = colorKey;
        }
    }
}
