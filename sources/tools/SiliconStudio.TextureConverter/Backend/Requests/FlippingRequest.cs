// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to flip a texture vertically or horizontally
    /// </summary>
    internal class FlippingRequest : IRequest
    {

        public override RequestType Type { get { return RequestType.Flipping; } }


        /// <summary>
        /// The requested orientation flip
        /// </summary>
        public Orientation Flip { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FlippingRequest"/> class.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        public FlippingRequest(Orientation orientation)
        {
            this.Flip = orientation;
        }

    }
}
