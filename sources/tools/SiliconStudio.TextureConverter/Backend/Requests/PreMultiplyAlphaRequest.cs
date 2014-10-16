// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to premultiply the alpha on the texture
    /// </summary>
    class PreMultiplyAlphaRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.PreMultiplyAlpha; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreMultiplyAlphaRequest"/> class.
        /// </summary>
        public PreMultiplyAlphaRequest()
        {
        }
    }
}
