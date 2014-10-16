// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to remove the texture at a specified position from a texture array.
    /// </summary>
    class ArrayElementRemovalRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.ArrayElementRemoval; } }

        /// <summary>
        /// The indice of the texture to be removed.
        /// </summary>
        public int Indice { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayElementRemovalRequest"/> class.
        /// </summary>
        /// <param name="indice">The indice of the texture to be removed.</param>
        public ArrayElementRemovalRequest(int indice)
        {
            Indice = indice;
        }
    }
}
