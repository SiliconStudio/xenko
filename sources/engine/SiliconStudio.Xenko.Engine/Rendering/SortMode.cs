// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a way to sort RenderObject.
    /// </summary>
    [DataContract("SortMode")]
    public abstract class SortMode
    {
        public abstract unsafe void GenerateSortKey(RenderView renderView, RenderViewStage renderViewStage, SortKey* sortKeys);
    }
}