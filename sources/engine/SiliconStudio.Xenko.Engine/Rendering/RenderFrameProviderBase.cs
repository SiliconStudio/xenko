// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Abstract implementation of <see cref="IRenderFrameProvider"/>.
    /// </summary>
    public abstract class RenderFrameProviderBase : IRenderFrameProvider
    {
        public abstract RenderFrame GetRenderFrame(RenderContext context);

        public virtual void Dispose()
        {
        }
    }
}