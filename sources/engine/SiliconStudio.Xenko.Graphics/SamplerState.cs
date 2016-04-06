// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class SamplerState : GraphicsResourceBase
    {
        /// <summary>
        /// Gets the sampler state description.
        /// </summary>
        public readonly SamplerStateDescription Description;

        // For FakeSamplerState.
        protected SamplerState()
        {
        }

        // For FakeSamplerState.
        private SamplerState(SamplerStateDescription description)
        {
            Description = description;
        }

        public static SamplerState New(GraphicsDevice graphicsDevice, SamplerStateDescription samplerStateDescription)
        {
            // Store SamplerState in a cache (D3D seems to have quite bad concurrency when using CreateSampler while rendering)
            SamplerState samplerState;
            lock (graphicsDevice.CachedSamplerStates)
            {
                if (graphicsDevice.CachedSamplerStates.TryGetValue(samplerStateDescription, out samplerState))
                {
                    // TODO: Appropriate destroy
                    samplerState.AddReferenceInternal();
                }
                else
                {
                    samplerState = new SamplerState(graphicsDevice, samplerStateDescription);
                    graphicsDevice.CachedSamplerStates.Add(samplerStateDescription, samplerState);
                }
            }
            return samplerState;
        }
        
        /// <summary>
        /// Create a new fake sampler state for serialization.
        /// </summary>
        /// <param name="description">The description of the sampler state</param>
        /// <returns>The fake sampler state</returns>
        public static SamplerState NewFake(SamplerStateDescription description)
        {
            return new SamplerState(description);
        }

        protected unsafe override void Destroy()
        {
            lock (GraphicsDevice.CachedSamplerStates)
            {
                GraphicsDevice.CachedSamplerStates.Remove(Description);
            }

            GraphicsDevice.NativeDevice.DestroySampler(NativeSampler);

            base.Destroy();
        }
    }
}