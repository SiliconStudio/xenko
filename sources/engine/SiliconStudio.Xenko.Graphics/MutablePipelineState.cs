using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    public class MutablePipelineState
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly Dictionary<PipelineStateDescriptionWithHash, PipelineState> cache;
        public PipelineStateDescription State;

        /// <summary>
        /// Current compiled state.
        /// </summary>
        public PipelineState CurrentState;

        public MutablePipelineState(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            cache = graphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, typeof(MutablePipelineStateCache), device => new MutablePipelineStateCache()).Cache;

            State = new PipelineStateDescription();
            State.SetDefaults();
        }

        /// <summary>
        /// Determine and updates <see cref="CurrentState"/> from <see cref="State"/>.
        /// </summary>
        public void Update()
        {
            // Hash current state
            var hashedState = new PipelineStateDescriptionWithHash(State);

            // Find existing PipelineState object
            PipelineState pipelineState;

            // TODO GRAPHICS REFACTOR We could avoid lock by adding them to a ThreadLocal (or RenderContext) and merge at end of frame
            lock (cache)
            {
                if (!cache.TryGetValue(hashedState, out pipelineState))
                {
                    // Otherwise, instantiate it
                    // First, make an copy
                    hashedState = new PipelineStateDescriptionWithHash(State.Clone());
                    cache.Add(hashedState, pipelineState = PipelineState.New(graphicsDevice, ref State));
                }
            }

            CurrentState = pipelineState;
        }

        struct PipelineStateDescriptionWithHash : IEquatable<PipelineStateDescriptionWithHash>
        {
            public readonly int Hash;
            public readonly PipelineStateDescription State;

            public PipelineStateDescriptionWithHash(PipelineStateDescription state)
            {
                Hash = state.GetHashCode();
                State = state;
            }

            public bool Equals(PipelineStateDescriptionWithHash other)
            {
                return Hash == other.Hash && State.Equals(other.State);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is PipelineStateDescriptionWithHash && Equals((PipelineStateDescriptionWithHash)obj);
            }

            public override int GetHashCode()
            {
                return Hash;
            }

            public static bool operator ==(PipelineStateDescriptionWithHash left, PipelineStateDescriptionWithHash right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(PipelineStateDescriptionWithHash left, PipelineStateDescriptionWithHash right)
            {
                return !left.Equals(right);
            }
        }

        class MutablePipelineStateCache : IDisposable
        {
            public readonly Dictionary<PipelineStateDescriptionWithHash, PipelineState> Cache = new Dictionary<PipelineStateDescriptionWithHash, PipelineState>();

            public void Dispose()
            {
                foreach (var pipelineState in Cache)
                {
                    ((IReferencable)pipelineState.Value).Release();
                }

                Cache.Clear();
            }
        }
    }
}