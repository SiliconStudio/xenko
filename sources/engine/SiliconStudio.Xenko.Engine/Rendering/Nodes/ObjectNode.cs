// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a <see cref="RenderObject"/> and allows to attach properties every frame.
    /// </summary>
    public struct ObjectNode
    {
        /// <summary>
        /// Access underlying RenderObject.
        /// </summary>
        public RenderObject RenderObject;

        public ObjectNode(RenderObject renderObject)
        {
            RenderObject = renderObject;
        }
    }
}
