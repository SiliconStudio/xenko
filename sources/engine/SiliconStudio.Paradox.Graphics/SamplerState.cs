// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    [ContentSerializer(typeof(SamplerStateSerializer))]
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
        protected SamplerState(SamplerStateDescription description)
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

        protected override void Destroy()
        {
            lock (GraphicsDevice.CachedSamplerStates)
            {
                GraphicsDevice.CachedSamplerStates.Remove(Description);
            }

            base.Destroy();
        }
    }
}