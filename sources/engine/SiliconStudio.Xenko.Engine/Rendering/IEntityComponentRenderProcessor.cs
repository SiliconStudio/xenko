// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// An <see cref="EntityProcessor"/> dedicated for rendering.
    /// </summary>
    /// Note that it might be instantiated multiple times in a given <see cref="SceneInstance"/>.
    public interface IEntityComponentRenderProcessor
    {
        VisibilityGroup VisibilityGroup { get; set; }
    }
}
