// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public class PipelineStateDescription : IEquatable<PipelineStateDescription>
    {
        // Root Signature
        public RootSignature RootSignature;

        // Effect/Shader
        public EffectBytecode EffectBytecode;

        // Rendering States
        public BlendStateDescription BlendState;
        public uint SampleMask = 0xFFFFFFFF;
        public RasterizerStateDescription RasterizerState;
        public DepthStencilStateDescription DepthStencilState;

        // Input layout
        public InputElementDescription[] InputElements;

        public PrimitiveType PrimitiveType;
        public PixelFormat[] RenderTargetFormats;
        public PixelFormat DepthStencilFormat;

        public PipelineStateDescription Clone()
        {
            InputElementDescription[] inputElements;
            if (InputElements != null)
            {
                inputElements = new InputElementDescription[InputElements.Length];
                for (int i = 0; i < inputElements.Length; ++i)
                    inputElements[i] = InputElements[i];
            }
            else
            {
                inputElements = null;
            }

            PixelFormat[] renderTargetFormats;
            if (RenderTargetFormats != null)
            {
                renderTargetFormats = new PixelFormat[RenderTargetFormats.Length];
                for (int i = 0; i < renderTargetFormats.Length; ++i)
                    renderTargetFormats[i] = RenderTargetFormats[i];
            }
            else
            {
                renderTargetFormats = null;
            }

            return new PipelineStateDescription
            {
                RootSignature = RootSignature,
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
            if (!(RootSignature == other.RootSignature
                && EffectBytecode == other.EffectBytecode
                && BlendState.Equals(other.BlendState)
                && SampleMask == other.SampleMask
                && RasterizerState.Equals(other.RasterizerState)
                && DepthStencilState.Equals(other.DepthStencilState)
                && PrimitiveType == other.PrimitiveType
                && DepthStencilFormat == other.DepthStencilFormat))
                return false;

            if ((InputElements != null) != (other.InputElements != null))
                return false;
            if (InputElements != null)
            {
                for (int i = 0; i < InputElements.Length; ++i)
                {
                    if (!InputElements[i].Equals(other.InputElements[i]))
                        return false;
                }
            }

            if ((RenderTargetFormats != null) != (other.RenderTargetFormats != null))
                return false;
            if (RenderTargetFormats != null)
            {
                for (int i = 0; i < RenderTargetFormats.Length; ++i)
                {
                    if (!RenderTargetFormats[i].Equals(other.RenderTargetFormats[i]))
                        return false;
                }
            }

            return true;
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
                var hashCode = RootSignature != null ? RootSignature.GetHashCode() : 0;
                hashCode = (hashCode*397) ^ (EffectBytecode != null ? EffectBytecode.GetHashCode() : 0);
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
}