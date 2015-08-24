// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Class SourceCodeAsset.
    /// </summary>
    [DataContract("SourceCodeAsset")]
    public abstract class SourceCodeAsset : Asset
    {
        /// <summary>
        /// Gets or sets the absolute source location of this asset on the disk.
        /// </summary>
        /// <value>The absolute source location.</value>
        [Browsable(false)]
        public string AbsoluteSourceLocation { get; set; }

        /// <summary>
        /// Loads the underlying content located at <see cref="AbsoluteSourceLocation"/> if necessary.
        /// </summary>
        public virtual void Load()
        {
        }

        /// <summary>
        /// Saves the underlying content located at <see cref="AbsoluteSourceLocation"/> if necessary.
        /// </summary>
        public virtual void Save()
        {
        }

        /// <summary>
        /// Generates a unique identifier from location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Guid.</returns>
        public static Guid GenerateGuidFromLocation(string location)
        {
            if (location == null) throw new ArgumentNullException("location");
            return ObjectId.FromBytes(Encoding.UTF8.GetBytes(location)).ToGuid();
        }
    }
}