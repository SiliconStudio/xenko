// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Class SourceCodeAsset.
    /// </summary>
    [DataContract("SourceCodeAsset")]
    public abstract class SourceCodeAsset : Asset
    {
        [DataMemberIgnore]
        [Display(Browsable = false)]
        public ITextAccessor TextAccessor { get; set; } = new DefaultTextAccessor();

        /// <summary>
        /// Used internally by serialization.
        /// </summary>
        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public ISerializableTextAccessor InternalSerializableTextAccessor
        {
            get { return TextAccessor.GetSerializableVersion(); }
            internal set { TextAccessor = value.Create(); }
        }

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
            TextAccessor.WriteTo(stream).Wait();
        }

        /// <summary>
        /// Generates a unique identifier from location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Guid.</returns>
        public static AssetId GenerateIdFromLocation(string location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            return (AssetId)ObjectId.FromBytes(Encoding.UTF8.GetBytes(location)).ToGuid();
        }

        public interface ISerializableTextAccessor
        {
            ITextAccessor Create();
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

            ISerializableTextAccessor GetSerializableVersion();
        }

        [DataContract]
        public class FileTextAccessor : ISerializableTextAccessor
        {
            public string FilePath { get; set; }

            public ITextAccessor Create()
            {
                return new DefaultTextAccessor { FilePath = FilePath };
            }
        }

        [DataContract]
        public class StringTextAccessor : ISerializableTextAccessor
        {
            public string Text { get; set; }

            public ITextAccessor Create()
            {
                var result = new DefaultTextAccessor();
                result.Set(Text);
                return result;
            }
        }

        public class DefaultTextAccessor : ITextAccessor
        {
            private string text;

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

            public ISerializableTextAccessor GetSerializableVersion()
            {
                // Still not loaded?
                if (text == null && FilePath != null)
                    return new FileTextAccessor { FilePath = FilePath };

                return new StringTextAccessor { Text = text };
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
