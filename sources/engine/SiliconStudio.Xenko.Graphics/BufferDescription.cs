// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes a buffer.
    /// </summary>
    public struct BufferDescription
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BufferDescription"/> struct.
        /// </summary>
        /// <param name="sizeInBytes">Size of the buffer in bytes.</param>
        /// <param name="bufferFlags">Buffer flags describing the type of buffer.</param>
        /// <param name="usage">Usage of this buffer.</param>
        /// <param name="structureByteStride">The size of the structure (in bytes) when it represents a structured/typed buffer. Default = 0.</param>
        public BufferDescription(int sizeInBytes, BufferFlags bufferFlags, GraphicsResourceUsage usage, int structureByteStride = 0)
        {
            SizeInBytes = sizeInBytes;
            BufferFlags = bufferFlags;
            Usage = usage;
            StructureByteStride = structureByteStride;
        }

        /// <summary>	
        /// Size of the buffer in bytes.
        /// </summary>	
        public int SizeInBytes;

        /// <summary>	
        /// Buffer flags describing the type of buffer.
        /// </summary>	
        public BufferFlags BufferFlags;

        /// <summary>	
        /// Usage of this buffer.
        /// </summary>	
        public GraphicsResourceUsage Usage;

        /// <summary>	
        /// The size of the structure (in bytes) when it represents a structured/typed buffer.
        /// </summary>	
        public int StructureByteStride;
    }
}