// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>	
    /// Describes a blend state.	
    /// </summary>
    public partial class BlendState : GraphicsResourceBase
    {
        // For FakeBlendState.
        protected BlendState()
        {
        }

        // For FakeBlendState.
        private BlendState(BlendStateDescription description)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendState"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="blendStateDescription">The blend state description.</param>
        public static BlendState New(GraphicsDevice graphicsDevice, BlendStateDescription blendStateDescription)
        {
            BlendState blendState;
            lock (graphicsDevice.CachedBlendStates)
            {
                if (graphicsDevice.CachedBlendStates.TryGetValue(blendStateDescription, out blendState))
                {
                    // TODO: Appropriate destroy
                    blendState.AddReferenceInternal();
                }
                else
                {
                    // Make a local copy of the render targets (ideally, should be ImmutableArray)
                    var renderTargets = blendStateDescription.RenderTargets;
                    blendStateDescription.RenderTargets = new BlendStateRenderTargetDescription[renderTargets.Length];
                    for (int i = 0; i < renderTargets.Length; ++i)
                        blendStateDescription.RenderTargets[i] = renderTargets[i];

                    blendState = new BlendState(graphicsDevice, blendStateDescription);
                    graphicsDevice.CachedBlendStates.Add(blendStateDescription, blendState);
                }
            }
            return blendState;
        }
        
        /// <summary>
        /// Create a new fake blend state for serialization.
        /// </summary>
        /// <param name="description">The description of the blend state</param>
        /// <returns>The fake blend state</returns>
        public static BlendState NewFake(BlendStateDescription description)
        {
            return new BlendState(description);
        }

        protected override void Destroy()
        {
            lock (GraphicsDevice.CachedBlendStates)
            {
                GraphicsDevice.CachedBlendStates.Remove(Description);
            }

            base.Destroy();
        }
        
        /// <summary>
        /// Gets the blend state description.
        /// </summary>
        public readonly BlendStateDescription Description;

        /// <summary>
        /// Gets or sets the four-component (RGBA) blend factor for alpha blending.
        /// </summary>
        public Color4 BlendFactor;

        /// <summary>
        /// Gets or sets a bitmask which defines which samples can be written during multisampling. The default is 0xffffffff.
        /// </summary>
        public int MultiSampleMask = -1;
    }
}
