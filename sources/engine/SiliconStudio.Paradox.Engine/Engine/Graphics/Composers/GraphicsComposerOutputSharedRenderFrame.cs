// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A link to a shared <see cref="RenderFrame"/>.
    /// </summary>
    [DataContract("GraphicsComposerOutputSharedRenderFrame")]
    [Display("Shared RenderFrame")]
    public sealed class GraphicsComposerOutputSharedRenderFrame : IGraphicsComposerOutput
    {
        /// <summary>
        /// Gets or sets the shared RenderFrame.
        /// </summary>
        /// <value>The shared RenderFrame.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public RenderFrame RenderFrame { get; set; }

        public void Dispose()
        {
        }

        public RenderFrame GetRenderFrame(RenderContext context)
        {
            return RenderFrame;
        }
    }
}