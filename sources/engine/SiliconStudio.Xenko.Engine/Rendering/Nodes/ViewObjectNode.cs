// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a <see cref="RenderObject"/> from a specific view.
    /// </summary>
    public struct ViewObjectNode
    {
        /// <summary>
        /// Access underlying RenderObject.
        /// </summary>
        public readonly RenderObject RenderObject;

        // TODO: This can properly be removed and stored as a RenderView, [RenderPerViewNode start..end]
        public readonly RenderView RenderView;

        /// <summary>
        /// The object node reference.
        /// </summary>
        public readonly ObjectNodeReference ObjectNode;

        public ViewObjectNode(RenderObject renderObject, RenderView renderView, ObjectNodeReference objectNode)
        {
            RenderObject = renderObject;
            RenderView = renderView;
            ObjectNode = objectNode;
        }
    }
}
