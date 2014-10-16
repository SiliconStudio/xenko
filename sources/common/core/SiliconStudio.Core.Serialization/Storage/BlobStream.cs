// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Storage
{
    /// <summary>
    /// A read-only <see cref="NativeMemoryStream"/> that will properly keep references on its underlying <see cref="Blob"/>.
    /// </summary>
    class BlobStream : NativeMemoryStream
    {
        private Blob blob;

        public BlobStream(Blob blob) : base(blob.Content, blob.Size)
        {
            this.blob = blob;

            // Keep a reference on the blob while its data is used.
            this.blob.AddReference();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Release reference on the blob
            blob.Release();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(IntPtr buffer, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }
}