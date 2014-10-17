// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to export a texture to a Paradox <see cref="Image"/> instance.
    /// </summary>
    internal class ExportToParadoxRequest : IRequest
    {

        public override RequestType Type { get { return RequestType.ExportToParadox; } }

        /// <summary>
        /// The paradox <see cref="Image"/> which will contains the exported texture.
        /// </summary>
        public Image PdxImage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportToParadoxRequest"/> class.
        /// </summary>
        public ExportToParadoxRequest()
        {
        }
    }
}