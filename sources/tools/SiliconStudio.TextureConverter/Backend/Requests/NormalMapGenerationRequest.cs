// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.TextureConverter.Requests
{
    class NormalMapGenerationRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.NormalMapGeneration; } }


        public float Amplitude { get; private set; }
        public TexImage NormalMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalMapGenerationRequest"/> class.
        /// </summary>
        public NormalMapGenerationRequest(float amplitude)
        {
            Amplitude = amplitude;
        }
    }
}