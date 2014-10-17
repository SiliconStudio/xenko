// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to load a texture, either from a file, or from memory with an <see cref="TexImage"/> or a <see cref="SiliconStudio.Paradox.Graphics.Image"/>
    /// </summary>
    internal class LoadingRequest : IRequest
    {

        /// <summary>
        /// The different loading mode : TexImage, file, Paradox Image
        /// </summary>
        public enum LoadingMode
        {
            TexImage,
            PdxImage,
            FilePath,
        }

        public override RequestType Type { get { return RequestType.Loading; } }

        /// <summary>
        /// The mode used by the request
        /// </summary>
        public LoadingMode Mode { set; get; }


        /// <summary>
        /// The file path
        /// </summary>
        public String FilePath { set; get; }


        /// <summary>
        /// The TexImage to be loaded
        /// </summary>
        public TexImage Image { set; get; }

        /// <summary>
        /// The Paradox Image to be loaded
        /// </summary>
        public SiliconStudio.Paradox.Graphics.Image PdxImage;

        /// <summary>
        /// Indicate if we should keep the original mip-maps during the load
        /// </summary>
        public bool KeepMipMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingRequest"/> class to load a texture from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public LoadingRequest(String filePath)
        {
            this.FilePath = filePath;
            this.Mode = LoadingMode.FilePath;
            KeepMipMap = false;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingRequest"/> class to load a texture from a <see cref="TexImage"/> instance.
        /// </summary>
        /// <param name="image">The image.</param>
        public LoadingRequest(TexImage image)
        {
            this.Image = image;
            this.Mode = LoadingMode.TexImage;
            KeepMipMap = false;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingRequest"/> class to load a texture from a <see cref="SiliconStudio.Paradox.Graphics.Image"/> instance.
        /// </summary>
        /// <param name="image">The image.</param>
        public LoadingRequest(SiliconStudio.Paradox.Graphics.Image image)
        {
            this.PdxImage = image;
            this.Mode = LoadingMode.PdxImage;
            KeepMipMap = false;
        }

    }
}
