// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Interface for a scene graphics renderer
    /// </summary>
    public interface ISceneRenderer : IGraphicsRenderer, IRenderCollector
    {
        /// <summary>
        /// Gets or sets the output of this effect
        /// </summary>
        /// <value>The output.</value>
        [NotNull]
        ISceneRendererOutput Output { get; set; }
    }
}