// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A dynamic effect instance, including its values and resources.
    /// </summary>
    public class EffectInstance : DisposeBase
    {
        // Store current effect
        protected Effect effect;
        protected int permutationCounter;

        // Describes how to update resource bindings
        private ResourceGroupBufferUploader bufferUploader;

        private DescriptorSet[] descriptorSets;

        private EffectParameterUpdater parameterUpdater;

        private EffectDescriptorSetReflection descriptorReflection;

        public EffectInstance(Effect effect, ParameterCollection parameters = null)
        {
            this.effect = effect;
            Parameters = parameters ?? new ParameterCollection();
        }

        public Effect Effect => effect;

        public EffectDescriptorSetReflection DescriptorReflection => descriptorReflection;
        public RootSignature RootSignature { get; private set; }

        public ParameterCollection Parameters { get; }

        /// <summary>
        /// Compiles or recompiles the effect if necesssary.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <returns>True if the effect was recompiled, false otherwise.</returns>
        public bool UpdateEffect(GraphicsDevice graphicsDevice)
        {
            if (permutationCounter != Parameters.PermutationCounter || (effect != null && effect.SourceChanged))
            {
                permutationCounter = Parameters.PermutationCounter;

                var oldEffect = effect;
                ChooseEffect(graphicsDevice);

                // Early exit: same effect, and already initialized
                if (oldEffect == effect && descriptorReflection != null)
                    return false;

                // Update reflection and rearrange buffers/resources
                var layoutNames = effect.Bytecode.Reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();
                descriptorReflection = EffectDescriptorSetReflection.New(graphicsDevice, effect.Bytecode, layoutNames, "Globals");

                RootSignature = RootSignature.New(graphicsDevice, descriptorReflection);

                bufferUploader.Compile(graphicsDevice, descriptorReflection, effect.Bytecode);

                // Create parameter updater
                var layouts = new DescriptorSetLayoutBuilder[descriptorReflection.Layouts.Count];
                for (int i = 0; i < descriptorReflection.Layouts.Count; ++i)
                    layouts[i] = descriptorReflection.Layouts[i].Layout;
                var parameterUpdaterLayout = new EffectParameterUpdaterLayout(graphicsDevice, effect, layouts);
                parameterUpdater = new EffectParameterUpdater(parameterUpdaterLayout, Parameters);

                descriptorSets = new DescriptorSet[parameterUpdater.ResourceGroups.Length];

                return true;
            }

            return false;
        }

        protected virtual void ChooseEffect(GraphicsDevice graphicsDevice)
        {
        }

        public void Apply(GraphicsContext graphicsContext)
        {
            var commandList = graphicsContext.CommandList;

            parameterUpdater.Update(commandList.GraphicsDevice, graphicsContext.ResourceGroupAllocator, Parameters);

            var resourceGroups = parameterUpdater.ResourceGroups;

            // Update cbuffer
            bufferUploader.Apply(commandList, resourceGroups, 0);

            // Bind descriptor sets
            for (int i = 0; i < descriptorSets.Length; ++i)
                descriptorSets[i] = resourceGroups[i].DescriptorSet;

            commandList.SetDescriptorSets(0, descriptorSets);
        }
    }
}
