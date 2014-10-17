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
    /// Request to adjust gamma on the texture to a specified value
    /// </summary>
    internal class GammaCorrectionRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.GammaCorrection; } }

        /// <summary>
        /// The gamma value
        /// </summary>
        public double Gamma { private set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GammaCorrectionRequest"/> class.
        /// </summary>
        public GammaCorrectionRequest(double gamma)
        {
            Gamma = gamma;
        }
    }
}