// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines the type of rendering (Forward, Deferred...etc.)
    /// </summary>
    [DataContract("CameraRendererMode")]
    public abstract class CameraRendererMode : RendererBase, INextGenRenderer
    {
        /// <summary>
        /// Gets or sets the effect to use to render the models in the scene.
        /// </summary>
        /// <value>The main model effect.</value>
        /// <userdoc>The name of the effect to use to render models (a '.xksl' or '.xkfx' filename without the extension).</userdoc>
        [DataMember(10)]
        public abstract string ModelEffect { get; set; }// TODO: This is not a good extensibility point. Check how to improve this

        /// <param name="context"></param>
        /// <inheritdoc/>
        public virtual void BeforeExtract(RenderContext context)
        {
            EnsureContext(context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
        }
    }
}