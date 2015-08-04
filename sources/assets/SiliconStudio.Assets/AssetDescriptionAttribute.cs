// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Associates meta-information to a particular <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetDescriptionAttribute : Attribute
    {
        private readonly string fileExtensions;
        private readonly bool allowUserCreation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="fileExtensions">The file extensions supported by a type of asset.</param>
        /// <param name="allowUserCreation">Indicate whether this asset can be created by users using engine tools.</param>
        public AssetDescriptionAttribute(string fileExtensions, bool allowUserCreation = true)
        {
            this.fileExtensions = fileExtensions;
            this.allowUserCreation = allowUserCreation;
        }

        /// <summary>
        /// Gets the file extensions supported by a type of asset.
        /// </summary>
        /// <value>The extension.</value>
        public string FileExtensions { get { return fileExtensions; } }

        /// <summary>
        /// Gets whether this asset can be created by users using engine tools.
        /// </summary>
        public bool AllowUserCreation { get { return allowUserCreation; } }
    }
}