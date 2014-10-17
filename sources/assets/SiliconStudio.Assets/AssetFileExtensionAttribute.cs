// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Associates a file extension (e.g '.pfxfont') with a particular <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetFileExtensionAttribute : Attribute
    {
        private readonly string fileExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetFileExtensionAttribute"/> class.
        /// </summary>
        /// <param name="fileExtensions">The extension.</param>
        public AssetFileExtensionAttribute(string fileExtensions)
        {
            this.fileExtensions = fileExtensions;
        }

        /// <summary>
        /// Gets the file extensions supported by a type of asset..
        /// </summary>
        /// <value>The extension.</value>
        public string FileExtensions
        {
            get
            {
                return fileExtensions;
            }
        }
    }
}