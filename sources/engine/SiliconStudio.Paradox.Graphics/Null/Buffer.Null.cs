// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class Buffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description">The description.</param>
        /// <param name="bufferFlags">Type of the buffer.</param>
        /// <param name="viewFormat">The view format.</param>
        /// <param name="dataPointer">The data pointer.</param>
        protected Buffer(GraphicsDevice device, BufferDescription description, BufferFlags bufferFlags, PixelFormat viewFormat, IntPtr dataPointer) : base(device)
        {
            Description = description;
            BufferFlags = bufferFlags;
            ViewFormat = viewFormat;
            //InitCountAndViewFormat(out this.elementCount, ref ViewFormat);
            //Initialize(device.RootDevice, null);
        }
    }
}
 
#endif 
