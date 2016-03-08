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
    public abstract class CameraRendererMode : RendererBase, IRenderCollector
    {
        /// <param name="context"></param>
        /// <inheritdoc/>
        public virtual void Collect(RenderContext context)
        {
            EnsureContext(context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
        }
    }
}