// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.UI.Renderers
{
    /// <summary>
    /// A factory that can create <see cref="ElementRenderer"/>s.
    /// </summary>
    public interface IElementRendererFactory
    {
        /// <summary>
        /// Try to create a renderer for the specified element.
        /// </summary>
        /// <param name="element">The element to render</param>
        /// <returns>An instance of renderer that can render it.</returns>
        ElementRenderer TryCreateRenderer(UIElement element);
    }
}