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
        }

        /// <summary>
        /// Built-in raterizer state object with settings for culling primitives with clockwise winding order.
        /// </summary>
        public readonly RasterizerState CullFront;

        /// <summary>
        /// Built-in raterizer state object with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public readonly RasterizerState CullBack;

        /// <summary>
        /// Built-in raterizer state object with settings for not culling any primitives.
        /// </summary>
        public readonly RasterizerState CullNone;
    }
}

