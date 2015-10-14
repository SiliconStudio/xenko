// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Structured returned by <see cref="GraphicsDeviceContext.Map"/>.
    /// </summary>
    public struct GraphicsResourceMap
    {
        /// <summary>
        /// Pointer to the data of the <see cref="GraphicsResource"/> being mapped into the CPU memory.
        /// </summary>
        /// <remarks>
        /// If <see cref="GraphicsProfile"/> is set to low, data are aligned to 4 bytes else alignment is 16 bytes.
        /// </remarks>
        public IntPtr DataPointer;

        /// <summary>
        /// The row pitch, or width, or physical size (in bytes) of the data.
        /// </summary>
        public int RowPitch;

        /// <summary>
        /// The depth pitch, or width, or physical size (in bytes)of the data.
        /// </summary>
        public int DepthPitch;
    }
}
