// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A component can be integrated into the rendering pipeline automatically if it defines 
    /// a <see cref="EntityComponentRenderableAttribute"/> on its class definition.
    /// </summary>
    public interface IEntityComponentRenderer : IGraphicsRenderer
    {
    }
}