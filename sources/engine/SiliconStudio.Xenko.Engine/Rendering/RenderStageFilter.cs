// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a way to filter RenderObject.
    /// </summary>
    [DataContract("RenderStageFilter")]
    public abstract class RenderStageFilter
    {
        public abstract bool IsVisible(RenderObject renderObject, RenderView renderView, RenderViewStage renderViewStage);
    }
}
