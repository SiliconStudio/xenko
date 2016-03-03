// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
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
            CullFront = new RasterizerStateDescription(CullMode.Front);
            CullBack = new RasterizerStateDescription(CullMode.Back);
            CullNone = new RasterizerStateDescription(CullMode.None);
            WireframeCullFront = new RasterizerStateDescription(CullMode.Front) { FillMode = FillMode.Wireframe };
            WireframeCullBack = new RasterizerStateDescription(CullMode.Back) { FillMode = FillMode.Wireframe };
            Wireframe = new RasterizerStateDescription(CullMode.None) { FillMode = FillMode.Wireframe };
        }

        /// <summary>
        /// Built-in rasterizer state object with settings for culling primitives with clockwise winding order.
        /// </summary>
        public readonly RasterizerStateDescription CullFront;

        /// <summary>
        /// Built-in rasterizer state object with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public readonly RasterizerStateDescription CullBack;

        /// <summary>
        /// Built-in rasterizer state object with settings for not culling any primitives.
        /// </summary>
        public readonly RasterizerStateDescription CullNone;

        /// <summary>
        /// Built-in rasterizer state object for wireframe rendering with settings for culling primitives with clockwise winding order.
        /// </summary>
        public readonly RasterizerStateDescription WireframeCullFront;

        /// <summary>
        /// Built-in rasterizer state object for wireframe with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public readonly RasterizerStateDescription WireframeCullBack;

        /// <summary>
        /// Built-in rasterizer state object for wireframe with settings for not culling any primitives.
        /// </summary>
        public readonly RasterizerStateDescription Wireframe;
    }
}

