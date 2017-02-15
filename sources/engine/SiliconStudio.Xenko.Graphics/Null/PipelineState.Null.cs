// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 

namespace SiliconStudio.Xenko.Graphics
{
    public partial class PipelineState
    {
        /// <summary>
        /// Initializes new instance of <see cref="PipelineState"/> for <param name="device"/>
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="pipelineStateDescription">The pipeline state description.</param>
        private PipelineState(GraphicsDevice device, PipelineStateDescription pipelineStateDescription) : base(device)
        {
            NullHelper.ToImplement();
        }
    }
}

#endif
