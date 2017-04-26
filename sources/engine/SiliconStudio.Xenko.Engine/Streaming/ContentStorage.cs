// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Streamable resources content storage containter.
    /// </summary>
    public class ContentStorage
    {
        private ContentChunk[] _chunks;

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                if (_chunks == null)
                    return DateTime.MinValue;

                var result = _chunks[0].LastAccessTime;
                for (int i = 1; i < _chunks.Length; i++)
                {
                    if (result < _chunks[i].LastAccessTime)
                        result = _chunks[i].LastAccessTime;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the chunk.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Chunk</returns>
        public ContentChunk GetChunk(int index)
        {
            Debug.Assert(_chunks != null && index >= 0 && _chunks.Length > index);

            var chunk = _chunks[index];
            chunk.RegisterUsage();
            return chunk;
        }
    }
}
