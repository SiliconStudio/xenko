// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Rendering
{
    // TODO GRAPHICS REFACTOR remove this class
    /// <summary>
    /// A component can be integrated into the rendering pipeline automatically if it defines 
    /// a <see cref="DefaultEntityComponentRendererAttribute"/> on its class definition.
    /// </summary>
    [Obsolete]
    public interface IEntityComponentRenderer : IEntityComponentRendererCore
    {
        /// <summary>
        /// Gets the value indicating whether the picking is supported by this renderer or not.
        /// </summary>
        bool SupportPicking { get; }

        /// <summary>
        /// Prepares a list of opaque and transparent <see cref="RenderItem"/>. See remarks.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="opaqueList">The opaque list.</param>
        /// <param name="transparentList">The transparent list.</param>
        /// <remarks>The implementation should fill the opaqueList and/or the transparentList of render items to render with a proper depth value</remarks>
        void Prepare(RenderDrawContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList);

        /// <summary>
        /// Draws the specified list of <see cref="RenderItem"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderItems">The render items.</param>
        /// <param name="fromIndex">From index.</param>
        /// <param name="toIndex">To index.</param>
        void Draw(RenderDrawContext context, RenderItemCollection renderItems, int fromIndex, int toIndex);
    }
}