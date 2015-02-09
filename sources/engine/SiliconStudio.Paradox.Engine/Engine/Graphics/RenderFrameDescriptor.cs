// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A descriptor used to create a <see cref="RenderFrame"/>.
    /// </summary>
    [DataContract("RenderFrameDescriptor")]
    public class RenderFrameDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFrameDescriptor"/> class.
        /// </summary>
        public RenderFrameDescriptor()
        {
            Mode = RenderFrameSizeMode.Relative;
            Width = 100;
            Height = 100;
            Format = RenderFrameFormat.LDR;
            DepthFormat = RenderFrameDepthFormat.None;
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        [DataMember(10)]
        [DefaultValue(RenderFrameSizeMode.Relative)]
        public RenderFrameSizeMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the width, in pixels when <see cref="Mode"/> is <see cref="RenderFrameSizeMode.Fixed"/> 
        /// or in percentage when <see cref="RenderFrameSizeMode.Relative"/>
        /// </summary>
        /// <value>The width.</value>
        [DataMember(20)]
        [DefaultValue(100)]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height, in pixels when <see cref="Mode"/> is <see cref="RenderFrameSizeMode.Fixed"/> 
        /// or in percentage when <see cref="RenderFrameSizeMode.Relative"/>
        /// </summary>
        /// <value>The height.</value>
        [DataMember(30)]
        [DefaultValue(100)]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the pixel format of this render frame.
        /// </summary>
        /// <value>The format.</value>
        [DataMember(40)]
        [DefaultValue(RenderFrameFormat.LDR)]
        public RenderFrameFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the depth format.
        /// </summary>
        /// <value>The depth format.</value>
        [DataMember(50)]
        [DefaultValue(RenderFrameDepthFormat.None)]
        public RenderFrameDepthFormat DepthFormat { get; set; }
    }
}