// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering
{
    public enum DepthBufferPolicy
    {
        /// <summary>
        /// The DepthStencil buffer is enabled for testing and writing
        /// </summary>
        Enabled,

        /// <summary>
        /// The DepthStencil buffer is disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// The DepthStencil buffer is enabled only for testing, not for writing
        /// </summary>
        ReadOnly,
    }
}
