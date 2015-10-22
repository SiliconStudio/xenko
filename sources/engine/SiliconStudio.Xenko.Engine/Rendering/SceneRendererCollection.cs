// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A collection of <see cref="IGraphicsRenderer"/>.
    /// </summary>
    [DataContract("SceneRendererCollection")]
    public sealed class SceneRendererCollection : GraphicsRendererCollection<ISceneRenderer>
    {
    }
}