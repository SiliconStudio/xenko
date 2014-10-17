// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to decompress a texture into R8G8B8A8
    /// </summary>
    internal class DecompressingRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.Decompressing; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompressingRequest"/> class.
        /// </summary>
        public DecompressingRequest()
        {
        }

    }
}
