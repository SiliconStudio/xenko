using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    // TODO: Should we keep that separate or merge it into RootRenderFeature?
    /// <summary>
    /// A root render feature that can manipulate effects.
    /// </summary>
    public abstract class RootEffectRenderFeature : RootRenderFeature
    {
        // Helper class to build pipeline state
        protected MutablePipelineState MutablePipeline = new MutablePipelineState();

        private readonly List<string> effectDescriptorSetSlots = new List<string>();
        private readonly Dictionary<string, int> effectPermutationSlots = new Dictionary<string, int>();
        private readonly Dictionary<ObjectId, FrameResourceGroupLayout> frameResourceLayouts = new Dictionary<ObjectId, FrameResourceGroupLayout>();
        private readonly Dictionary<ObjectId, ViewResourceGroupLayout> viewResourceLayouts = new Dictionary<ObjectId, ViewResourceGroupLayout>();

        private readonly Dictionary<ObjectId, DescriptorSetLayout> createdDescriptorSetLayouts = new Dictionary<ObjectId, DescriptorSetLayout>();

        private readonly List<ConstantBufferOffsetDefinition> frameCBufferOffsetSlots = new List<ConstantBufferOffsetDefinition>();
        private readonly List<ConstantBufferOffsetDefinition> viewCBufferOffsetSlots = new List<ConstantBufferOffsetDefinition>();
        private readonly List<ConstantBufferOffsetDefinition> drawCBufferOffsetSlots = new List<ConstantBufferOffsetDefinition>();

        // Common slots
        private EffectDescriptorSetReference perFrameDescriptorSetSlot;
        private EffectDescriptorSetReference perViewDescriptorSetSlot;
        private EffectDescriptorSetReference perDrawDescriptorSetSlot;

        private EffectPermutationSlot[] effectSlots = null;

        public List<EffectObjectNode> EffectObjectNodes { get; } = new List<EffectObjectNode>();

        public delegate Effect ComputeFallbackEffectDelegate(RenderObject renderObject, RenderEffect renderEffect, RenderEffectState renderEffectState);

        public ComputeFallbackEffectDelegate ComputeFallbackEffect { get; set; }

        public ResourceGroup[] ResourceGroupPool = new ResourceGroup[256];

        public List<FrameResourceGroupLayout> FrameLayouts { get; } = new List<FrameResourceGroupLayout>();
        public Action<NextGenRenderSystem, Effect, RenderEffectReflection> EffectCompiled;

        public delegate void ProcessPipelineStateDelegate(RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState);
        public ProcessPipelineStateDelegate PostProcessPipelineState;

        public int EffectDescriptorSetSlotCount => effectDescriptorSetSlots.Count;

        /// <summary>
        /// Gets number of effect permutation slot, which is the number of effect cached per object.
        /// </summary>
        public int EffectPermutationSlotCount => effectPermutationSlots.Count;

        /// <summary>
        /// Key to store extra info for each effect instantiation of each object.
        /// </summary>
        public StaticObjectPropertyKey<RenderEffect> RenderEffectKey;

        // TODO: Proper interface to register effects
        /// <summary>
        /// Stores reflection info for each effect.
        /// </summary>
        public Dictionary<Effect, RenderEffectReflection> InstantiatedEffects = new Dictionary<Effect, RenderEffectReflection>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EffectObjectNode GetEffectObjectNode(EffectObjectNodeReference reference)
        {
            return EffectObjectNodes[reference.Index];
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Create RenderEffectKey
            RenderEffectKey = RenderData.CreateStaticObjectKey<RenderEffect>(null, EffectPermutationSlotCount);

            // TODO: Assign weights so that PerDraw is always last? (we usually most custom user ones to be between PerView and PerDraw)
            perFrameDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerFrame");
            perViewDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerView");
            perDrawDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerDraw");

            RenderSystem.RenderStages.CollectionChanged += RenderStages_CollectionChanged;

            // Create effect slots
            Array.Resize(ref effectSlots, RenderSystem.RenderStages.Count);
            for (int index = 0; index < RenderSystem.RenderStages.Count; index++)
            {
                var renderStage = RenderSystem.RenderStages[index];
                effectSlots[index] = CreateEffectPermutationSlot(renderStage.EffectSlotName);
            }
        }

        /// <summary>
        /// Compute the index of first descriptor set stored in <see cref="ResourceGroupPool"/>.
        /// </summary>
        protected internal int ComputeResourceGroupOffset(RenderNodeReference renderNode)
        {
            return renderNode.Index * effectDescriptorSetSlots.Count;
        }

        public EffectDescriptorSetReference GetOrCreateEffectDescriptorSetSlot(string name)
        {
            // Check if it already exists
            var existingIndex = effectDescriptorSetSlots.IndexOf(name);
            if (existingIndex != -1)
                return new EffectDescriptorSetReference(existingIndex);

            // Otherwise creates it
            effectDescriptorSetSlots.Add(name);
            return new EffectDescriptorSetReference(effectDescriptorSetSlots.Count - 1);
        }

        public ConstantBufferOffsetReference CreateFrameCBufferOffsetSlot(string variable)
        {
            // TODO: Handle duplicates, and allow removal
            var slotReference = new ConstantBufferOffsetReference(frameCBufferOffsetSlots.Count);
            frameCBufferOffsetSlots.Add(new ConstantBufferOffsetDefinition(variable));

            // Update existing instantiated buffers
            foreach (var frameResourceLayoutEntry in frameResourceLayouts)
            {
                var resourceGroupLayout = frameResourceLayoutEntry.Value;

                // Ensure there is enough space
                if (resourceGroupLayout.ConstantBufferOffsets == null || resourceGroupLayout.ConstantBufferOffsets.Length < frameCBufferOffsetSlots.Count)
                    Array.Resize(ref resourceGroupLayout.ConstantBufferOffsets, frameCBufferOffsetSlots.Count);

                ResolveCBufferOffset(resourceGroupLayout, slotReference.Index, variable);
            }

            return slotReference;
        }

        public ConstantBufferOffsetReference CreateViewCBufferOffsetSlot(string variable)
        {
            // TODO: Handle duplicates, and allow removal
            var slotReference = new ConstantBufferOffsetReference(viewCBufferOffsetSlots.Count);
            viewCBufferOffsetSlots.Add(new ConstantBufferOffsetDefinition(variable));

            // Update existing instantiated buffers
            foreach (var viewResourceLayoutEntry in viewResourceLayouts)
            {
                var resourceGroupLayout = viewResourceLayoutEntry.Value;

                // Ensure there is enough space
                if (resourceGroupLayout.ConstantBufferOffsets == null || resourceGroupLayout.ConstantBufferOffsets.Length < viewCBufferOffsetSlots.Count)
                    Array.Resize(ref resourceGroupLayout.ConstantBufferOffsets, viewCBufferOffsetSlots.Count);

                ResolveCBufferOffset(resourceGroupLayout, slotReference.Index, variable);
            }

            return slotReference;
        }

        public ConstantBufferOffsetReference CreateDrawCBufferOffsetSlot(string variable)
        {
            // TODO: Handle duplicates, and allow removal
            var slotReference = new ConstantBufferOffsetReference(drawCBufferOffsetSlots.Count);
            drawCBufferOffsetSlots.Add(new ConstantBufferOffsetDefinition(variable));

            // Update existing instantiated buffers
            foreach (var effect in InstantiatedEffects)
            {
                var resourceGroupLayout = effect.Value.PerDrawLayout;

                // Ensure there is enough space
                if (resourceGroupLayout.ConstantBufferOffsets == null || resourceGroupLayout.ConstantBufferOffsets.Length < drawCBufferOffsetSlots.Count)
                    Array.Resize(ref resourceGroupLayout.ConstantBufferOffsets, drawCBufferOffsetSlots.Count);

                ResolveCBufferOffset(resourceGroupLayout, slotReference.Index, variable);
            }

            return slotReference;
        }

        private void ResolveCBufferOffset(RenderSystemResourceGroupLayout resourceGroupLayout, int index, string variable)
        {
            // Update slot
            if (resourceGroupLayout.ConstantBufferReflection != null)
            {
                foreach (var member in resourceGroupLayout.ConstantBufferReflection.Members)
                {
                    if (member.Param.KeyName == variable)
                    {
                        resourceGroupLayout.ConstantBufferOffsets[index] = member.Offset;
                        return;
                    }
                }
            }

            // Not found?
            resourceGroupLayout.ConstantBufferOffsets[index] = -1;
        }

        /// <summary>
        /// Creates a slot for storing a particular effect instantiation (per RenderObject).
        /// </summary>
        /// As an example, we could have main shader (automatically created), GBuffer shader and shadow mapping shader.
        /// <returns></returns>
        public EffectPermutationSlot CreateEffectPermutationSlot(string effectName)
        {
            // Allocate effect slot
            // TODO: Should we allow/support this to be called after Initialize()?
            int slot;
            if (!effectPermutationSlots.TryGetValue(effectName, out slot))
            {
                if (effectPermutationSlots.Count >= 32)
                {
                    throw new InvalidOperationException("Only 32 effect slots are currently allowed for meshes");
                }

                slot = effectPermutationSlots.Count;
                effectPermutationSlots.Add(effectName, slot);

                // Add render effect slot
                RenderData.ChangeDataMultiplier(RenderEffectKey, EffectPermutationSlotCount);
            }

            return new EffectPermutationSlot(slot);
        }

        /// <summary>
        /// Actual implementation of <see cref="PrepareEffectPermutations"/>.
        /// </summary>
        public virtual void PrepareEffectPermutationsImpl()
        {
            
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutations()
        {
            base.PrepareEffectPermutations();

            // TODO: Temporary until we have a better system for handling permutations
            var renderEffects = RenderData.GetData(RenderEffectKey);
            int effectSlotCount = EffectPermutationSlotCount;

            foreach (var view in RenderSystem.Views)
            {
                var viewFeature = view.Features[Index];
                foreach (var renderNodeReference in viewFeature.RenderNodes)
                {
                    var renderNode = this.GetRenderNode(renderNodeReference);
                    var renderObject = renderNode.RenderObject;

                    // Get RenderEffect
                    var staticObjectNode = renderObject.StaticObjectNode;
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + effectSlots[renderNode.RenderStage.Index].Index;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Create it (first time)
                    if (renderEffect == null)
                    {
                        renderEffect = new RenderEffect(renderObject.ActiveRenderStages[renderNode.RenderStage.Index].EffectName);
                        renderEffects[staticEffectObjectNode] = renderEffect;
                    }

                    // Is it the first time this frame that we check this RenderEffect?
                    if (renderEffect.MarkAsUsed(RenderSystem))
                    {
                        renderEffect.EffectValidator.BeginEffectValidation();
                    }
                }
            }

            // Step1: Perform permutations
            PrepareEffectPermutationsImpl();

            var compilerParameters = new CompilerParameters();

            // Step2: Compile effects and update reflection infos (offset, etc...)
            foreach (var renderObject in RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip if not used
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;
                   
                    if (renderEffect.EffectValidator.EndEffectValidation())
                    {
                        InvalidateEffectPermutation(renderObject, renderEffect);
                    
                        // Still, let's check if there is a pending effect compiling
                        var pendingEffect = renderEffect.PendingEffect;
                        if (pendingEffect == null || !pendingEffect.IsCompleted)
                            continue;

                        renderEffect.ClearFallbackParameters();
                        if (pendingEffect.IsFaulted)
                        {
                            renderEffect.State = RenderEffectState.Error;
                            renderEffect.FallbackParameters = new NextGenParameterCollection();
                            effect = ComputeFallbackEffect?.Invoke(renderObject, renderEffect, RenderEffectState.Error);
                        }
                        else
                        {
                            renderEffect.State = RenderEffectState.Normal;
                            effect = pendingEffect.Result;
                        }
                        renderEffect.PendingEffect = null;
                    }
                    else
                    {
                        // Reset pending effect, as it is now obsolete anyway
                        renderEffect.PendingEffect = null;

                        foreach (var effectValue in renderEffect.EffectValidator.EffectValues)
                        {
                            compilerParameters.SetObject(effectValue.Key, effectValue.Value);
                        }

                        var asyncEffect = RenderSystem.EffectSystem.LoadEffect(renderEffect.EffectName, compilerParameters);
                        compilerParameters.Clear();

                        effect = asyncEffect.Result;
                        if (effect == null)
                        {
                            // Effect still compiling, let's find if there is a fallback
                            renderEffect.ClearFallbackParameters();
                            renderEffect.FallbackParameters = new NextGenParameterCollection();
                            effect = ComputeFallbackEffect?.Invoke(renderObject, renderEffect, RenderEffectState.Compiling);
                            if (effect != null)
                            {
                                // Use the fallback for now
                                renderEffect.PendingEffect = asyncEffect.Task;
                                renderEffect.State = RenderEffectState.Compiling;
                            }
                            else
                            {
                                // No fallback effect, let's block until effect is compiled
                                effect = asyncEffect.WaitForResult();
                            }
                        }
                    }

                    if (effect != null)
                    {
                        RenderEffectReflection renderEffectReflection;
                        if (!InstantiatedEffects.TryGetValue(effect, out renderEffectReflection))
                        {
                            renderEffectReflection = new RenderEffectReflection();

                            // Build root signature automatically from reflection
                            renderEffectReflection.DescriptorReflection = EffectDescriptorSetReflection.New(RenderSystem.GraphicsDevice, effect.Bytecode, effectDescriptorSetSlots, "PerFrame");
                            renderEffectReflection.RootSignature = RootSignature.New(RenderSystem.GraphicsDevice, renderEffectReflection.DescriptorReflection);
                            renderEffectReflection.BufferUploader.Compile(RenderSystem.GraphicsDevice, renderEffectReflection.DescriptorReflection, effect.Bytecode);

                            // Prepare well-known descriptor set layouts
                            renderEffectReflection.PerDrawLayout = CreateDrawResourceGroupLayout(renderEffectReflection.DescriptorReflection.GetLayout("PerDraw"), effect.Bytecode);
                            renderEffectReflection.PerFrameLayout = CreateFrameResourceGroupLayout(renderEffectReflection.DescriptorReflection.GetLayout("PerFrame"), effect.Bytecode);
                            renderEffectReflection.PerViewLayout = CreateViewResourceGroupLayout(renderEffectReflection.DescriptorReflection.GetLayout("PerView"), effect.Bytecode);

                            InstantiatedEffects.Add(effect, renderEffectReflection);

                            // Notify a new effect has been compiled
                            EffectCompiled?.Invoke(RenderSystem, effect, renderEffectReflection);
                        }

                        // Setup fallback parameters
                        if (renderEffect.State != RenderEffectState.Normal && renderEffectReflection.FallbackUpdaterLayout == null)
                        {
                            // Process all "non standard" layouts
                            var layoutMapping = new int[renderEffectReflection.DescriptorReflection.Layouts.Count - 3];
                            var layouts = new DescriptorSetLayoutBuilder[renderEffectReflection.DescriptorReflection.Layouts.Count - 3];
                            int layoutMappingIndex = 0;
                            for (int index = 0; index < renderEffectReflection.DescriptorReflection.Layouts.Count; index++)
                            {
                                var layout = renderEffectReflection.DescriptorReflection.Layouts[index];

                                // Skip well-known layouts (already handled)
                                if (layout.Name == "PerDraw" || layout.Name == "PerFrame" || layout.Name == "PerView")
                                    continue;

                                layouts[layoutMappingIndex] = layout.Layout;
                                layoutMapping[layoutMappingIndex++] = index;
                            }

                            renderEffectReflection.FallbackUpdaterLayout = new EffectParameterUpdaterLayout(RenderSystem.GraphicsDevice, effect, layouts);
                            renderEffectReflection.FallbackResourceGroupMapping = layoutMapping;
                        }

                        // Update effect
                        renderEffect.Effect = effect;
                        renderEffect.Reflection = renderEffectReflection;

                        // Invalidate pipeline state (new effect)
                        renderEffect.PipelineState = null;

                        renderEffects[staticEffectObjectNode] = renderEffect;
                    }
                }
            }
        }

        /// <summary>
        /// Implemented by subclasses to reset effect dependent data.
        /// </summary>
        protected virtual void InvalidateEffectPermutation(RenderObject renderObject, RenderEffect renderEffect)
        {
        }

        /// <inheritdoc/>
        public override void Prepare(RenderContext context)
        {
            EffectObjectNodes.Clear();

            int requiredDescriptorSets = 0;

            // Make sure descriptor set pool is large enough
            var expectedDescriptorSetPoolSize = RenderNodes.Count * effectDescriptorSetSlots.Count;
            if (ResourceGroupPool.Length < expectedDescriptorSetPoolSize)
                Array.Resize(ref ResourceGroupPool, expectedDescriptorSetPoolSize);

            // Allocate PerFrame, PerView and PerDraw resource groups and constant buffers
            var renderEffects = RenderData.GetData(RenderEffectKey);
            int effectSlotCount = EffectPermutationSlotCount;
            foreach (var view in RenderSystem.Views)
            {
                var viewFeature = view.Features[Index];
                foreach (var renderNodeReference in viewFeature.RenderNodes)
                {
                    var renderNode = this.GetRenderNode(renderNodeReference);
                    var renderObject = renderNode.RenderObject;

                    // Get RenderEffect
                    var staticObjectNode = renderObject.StaticObjectNode;
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + effectSlots[renderNode.RenderStage.Index].Index;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Not compiled yet?
                    if (renderEffect.Effect == null)
                        continue;

                    var renderEffectReflection = renderEffect.Reflection;

                    // PerView resources/cbuffer
                    var viewLayout = renderEffectReflection.PerViewLayout;

                    if (viewLayout.Entries?.Length <= view.Index)
                    {
                        var oldEntries = viewLayout.Entries;
                        viewLayout.Entries = new ResourceGroupEntry[RenderSystem.Views.Count];

                        for (int index = 0; index < oldEntries.Length; index++)
                            viewLayout.Entries[index] = oldEntries[index];

                        for (int index = oldEntries.Length; index < viewLayout.Entries.Length; index++)
                            viewLayout.Entries[index].Resources = new ResourceGroup();
                    }

                    if (viewLayout.Entries[view.Index].MarkAsUsed(RenderSystem))
                    {
                        NextGenParameterCollectionLayoutExtensions.PrepareResourceGroup(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, RenderSystem.BufferPool, viewLayout, BufferPoolAllocationType.UsedMultipleTime, viewLayout.Entries[view.Index].Resources);

                        // Register it in list of view layouts to update for this frame
                        viewFeature.Layouts.Add(viewLayout);
                    }

                    // PerFrame resources/cbuffer
                    var frameLayout = renderEffect.Reflection.PerFrameLayout;
                    if (frameLayout != null && frameLayout.Entry.MarkAsUsed(RenderSystem))
                    {
                        NextGenParameterCollectionLayoutExtensions.PrepareResourceGroup(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, RenderSystem.BufferPool, viewLayout, BufferPoolAllocationType.UsedMultipleTime, frameLayout.Entry.Resources);

                        // Register it in list of view layouts to update for this frame
                        FrameLayouts.Add(frameLayout);
                    }

                    // PerDraw resources/cbuffer
                    // Get nodes
                    var viewObjectNode = GetViewObjectNode(renderNode.ViewObjectNode);

                    // Allocate descriptor set
                    renderNode.Resources = AllocateTemporaryResourceGroup();
                    NextGenParameterCollectionLayoutExtensions.PrepareResourceGroup(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, RenderSystem.BufferPool, renderEffectReflection.PerDrawLayout, BufferPoolAllocationType.UsedOnce, renderNode.Resources);

                    // Link to EffectObjectNode (created right after)
                    // TODO: rewrite this
                    renderNode.EffectObjectNode = new EffectObjectNodeReference(EffectObjectNodes.Count);

                    renderNode.RenderEffect = renderEffect;

                    // Bind well-known descriptor sets
                    var descriptorSetPoolOffset = ComputeResourceGroupOffset(renderNodeReference);
                    ResourceGroupPool[descriptorSetPoolOffset + perFrameDescriptorSetSlot.Index] = frameLayout?.Entry.Resources;
                    ResourceGroupPool[descriptorSetPoolOffset + perViewDescriptorSetSlot.Index] = renderEffect.Reflection.PerViewLayout.Entries[view.Index].Resources;
                    ResourceGroupPool[descriptorSetPoolOffset + perDrawDescriptorSetSlot.Index] = renderNode.Resources;

                    // Create resource group for everything else in case of fallback effects
                    if (renderEffect.State != RenderEffectState.Normal)
                    {
                        if (renderEffect.FallbackParameterUpdater.ResourceGroups == null)
                        {
                            // First time
                            renderEffect.FallbackParameterUpdater = new EffectParameterUpdater(renderEffect.Reflection.FallbackUpdaterLayout, renderEffect.FallbackParameters);
                        }

                        renderEffect.FallbackParameterUpdater.Update(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, RenderSystem.BufferPool, renderEffect.FallbackParameters);

                        var fallbackResourceGroupMapping = renderEffect.Reflection.FallbackResourceGroupMapping;
                        for (int i = 0; i < fallbackResourceGroupMapping.Length; ++i)
                        {
                            ResourceGroupPool[descriptorSetPoolOffset + fallbackResourceGroupMapping[i]] = renderEffect.FallbackParameterUpdater.ResourceGroups[i];
                        }
                    }

                    // Compile pipeline state object (if first time or need change)
                    // TODO GRAPHICS REFACTOR how to invalidate if we want to change some state? (setting to null should be fine)
                    if (renderEffect.PipelineState == null)
                    {
                        var pipelineState = MutablePipeline.State;
                        pipelineState.SetDefaults();

                        // Effect
                        pipelineState.EffectBytecode = renderEffect.Effect.Bytecode;
                        pipelineState.RootSignature = renderEffect.Reflection.RootSignature;

                        // Bind VAO
                        ProcessPipelineState(context, renderNodeReference, ref renderNode, renderObject, pipelineState);

                        // Extract outputs from render stage
                        pipelineState.Output = renderNode.RenderStage.Output;

                        PostProcessPipelineState?.Invoke(renderNodeReference, ref renderNode, renderObject, pipelineState);

                        MutablePipeline.Update(context.GraphicsDevice);
                        renderEffect.PipelineState = MutablePipeline.CurrentState;
                    }

                    RenderNodes[renderNodeReference.Index] = renderNode;

                    // Create EffectObjectNode
                    EffectObjectNodes.Add(new EffectObjectNode(renderEffect, viewObjectNode.ObjectNode));
                }
            }
        }

        protected virtual void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
        }

        private ResourceGroup AllocateTemporaryResourceGroup()
        {
            // TODO: Provide a better implementation that avoids allocation (i.e. using a pool)
            return new ResourceGroup();
        }

        public override void Reset()
        {
            base.Reset();
            FrameLayouts.Clear();
        }

        public DescriptorSetLayout CreateUniqueDescriptorSetLayout(DescriptorSetLayoutBuilder descriptorSetLayoutBuilder)
        {
            DescriptorSetLayout descriptorSetLayout;

            if (!createdDescriptorSetLayouts.TryGetValue(descriptorSetLayoutBuilder.Hash, out descriptorSetLayout))
            {
                descriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, descriptorSetLayoutBuilder);
                createdDescriptorSetLayouts.Add(descriptorSetLayoutBuilder.Hash, descriptorSetLayout);
            }

            return descriptorSetLayout;
        }

        private RenderSystemResourceGroupLayout CreateDrawResourceGroupLayout(DescriptorSetLayoutBuilder bindingBuilder, EffectBytecode effectBytecode)
        {
            if (bindingBuilder == null)
                return null;

            var constantBufferReflection = effectBytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerDraw");

            var result = new RenderSystemResourceGroupLayout
            {
                DescriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, bindingBuilder),
                ConstantBufferReflection = constantBufferReflection,
            };

            if (constantBufferReflection != null)
            {
                result.ConstantBufferSize = constantBufferReflection.Size;
                result.ConstantBufferHash = constantBufferReflection.Hash;
            }

            // Resolve slots
            result.ConstantBufferOffsets = new int[drawCBufferOffsetSlots.Count];
            for (int index = 0; index < drawCBufferOffsetSlots.Count; index++)
            {
                ResolveCBufferOffset(result, index, drawCBufferOffsetSlots[index].Variable);
            }

            return result;
        }

        private FrameResourceGroupLayout CreateFrameResourceGroupLayout(DescriptorSetLayoutBuilder bindingBuilder, EffectBytecode effectBytecode)
        {
            if (bindingBuilder == null)
                return null;

            var constantBufferReflection = effectBytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerFrame");

            // We combine both hash for DescriptorSet and cbuffer itself (if it exists)
            var hash = bindingBuilder.Hash;
            if (constantBufferReflection != null)
                ObjectId.Combine(ref hash, ref constantBufferReflection.Hash, out hash);

            FrameResourceGroupLayout result;
            if (!frameResourceLayouts.TryGetValue(hash, out result))
            {
                result = new FrameResourceGroupLayout
                {
                    DescriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, bindingBuilder),
                    ConstantBufferReflection = constantBufferReflection,
                };

                result.Entry.Resources = new ResourceGroup();

                if (constantBufferReflection != null)
                {
                    result.ConstantBufferSize = constantBufferReflection.Size;
                    result.ConstantBufferHash = constantBufferReflection.Hash;
                }

                // Resolve slots
                result.ConstantBufferOffsets = new int[frameCBufferOffsetSlots.Count];
                for (int index = 0; index < frameCBufferOffsetSlots.Count; index++)
                {
                    ResolveCBufferOffset(result, index, frameCBufferOffsetSlots[index].Variable);
                }

                frameResourceLayouts.Add(hash, result);
            }

            return result;
        }

        private ViewResourceGroupLayout CreateViewResourceGroupLayout(DescriptorSetLayoutBuilder bindingBuilder, EffectBytecode effectBytecode)
        {
            if (bindingBuilder == null)
                return null;

            var constantBufferReflection = effectBytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerView");

            // We combine both hash for DescriptorSet and cbuffer itself (if it exists)
            var hash = bindingBuilder.Hash;
            if (constantBufferReflection != null)
                ObjectId.Combine(ref hash, ref constantBufferReflection.Hash, out hash);

            ViewResourceGroupLayout result;
            if (!viewResourceLayouts.TryGetValue(hash, out result))
            {
                result = new ViewResourceGroupLayout
                {
                    DescriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, bindingBuilder),
                    ConstantBufferReflection = constantBufferReflection,
                    Entries = new ResourceGroupEntry[RenderSystem.Views.Count],
                };

                for (int index = 0; index < result.Entries.Length; index++)
                {
                    result.Entries[index].Resources = new ResourceGroup();
                }

                if (constantBufferReflection != null)
                {
                    result.ConstantBufferSize = constantBufferReflection.Size;
                    result.ConstantBufferHash = constantBufferReflection.Hash;
                }

                // Resolve slots
                result.ConstantBufferOffsets = new int[viewCBufferOffsetSlots.Count];
                for (int index = 0; index < viewCBufferOffsetSlots.Count; index++)
                {
                    ResolveCBufferOffset(result, index, viewCBufferOffsetSlots[index].Variable);
                }

                viewResourceLayouts.Add(hash, result);
            }

            return result;
        }

        protected override int ComputeDataArrayExpectedSize(DataType type)
        {
            switch (type)
            {
                case DataType.EffectObject:
                    return EffectObjectNodes.Count;
            }

            return base.ComputeDataArrayExpectedSize(type);
        }

        private void RenderStages_CollectionChanged(object sender, Core.Collections.TrackingCollectionChangedEventArgs e)
        {
            var renderStage = (RenderStage)e.Item;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Array.Resize(ref effectSlots, RenderSystem.RenderStages.Count);
                    effectSlots[e.Index] = CreateEffectPermutationSlot(renderStage.EffectSlotName);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    // TODO GRAPHICS REFACTOR support removal of render stages
                    throw new NotImplementedException();
            }
        }

        struct ConstantBufferOffsetDefinition
        {
            public readonly string Variable;

            public ConstantBufferOffsetDefinition(string variable)
            {
                Variable = variable;
            }
        }
    }
}