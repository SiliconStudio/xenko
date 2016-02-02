// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public enum InputClassification
    {
        Vertex,
        Instance,
    }

    public struct InputElementDescription
    {
        public string SemanticName;
        public int SemanticIndex;
        public PixelFormat Format;
        public int InputSlot;
        public int AlignedByteOffset;
        public InputClassification InputSlotClass;
        public int InstanceDataStepRate;
    }

    public class PipelineStateDescription : IEquatable<PipelineStateDescription>
    {
        // TODO: Root Signature?

        // Effect/Shader
        public EffectBytecode EffectBytecode;

        // Rendering States
        public BlendStateDescription BlendState;
        public uint SampleMask;
        public RasterizerStateDescription RasterizerState;
        public DepthStencilStateDescription DepthStencilState;

        // Input layout
        public InputElementDescription[] InputElements;

        public PrimitiveType PrimitiveType;
        public PixelFormat[] RenderTargetFormats;
        public PixelFormat DepthStencilFormat;

        public PipelineStateDescription Clone()
        {
            var inputElements = new InputElementDescription[InputElements.Length];
            for (int i = 0; i < inputElements.Length; ++i)
                inputElements[i] = InputElements[i];

            var renderTargetFormats = new PixelFormat[InputElements.Length];
            for (int i = 0; i < renderTargetFormats.Length; ++i)
                renderTargetFormats[i] = RenderTargetFormats[i];

            return new PipelineStateDescription
            {
                EffectBytecode = EffectBytecode,
                BlendState = BlendState,
                SampleMask = SampleMask,
                RasterizerState = RasterizerState,
                DepthStencilState = DepthStencilState,

                InputElements = inputElements,

                PrimitiveType = PrimitiveType,
                RenderTargetFormats = renderTargetFormats,
                DepthStencilFormat = DepthStencilFormat,
            };
        }

        public void SetDefaults()
        {
            BlendState.SetDefaults();
            RasterizerState.SetDefault();
            DepthStencilState.SetDefault();
        }

        public bool Equals(PipelineStateDescription other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(EffectBytecode, other.EffectBytecode)
                && BlendState.Equals(other.BlendState)
                && SampleMask == other.SampleMask
                && RasterizerState.Equals(other.RasterizerState)
                && DepthStencilState.Equals(other.DepthStencilState)
                && Equals(InputElements, other.InputElements)
                && PrimitiveType == other.PrimitiveType
                && Equals(RenderTargetFormats, other.RenderTargetFormats)
                && DepthStencilFormat == other.DepthStencilFormat;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PipelineStateDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EffectBytecode != null ? EffectBytecode.GetHashCode() : 0;
                hashCode = (hashCode*397) ^ BlendState.GetHashCode();
                hashCode = (hashCode*397) ^ (int)SampleMask;
                hashCode = (hashCode*397) ^ RasterizerState.GetHashCode();
                hashCode = (hashCode*397) ^ DepthStencilState.GetHashCode();
                if (InputElements != null)
                    foreach (var inputElement in InputElements)
                        hashCode = (hashCode*397) ^ inputElement.GetHashCode();
                hashCode = (hashCode*397) ^ (int)PrimitiveType;
                if (RenderTargetFormats != null)
                    foreach (var renderTargetFormat in RenderTargetFormats)
                        hashCode = (hashCode*397) ^ renderTargetFormat.GetHashCode();
                hashCode = (hashCode*397) ^ (int)DepthStencilFormat;
                return hashCode;
            }
        }

        public static bool operator ==(PipelineStateDescription left, PipelineStateDescription right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PipelineStateDescription left, PipelineStateDescription right)
        {
            return !Equals(left, right);
        }
    }

    public partial class PipelineState : GraphicsResourceBase
    {
        public static PipelineState New(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription)
        {
            return new PipelineState(graphicsDevice, pipelineStateDescription);
        }
    }

    public class MutablePipeline
    {
        private Dictionary<PipelineStateDescriptionWithHash, PipelineState> cache = new Dictionary<PipelineStateDescriptionWithHash, PipelineState>();
        public PipelineStateDescription State;

        public MutablePipeline()
        {
            State = new PipelineStateDescription();
            State.SetDefaults();
        }

        public void Apply(GraphicsDevice graphicsDevice)
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
                cache.Add(hashedState, pipelineState = PipelineState.New(graphicsDevice, State));
            }

            graphicsDevice.SetPipelineState(pipelineState);
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