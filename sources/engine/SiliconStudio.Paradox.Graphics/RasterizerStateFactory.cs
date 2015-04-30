// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Base factory for <see cref="RasterizerState"/>.
    /// </summary>
    public class RasterizerStateFactory : GraphicsResourceFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerStateFactory"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal RasterizerStateFactory(GraphicsDevice device) : base(device)
        {
            CullFront = RasterizerState.New(device, new RasterizerStateDescription(CullMode.Front)).KeepAliveBy(this);
            CullFront.Name = "RasterizerState.CullClockwise";

            CullBack = RasterizerState.New(device, new RasterizerStateDescription(CullMode.Back)).KeepAliveBy(this);
            CullBack.Name = "RasterizerState.CullCounterClockwiseFace";

            CullNone = RasterizerState.New(device, new RasterizerStateDescription(CullMode.None)).KeepAliveBy(this);
            CullNone.Name = "RasterizerState.CullNone";

            WireFrameCullFront = RasterizerState.New(device, new RasterizerStateDescription(CullMode.Front) { FillMode = FillMode.Wireframe }).KeepAliveBy(this);
            WireFrameCullFront.Name = "RasterizerState.WireFrameCullFront";

            WireFrameCullBack = RasterizerState.New(device, new RasterizerStateDescription(CullMode.Back) { FillMode = FillMode.Wireframe }).KeepAliveBy(this);
            WireFrameCullBack.Name = "RasterizerState.WireFrameCullBack";

            WireFrame = RasterizerState.New(device, new RasterizerStateDescription(CullMode.None) { FillMode = FillMode.Wireframe }).KeepAliveBy(this);
            WireFrame.Name = "RasterizerState.WireFrame";

        }

        /// <summary>
        /// Built-in rasterizer state object with settings for culling primitives with clockwise winding order.
        /// </summary>
        public readonly RasterizerState CullFront;

        /// <summary>
        /// Built-in rasterizer state object with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public readonly RasterizerState CullBack;

        /// <summary>
        /// Built-in rasterizer state object with settings for not culling any primitives.
        /// </summary>
        public readonly RasterizerState CullNone;

        /// <summary>
        /// Built-in rasterizer state object for wireframe rendering with settings for culling primitives with clockwise winding order.
        /// </summary>
        public readonly RasterizerState WireFrameCullFront;

        /// <summary>
        /// Built-in rasterizer state object for wireframe with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public readonly RasterizerState WireFrameCullBack;

        /// <summary>
        /// Built-in rasterizer state object for wireframe with settings for not culling any primitives.
        /// </summary>
        public readonly RasterizerState WireFrame;
    }
}

