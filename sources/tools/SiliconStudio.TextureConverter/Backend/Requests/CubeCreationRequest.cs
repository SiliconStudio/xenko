// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to create a texture cube from a texture list.
    /// </summary>
    class CubeCreationRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.CubeCreation; } }

        /// <summary>
        /// The texture list that will populate the cube.
        /// </summary>
        public List<TexImage> TextureList { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CubeCreationRequest"/> class.
        /// </summary>
        /// <param name="textureList">The texture list.</param>
        public CubeCreationRequest(List<TexImage> textureList)
        {
            TextureList = textureList;
        }
    }
}