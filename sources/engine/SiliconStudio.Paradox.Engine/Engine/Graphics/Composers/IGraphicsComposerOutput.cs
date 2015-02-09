// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Defines the output of a <see cref="IGraphicsComposer"/>.
    /// </summary>
    public interface IGraphicsComposerOutput
    {
    }

    [DataContract("GraphicsComposerOutputTexture")]
    [Display("RenderFrame")]
    public class GraphicsComposerOutputTexture : IGraphicsComposerOutput
    {
        public RenderFrameDescriptor FrameDescriptor;
    }

    [DataContract("GraphicsComposerOutputSharedTexture")]
    [Display("Shared RenderFrame")]
    public class GraphicsComposerOutputSharedTexture : IGraphicsComposerOutput
    {
        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        /// <value>The texture.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public RenderFrame Texture { get; set; }
    }

}