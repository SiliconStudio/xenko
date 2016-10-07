// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Class SourceCodeAsset.
    /// </summary>
    [DataContract("SourceCodeAsset")]
    public abstract class SourceCodeAsset : Asset
    {
        protected SourceCodeAsset()
        {
            // Default: fallback to internal storage
            TextAccessor = new DefaultTextAccessor(this);
        }

        [DataMemberIgnore]
        [Display(Browsable = false)]
        public ITextAccessor TextAccessor { get; set; }

        /// <summary>
        /// Gets the sourcecode text.
        /// </summary>
        /// <value>The sourcecode text.</value>
        [DataMemberIgnore]
        [Display(Browsable = false)]
        public string Text
        {
            get
            {
                return TextAccessor.Get();
            }
            set
            {
                TextAccessor.Set(value);
            }
        }

        /// <summary>
        /// Saves the content to as stream.
        /// </summary>
        /// <param name="stream"></param>
        public virtual void Save(Stream stream)
        {
            TextAccessor.WriteTo(stream);
        }

        /// <summary>
        /// Generates a unique identifier from location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Guid.</returns>
        public static Guid GenerateGuidFromLocation(string location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            return ObjectId.FromBytes(Encoding.UTF8.GetBytes(location)).ToGuid();
        }

        public interface ITextAccessor
        {
            /// <summary>
            /// Gets the underlying text.
            /// </summary>
            /// <returns></returns>
            string Get();

            /// <summary>
            /// Sets the underlying text.
            /// </summary>
            /// <param name="value"></param>
            void Set(string value);

            /// <summary>
            /// Writes the text to the given <see cref="StreamWriter"/>.
            /// </summary>
            /// <param name="streamWriter"></param>
            Task WriteTo(Stream streamWriter);
        }

        public class DefaultTextAccessor : ITextAccessor
        {
            private readonly SourceCodeAsset sourceCodeAsset;
            private string text;

            public DefaultTextAccessor(SourceCodeAsset sourceCodeAsset)
            {
                this.sourceCodeAsset = sourceCodeAsset;
            }

            public string FilePath { get; internal set; }

            /// <inheritdoc/>
            public string Get()
            {
                return text ?? (text = (FilePath != null ? LoadFromFile() : FilePath) ?? "");
            }

            /// <inheritdoc/>
            public void Set(string value)
            {
                text = value;
            }

            public async Task WriteTo(Stream stream)
            {
                if (text != null)
                {
                    using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        await streamWriter.WriteAsync(text);
                    }
                }
                else if (FilePath != null)
                {
                    using (var inputStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, bufferSize: 4096, useAsync: true))
                    {
                        await inputStream.CopyToAsync(stream);
                    }
                }
            }

            private string LoadFromFile()
            {
                if (FilePath == null)
                    return null;

                try
                {
                    return File.ReadAllText(FilePath);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
