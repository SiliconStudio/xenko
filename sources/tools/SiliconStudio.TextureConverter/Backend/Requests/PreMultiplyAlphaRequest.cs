// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
