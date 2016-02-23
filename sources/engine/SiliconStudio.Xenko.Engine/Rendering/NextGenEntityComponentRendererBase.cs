// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A default implementation for a <see cref="INextGenEntityComponentRenderer"/>.
    /// </summary>
    public abstract class NextGenEntityComponentRendererBase : EntityComponentRendererCoreBase, INextGenEntityComponentRenderer
    {
        public abstract void Extract(NextGenRenderSystem renderSystem);

        public abstract void Draw(NextGenRenderSystem renderSystem);
    }
}