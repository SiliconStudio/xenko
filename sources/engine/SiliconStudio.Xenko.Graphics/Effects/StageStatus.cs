// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    internal class ShaderStageSetup
    {
        private static readonly EffectParameterResourceBinding.ApplyParameterWithUpdaterDelegate UpdateConstantBufferFromUpdater = UpdateConstantBuffer;
        private static readonly EffectParameterResourceBinding.ApplyParameterWithUpdaterDelegate UpdateSamplerFromUpdater = UpdateSampler;
        private static readonly EffectParameterResourceBinding.ApplyParameterWithUpdaterDelegate UpdateShaderResourceViewFromUpdater = UpdateShaderResourceView;
        private static readonly EffectParameterResourceBinding.ApplyParameterWithUpdaterDelegate UpdateUnorderedAccessViewFromUpdater = UpdateUnorderedAccessView;
        private static readonly EffectParameterResourceBinding.ApplyParameterFromValueDelegate UpdateConstantBufferDirect = UpdateConstantBuffer;
        private static readonly EffectParameterResourceBinding.ApplyParameterFromValueDelegate UpdateSamplerDirect = UpdateSampler;
        private static readonly EffectParameterResourceBinding.ApplyParameterFromValueDelegate UpdateShaderResourceViewDirect = UpdateShaderResourceView;
        private static readonly EffectParameterResourceBinding.ApplyParameterFromValueDelegate UpdateUnorderedAccessViewDirect = UpdateUnorderedAccessView;

        public ShaderStageSetup()
        {
        }

        public void PrepareBindings(EffectParameterResourceBinding[] bindings)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                PrepareBinding(ref bindings[i]);
            }
        }

        public void UpdateParameters(GraphicsDevice graphicsDevice, EffectParameterCollectionGroup parameterCollectionGroup, EffectParameterUpdaterDefinition parameterUpdaterDefinition)
        {
            parameterCollectionGroup.Update(graphicsDevice, parameterUpdaterDefinition);
        }

        public void Apply(GraphicsDevice graphicsDevice, EffectParameterResourceBinding[] bindings, EffectParameterCollectionGroup parameterCollectionGroup, ref EffectStateBindings effectStateBindings, bool applyEffectStates)
        {
            // Apply shader parameters
            for (int i = 0; i < bindings.Length; i++)
            {
                bindings[i].ApplyParameterWithUpdater(graphicsDevice, ref bindings[i].Description, parameterCollectionGroup);
            }

            if (applyEffectStates)
            {
                // Apply graphics states
                var rasterizerState = parameterCollectionGroup.GetValue<RasterizerState>(effectStateBindings.RasterizerStateKeyIndex);
                if (rasterizerState != null)
                {
                    graphicsDevice.SetRasterizerState(rasterizerState);
                }

                var depthStencilState = parameterCollectionGroup.GetValue<DepthStencilState>(effectStateBindings.DepthStencilStateKeyIndex);
                if (depthStencilState != null)
                {
                    graphicsDevice.SetDepthStencilState(depthStencilState);
                }

                var blendState = parameterCollectionGroup.GetValue<BlendState>(effectStateBindings.BlendStateKeyIndex);
                if (blendState != null)
                {
                    graphicsDevice.SetBlendState(blendState);
                }
            }
        }

        public void UnbindResources(GraphicsDevice graphicsDevice, EffectParameterResourceBinding[] bindings)
        {
            // look for shader resource and unbind
            for (int i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                if (binding.Description.Param.Class == EffectParameterClass.ShaderResourceView)
                    graphicsDevice.SetShaderResourceView(binding.Description.Stage, binding.Description.SlotStart, null);
                else if (binding.Description.Param.Class == EffectParameterClass.UnorderedAccessView)
                    graphicsDevice.SetUnorderedAccessView(binding.Description.Stage, binding.Description.SlotStart, null);
            }
        }

        private void PrepareBinding(ref EffectParameterResourceBinding binding)
        {
            switch (binding.Description.Param.Class)
            {
                case EffectParameterClass.ConstantBuffer: // Constant buffer
                    binding.ApplyParameterWithUpdater = UpdateConstantBufferFromUpdater;
                    binding.ApplyParameterDirect = UpdateConstantBufferDirect;
                    break;
                case EffectParameterClass.Sampler: // Sampler state
                    binding.ApplyParameterWithUpdater = UpdateSamplerFromUpdater;
                    binding.ApplyParameterDirect = UpdateSamplerDirect;
                    break;
                case EffectParameterClass.ShaderResourceView: // Texture & StructuredBuffer (using ShaderResourceView)
                    binding.ApplyParameterWithUpdater = UpdateShaderResourceViewFromUpdater;
                    binding.ApplyParameterDirect = UpdateShaderResourceViewDirect;
                    break;
                case EffectParameterClass.UnorderedAccessView: // RWTexture, RWStructuredBuffer only valid for Compute shaders.
                    binding.ApplyParameterWithUpdater = UpdateUnorderedAccessViewFromUpdater;
                    binding.ApplyParameterDirect = UpdateUnorderedAccessViewDirect;
                    break;
            }
        }

        private static void UpdateConstantBuffer(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, EffectParameterCollectionGroup parameterCollectionGroup)
        {
            var constantBufferHelper = parameterCollectionGroup.GetValue<ParameterConstantBuffer>(binding.Param.KeyIndex);
            
            // Update constant buffer content (if required)
            constantBufferHelper.Update(graphicsDevice, parameterCollectionGroup);

            graphicsDevice.SetConstantBuffer(binding.Stage, binding.SlotStart, constantBufferHelper.Buffer);
        }

        private static void UpdateSampler(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, EffectParameterCollectionGroup parameterCollectionGroup)
        {
            var samplerState = (SamplerState)parameterCollectionGroup.GetObject(binding.Param.KeyIndex);
            graphicsDevice.SetSamplerState(binding.Stage, binding.SlotStart, samplerState);
        }

        private static void UpdateShaderResourceView(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, EffectParameterCollectionGroup parameterCollectionGroup)
        {
            var shaderResourceView = (GraphicsResource)parameterCollectionGroup.GetObject(binding.Param.KeyIndex);
            graphicsDevice.SetShaderResourceView(binding.Stage, binding.SlotStart, shaderResourceView);
        }

        private static void UpdateUnorderedAccessView(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, EffectParameterCollectionGroup parameterCollectionGroup)
        {
            var unorderedAccessView = (GraphicsResource)parameterCollectionGroup.GetObject(binding.Param.KeyIndex);
            graphicsDevice.SetUnorderedAccessView(binding.Stage, binding.SlotStart, unorderedAccessView);
        }

        private static void UpdateConstantBuffer(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, object value)
        {
            throw new NotSupportedException("Fast update for constant buffer not supported");
        }

        private static void UpdateSampler(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, object value)
        {
            var samplerState = (SamplerState)value;
            graphicsDevice.SetSamplerState(binding.Stage, binding.SlotStart, samplerState);
        }

        private static void UpdateShaderResourceView(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, object value)
        {
            var shaderResourceView = (GraphicsResource)value;
            graphicsDevice.SetShaderResourceView(binding.Stage, binding.SlotStart, shaderResourceView);
        }

        private static void UpdateUnorderedAccessView(GraphicsDevice graphicsDevice, ref EffectParameterResourceData binding, object value)
        {
            var unorderedAccessView = (GraphicsResource)value;
            graphicsDevice.SetUnorderedAccessView(binding.Stage, binding.SlotStart, unorderedAccessView);
        }
    }
}