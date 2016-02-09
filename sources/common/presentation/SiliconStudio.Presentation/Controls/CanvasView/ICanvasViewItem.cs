// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Presentation.Controls
{
    public interface ICanvasViewItem
    {
        void Attach(CanvasView view);

        void Detach(CanvasView view);

        /// <summary>
        /// Renders the canvas with the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="width">The available width.</param>
        /// <param name="height">The available height.</param>
        void Render(CanvasRenderer renderer, double width, double height);
    }
}
