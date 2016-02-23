// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using System.IO;
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
        private string text;

        protected SourceCodeAsset()
        {
        }

        /// <summary>
        /// Gets or sets the absolute source location of this asset on the disk.
        /// </summary>
        /// <value>The absolute source location.</value>
        [Display(Browsable = false)]
        public string AbsoluteSourceLocation { get; set; }

        /// <summary>
        /// Gets the sourcecode text.
        /// </summary>
        /// <value>The sourcecode text.</value>
        [DataMemberIgnore]
        public string Text
        {
            get
            {
                return text ?? (text = Load()); // Lazy loading
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// Saves the underlying content located at <see cref="AbsoluteSourceLocation"/> if necessary.
        /// </summary>
        /// <param name="stream"></param>
        public virtual void Save(Stream stream)
        {
            // If the text was not loaded in memory, just copy the stream from input to output
            if (text == null)
            {
                if (!string.IsNullOrEmpty(AbsoluteSourceLocation) && File.Exists(AbsoluteSourceLocation))
                {
                    using (var inputStream = new FileStream(AbsoluteSourceLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        inputStream.CopyTo(stream);
                    }
                }
            }
            else
            { 
                // Otherwise save the text direcly
                var buffer = Encoding.UTF8.GetBytes(Text);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Loads the underlying content located at <see cref="AbsoluteSourceLocation"/> if necessary.
        /// </summary>
        private string Load()
        {
            if (!string.IsNullOrEmpty(AbsoluteSourceLocation) && File.Exists(AbsoluteSourceLocation))
            {
                return File.ReadAllText(AbsoluteSourceLocation);
            }
            return null;
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