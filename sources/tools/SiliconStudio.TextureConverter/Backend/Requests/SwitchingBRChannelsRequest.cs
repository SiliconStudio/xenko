// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to switch the R and B channels on a texture.
    /// </summary>
    internal class SwitchingBRChannelsRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.SwitchingChannels; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchingBRChannelsRequest"/> class.
        /// </summary>
        public SwitchingBRChannelsRequest()
        {
        }
    }
}