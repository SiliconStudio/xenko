// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Runtime.InteropServices;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct DepthStencilStencilOpDescription
    {
        /// <summary>
        /// Gets or sets the stencil operation to perform if the stencil test fails. The default is StencilOperation.Keep.
        /// </summary>
        public StencilOperation StencilFail { get; set; }

        /// <summary>
        /// Gets or sets the stencil operation to perform if the stencil test passes and the depth-test fails. The default is StencilOperation.Keep.
        /// </summary>
        public StencilOperation StencilDepthBufferFail { get; set; }
        
        /// <summary>
        /// Gets or sets the stencil operation to perform if the stencil test passes. The default is StencilOperation.Keep.
        /// </summary>
        public StencilOperation StencilPass { get; set; }
        
        /// <summary>
        /// Gets or sets the comparison function for the stencil test. The default is CompareFunction.Always.
        /// </summary>
        public CompareFunction StencilFunction { get; set; }
    }
}