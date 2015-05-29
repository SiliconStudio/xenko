// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class RasterizerState : GraphicsResourceBase
    {
        // For FakeRasterizerState.
        protected RasterizerState()
        {
        }

        // For FakeRasterizerState.
        private RasterizerState(RasterizerStateDescription description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets the rasterizer state description.
        /// </summary>
        public readonly RasterizerStateDescription Description;

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerState"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="rasterizerStateDescription">The rasterizer state description.</param>
        public static RasterizerState New(GraphicsDevice graphicsDevice, RasterizerStateDescription rasterizerStateDescription)
        {
            RasterizerState rasterizerState;
            lock (graphicsDevice.CachedRasterizerStates)
            {
                if (graphicsDevice.CachedRasterizerStates.TryGetValue(rasterizerStateDescription, out rasterizerState))
                {
                    // TODO: Appropriate destroy
                    rasterizerState.AddReferenceInternal();
                }
                else
                {
                    rasterizerState = new RasterizerState(graphicsDevice, rasterizerStateDescription);
                    graphicsDevice.CachedRasterizerStates.Add(rasterizerStateDescription, rasterizerState);
                }
            }
            return rasterizerState;
        }

        /// <summary>
        /// Create a new fake rasterizer state for serialization.
        /// </summary>
        /// <param name="description">The description of the rasterizer state</param>
        /// <returns>The fake rasterizer state</returns>
        public static RasterizerState NewFake(RasterizerStateDescription description)
        {
            return new RasterizerState(description);
        }

        protected override void Destroy()
        {
            lock (GraphicsDevice.CachedRasterizerStates)
            {
                GraphicsDevice.CachedRasterizerStates.Remove(Description);
            }

            base.Destroy();
        }
    }
}