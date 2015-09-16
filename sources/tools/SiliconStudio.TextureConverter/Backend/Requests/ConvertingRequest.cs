// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        public SiliconStudio.Paradox.Graphics.PixelFormat Format { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertingRequest"/> class.
        /// </summary>
        /// <param name="format">The destination format.</param>
        public ConvertingRequest(SiliconStudio.Paradox.Graphics.PixelFormat format)
        {
            this.Format = format;
        }
    }
}
