// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to convert a texture to the specified format.
    /// </summary>
    internal class ConvertingRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.Converting; } }


        /// <summary>
        /// The destination format.
        /// </summary>
        public SiliconStudio.Xenko.Graphics.PixelFormat Format { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertingRequest"/> class.
        /// </summary>
        /// <param name="format">The destination format.</param>
        public ConvertingRequest(SiliconStudio.Xenko.Graphics.PixelFormat format)
        {
            this.Format = format;
        }
    }
}
