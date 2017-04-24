// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
