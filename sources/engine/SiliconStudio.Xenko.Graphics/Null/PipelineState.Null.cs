// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
