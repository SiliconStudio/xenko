// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;

namespace SiliconStudio.Xenko.Games.Testing
{
    class BlockingBufferStream : Stream
    {
        private readonly Stream innerStream;

        /// <inheritdoc/>
        public BlockingBufferStream(Stream innerStream)
        {
            this.innerStream = innerStream;
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get { return innerStream.CanRead; }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get { return innerStream.CanSeek; }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get { return innerStream.CanWrite; }
        }

        /// <inheritdoc/>
        public override bool CanTimeout
        {
            get { return innerStream.CanTimeout; }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get { return innerStream.Length; }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get { return innerStream.Position; }
            set { innerStream.Position = value; }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Read in multiple steps if necessary
            var totalByteRead = 0;
            while (count > 0)
            {
                var byteRead = innerStream.Read(buffer, offset, count);
                if (byteRead == 0 && totalByteRead == 0)
                    return 0;

                offset += byteRead;
                count -= byteRead;
                totalByteRead += byteRead;
            }

            return totalByteRead;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            return innerStream.ReadByte();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            innerStream.WriteByte(value);
        }

#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME

        /// <inheritdoc/>
        public override void Close()
        {
            innerStream.Close();
            base.Close();
        }

#endif

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                innerStream.Dispose();
            base.Dispose(disposing);
        }
    }
}