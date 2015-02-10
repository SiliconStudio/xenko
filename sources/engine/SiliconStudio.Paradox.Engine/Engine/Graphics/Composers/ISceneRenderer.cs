// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Defines the common interface for a graphics composer responsible to compose the scene to a final render target.
    /// </summary>
    public interface ISceneRenderer
    {
        // TODO: Add Disposable for composer

        /// <summary>
        /// Gets or sets the output of the composer.
        /// </summary>
        /// <value>The output.</value>
        IGraphicsComposerOutput Output { get; set; }

        /// <summary>
        /// Draws this composer.
        /// </summary>
        /// <param name="context">The context.</param>
        void Draw(RenderContext context);
    }
}