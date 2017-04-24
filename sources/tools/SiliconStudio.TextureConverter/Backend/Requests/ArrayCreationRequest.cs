// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to create a texture array from a texture list.
    /// </summary>
    class ArrayCreationRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.ArrayCreation; } }

        /// <summary>
        /// The texture list that will populate the array.
        /// </summary>
        public List<TexImage> TextureList { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayCreationRequest"/> class.
        /// </summary>
        /// <param name="textureList">The texture list.</param>
        public ArrayCreationRequest(List<TexImage> textureList)
        {
            TextureList = textureList;
        }
    }
}
