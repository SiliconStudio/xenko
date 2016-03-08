// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Abstract implementation of <see cref="IRenderFrameProvider"/>.
    /// </summary>
    [NonIdentifiable] // For now, we disable identifiable for all this class of objects, but we may renable it if they are used in lists
    public abstract class RenderFrameProviderBase : IRenderFrameProvider
    {
        public abstract RenderFrame GetRenderFrame(RenderContext context);

        public virtual void Dispose()
        {
        }
    }
}