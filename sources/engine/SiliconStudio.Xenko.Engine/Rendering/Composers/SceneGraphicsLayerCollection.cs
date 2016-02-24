// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// A Collection of <see cref="SceneGraphicsLayer"/>.
    /// </summary>
    [DataContract("SceneGraphicsLayerCollection")]
    public sealed class SceneGraphicsLayerCollection : GraphicsRendererCollection<SceneGraphicsLayer>, INextGenRenderer
    {
        /// <inheritdoc/>
        public void BeforeExtract(RenderContext context)
        {
            InitializeRenderers(context);

            // Draw all renderers
            foreach (var renderer in this)
            {
                if (renderer.Enabled && !renderer.Faulted)
                {
                    // Draw the renderer
                    renderer.BeforeExtract(context);
                }
            }
        }
    }
}