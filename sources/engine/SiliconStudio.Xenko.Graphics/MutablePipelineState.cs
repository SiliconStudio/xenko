using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Graphics
{
    public class MutablePipelineState
    {
        private static Dictionary<PipelineStateDescriptionWithHash, PipelineState> cache = new Dictionary<PipelineStateDescriptionWithHash, PipelineState>();
        public PipelineStateDescription State;

        /// <summary>
        /// Current compiled state.
        /// </summary>
        public PipelineState CurrentState;

        public MutablePipelineState()
        {
            State = new PipelineStateDescription();
            State.SetDefaults();
        }

        /// <summary>
        /// Determine and updates <see cref="CurrentState"/> from <see cref="State"/>.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void Update(GraphicsDevice graphicsDevice)
        {
            // Hash current state
            var hashedState = new PipelineStateDescriptionWithHash(State);

            // Find existing PipelineState object
            PipelineState pipelineState;
            if (!cache.TryGetValue(hashedState, out pipelineState))
            {
                // Otherwise, instantiate it
                // First, make an copy
                hashedState = new PipelineStateDescriptionWithHash(State.Clone());
                cache.Add(hashedState, pipelineState = PipelineState.New(graphicsDevice, ref State));
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
    }
}