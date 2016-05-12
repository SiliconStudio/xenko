// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Resolves a RenderTarget from one render pass to be used as an input to another render pass
    /// </summary>
    public abstract class BufferResolver
    {
        /// <summary>
        /// Resolve the assigned buffer, making it available as SRV
        /// </summary>
        public abstract void Resolve(RenderDrawContext renderContext, Texture texture);

        /// <summary>
        /// Gets the assigned buffer as a Render Target
        /// </summary>
        /// <returns>Resource view to the buffer</returns>
        public abstract Texture AsRenderTarget();

        /// <summary>
        /// Gets the assigned buffer as a Shader Resource View
        /// </summary>
        /// <returns><c>null</c> if unavailable, the resource view as a shader resource if available</returns>
        public abstract Texture AsShaderResourceView();

        public void Reset()
        {
            
        }
    }
}
