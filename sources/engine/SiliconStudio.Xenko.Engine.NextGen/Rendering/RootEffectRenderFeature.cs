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
        private readonly List<string> effectDescriptorSetSlots = new List<string>();
        private readonly Dictionary<string, int> effectPermutationSlots = new Dictionary<string, int>();
        private readonly Dictionary<ObjectId, FrameResourceGroupLayout> frameResourceLayouts = new Dictionary<ObjectId, FrameResourceGroupLayout>();
        private readonly Dictionary<ObjectId, ViewResourceGroupLayout> viewResourceLayouts = new Dictionary<ObjectId, ViewResourceGroupLayout>();

        private readonly Dictionary<ObjectId, DescriptorSetLayout> createdDescriptorSetLayouts = new Dictionary<ObjectId, DescriptorSetLayout>();

        private readonly List<ConstantBufferOffsetDefinition> viewCBufferOffsetSlots = new List<ConstantBufferOffsetDefinition>();
        private readonly List<ConstantBufferOffsetDefinition> frameCBufferOffsetSlots = new List<ConstantBufferOffsetDefinition>();
        private readonly List<ConstantBufferOffsetDefinition> drawCBufferOffsetSlots = new List<ConstantBufferOffsetDefinition>();

        // Common slots
        private EffectDescriptorSetReference perFrameDescriptorSetSlot;
        private EffectDescriptorSetReference perViewDescriptorSetSlot;
        private EffectDescriptorSetReference perDrawDescriptorSetSlot;

        internal List<EffectObjectNode> EffectObjectNodes { get; } = new List<EffectObjectNode>();

        public ResourceGroup[] ResourceGroupPool = new ResourceGroup[256];

        public List<FrameResourceGroupLayout> FrameLayouts { get; } = new List<FrameResourceGroupLayout>();
        public Action<NextGenRenderSystem, Effect, RenderEffectReflection> EffectCompiled;

        /// <summary>
        /// Gets number of effect permutation slot, which is the number of effect cached per object.
        /// </summary>
        public int EffectPermutationSlotCount => effectPermutationSlots.Count;

        /// <summary>
        /// Key to store extra info for each effect instantiation of each object.
        /// </summary>
        public StaticEffectObjectPropertyKey<RenderEffect> RenderEffectKey;

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
        public override void Initialize()
        {
            base.Initialize();

            // Create RenderEffectKey
            RenderEffectKey = CreateStaticEffectObjectKey<RenderEffect>();

            // TODO: Assign weights so that PerDraw is always last? (we usually most custom user ones to be between PerView and PerDraw)
            perFrameDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerFrame");
            perViewDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerView");
            perDrawDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerDraw");

            // Create effect slots
            foreach (var renderStage in RenderSystem.RenderStages)
            {
                renderStage.EffectSlot = CreateEffectPermutationSlot(renderStage.Name);
            }
        }

        /// <summary>
        /// Compute the index of first descriptor set stored in <see cref="ResourceGroupPool"/>.
        /// </summary>
        internal int ComputeResourceGroupOffset(RenderNodeReference renderNode)
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
                if (resourceGroupLayout.ConstantBufferOffsets == null || resourceGroupLayout.ConstantBufferOffsets.Length < viewCBufferOffsetSlots.Count)
                    Array.Resize(ref resourceGroupLayout.ConstantBufferOffsets, viewCBufferOffsetSlots.Count);

                ResolveCBufferOffset(resourceGroupLayout, slotReference.Index, variable);
            }

            return slotReference;
        }

        private void ResolveCBufferOffset(ResourceGroupLayout resourceGroupLayout, int index, string variable)
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
        public override void PrepareEffectPermutations(NextGenRenderSystem RenderSystem)
        {
            base.PrepareEffectPermutations(RenderSystem);

            // TODO: Temporary until we have a better system for handling permutations
            var renderEffects = GetData(RenderEffectKey);
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
                    var staticEffectObjectNode = staticObjectNode.CreateEffectReference(effectSlotCount, renderNode.RenderStage.EffectSlot.Index);
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
                    var staticEffectObjectNode = staticObjectNode.CreateEffectReference(effectSlotCount, i);
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip if not used or nothing changed
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem) || renderEffect.EffectValidator.EndEffectValidation())
                        continue;

                    foreach (var effectValue in renderEffect.EffectValidator.EffectValues)
                    {
                        compilerParameters.SetObject(effectValue.Key, effectValue.Value);
                    }

                    var effect = RenderSystem.EffectSystem.LoadEffect(renderEffect.EffectName, compilerParameters).WaitForResult();

                    RenderEffectReflection renderEffectReflection;
                    if (!InstantiatedEffects.TryGetValue(effect, out renderEffectReflection))
                    {
                        renderEffectReflection = new RenderEffectReflection();

                        // Build root signature automatically from reflection

                        renderEffectReflection.Binder.Compile(RenderSystem.GraphicsDevice, effect.Bytecode, effectDescriptorSetSlots);

                        // Prepare well-known descriptor set layouts
                        renderEffectReflection.PerDrawLayout = CreateDrawResourceGroupLayout(RenderSystem, renderEffectReflection.Binder.DescriptorReflection.GetLayout("PerDraw"), effect.Bytecode);
                        renderEffectReflection.PerFrameLayout = CreateFrameResourceGroupLayout(RenderSystem, renderEffectReflection.Binder.DescriptorReflection.GetLayout("PerFrame"), effect.Bytecode);
                        renderEffectReflection.PerViewLayout = CreateViewResourceGroupLayout(RenderSystem, renderEffectReflection.Binder.DescriptorReflection.GetLayout("PerView"), effect.Bytecode);

                        InstantiatedEffects.Add(effect, renderEffectReflection);

                        // Notify a new effect has been compiled
                        EffectCompiled?.Invoke(RenderSystem, effect, renderEffectReflection);
                    }

                    renderEffect.Effect = effect;
                    renderEffect.Reflection = renderEffectReflection;
                    renderEffects[staticEffectObjectNode] = renderEffect;

                    compilerParameters.Clear();
                }
            }
        }

        /// <inheritdoc/>
        public override void Prepare()
        {
            EffectObjectNodes.Clear();

            int requiredDescriptorSets = 0;

            // Make sure descriptor set pool is large enough
            var expectedDescriptorSetPoolSize = renderNodes.Count * effectDescriptorSetSlots.Count;
            if (ResourceGroupPool.Length < expectedDescriptorSetPoolSize)
                Array.Resize(ref ResourceGroupPool, expectedDescriptorSetPoolSize);

            // Allocate PerFrame, PerView and PerDraw resource groups and constant buffers
            var renderEffects = GetData(RenderEffectKey);
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
                    var staticEffectObjectNode = staticObjectNode.CreateEffectReference(effectSlotCount, renderNode.RenderStage.EffectSlot.Index);
                    var renderEffect = renderEffects[staticEffectObjectNode];
                    var renderEffectReflection = renderEffects[staticEffectObjectNode].Reflection;

                    // PerView resources/cbuffer
                    var viewLayout = renderEffectReflection.PerViewLayout;
                    if (viewLayout.Entries[view.Index].MarkAsUsed(RenderSystem))
                    {
                        PrepareResourceGroup(RenderSystem, viewLayout, BufferPoolAllocationType.UsedMultipleTime, viewLayout.Entries[view.Index].Resources);

                        // Register it in list of view layouts to update for this frame
                        viewFeature.Layouts.Add(viewLayout);
                    }

                    // PerFrame resources/cbuffer
                    var frameLayout = renderEffect.Reflection.PerFrameLayout;
                    if (frameLayout != null && frameLayout.Entry.MarkAsUsed(RenderSystem))
                    {
                        PrepareResourceGroup(RenderSystem, viewLayout, BufferPoolAllocationType.UsedMultipleTime, frameLayout.Entry.Resources);

                        // Register it in list of view layouts to update for this frame
                        FrameLayouts.Add(frameLayout);
                    }

                    // PerDraw resources/cbuffer
                    // Get nodes
                    var viewObjectNode = GetViewObjectNode(renderNode.ViewObjectNode);

                    // Allocate descriptor set
                    renderNode.Resources = AllocateTemporaryResourceGroup();
                    PrepareResourceGroup(RenderSystem, renderEffectReflection.PerDrawLayout, BufferPoolAllocationType.UsedOnce, renderNode.Resources);

                    // Link to EffectObjectNode (created right after)
                    // TODO: rewrite this
                    renderNode.EffectObjectNode = new EffectObjectNodeReference(EffectObjectNodes.Count);

                    renderNode.RenderEffect = renderEffect;

                    // Bind well-known descriptor sets
                    var descriptorSetPoolOffset = ComputeResourceGroupOffset(renderNodeReference);
                    ResourceGroupPool[descriptorSetPoolOffset + perFrameDescriptorSetSlot.Index] = renderEffect.Reflection.PerFrameResources;
                    ResourceGroupPool[descriptorSetPoolOffset + perViewDescriptorSetSlot.Index] = renderEffect.Reflection.PerViewLayout.Entries[view.Index].Resources;
                    ResourceGroupPool[descriptorSetPoolOffset + perDrawDescriptorSetSlot.Index] = renderNode.Resources;

                    renderNodes[renderNodeReference.Index] = renderNode;

                    // Create EffectObjectNode
                    EffectObjectNodes.Add(new EffectObjectNode(renderEffect, viewObjectNode.ObjectNode));
                }
            }
        }

        private ResourceGroup AllocateTemporaryResourceGroup()
        {
            // TODO: Provide a better implementation that avoids allocation (i.e. using a pool)
            return new ResourceGroup();
        }

        public static void PrepareResourceGroup(NextGenRenderSystem renderSystem, ResourceGroupLayout resourceGroupLayout, BufferPoolAllocationType constantBufferAllocationType, ResourceGroup resourceGroup)
        {
            // TODO: Make sure ResourceGroup is properly created
            if (resourceGroup == null)
                throw new InvalidOperationException();

            resourceGroup.DescriptorSet = DescriptorSet.New(renderSystem.GraphicsDevice, renderSystem.DescriptorPool, resourceGroupLayout.DescriptorSetLayout);

            if (resourceGroupLayout.ConstantBufferSize > 0)
            {
                renderSystem.BufferPool.Allocate(renderSystem.GraphicsDevice, resourceGroupLayout.ConstantBufferSize, constantBufferAllocationType, ref resourceGroup.ConstantBuffer);
            }
        }

        public override void Reset()
        {
            base.Reset();
            FrameLayouts.Clear();
        }

        public DescriptorSetLayout CreateUniqueDescriptorSetLayout(NextGenRenderSystem RenderSystem, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder)
        {
            DescriptorSetLayout descriptorSetLayout;

            if (!createdDescriptorSetLayouts.TryGetValue(descriptorSetLayoutBuilder.Hash, out descriptorSetLayout))
            {
                descriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, descriptorSetLayoutBuilder);
                createdDescriptorSetLayouts.Add(descriptorSetLayoutBuilder.Hash, descriptorSetLayout);
            }

            return descriptorSetLayout;
        }

        private ResourceGroupLayout CreateDrawResourceGroupLayout(NextGenRenderSystem RenderSystem, DescriptorSetLayoutBuilder bindingBuilder, EffectBytecode effectBytecode)
        {
            if (bindingBuilder == null)
                return null;

            // TODO: This code might need some improvements (waiting to have better visibility on how we define resource groups and descriptor layouts)
            // TODO: For now, assume cbuffer is always in slot 0 (if it exists)
            var constantBufferSlot = 0;
            var constantBufferReflection = effectBytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerDraw");

            var result = new FrameResourceGroupLayout
            {
                DescriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, bindingBuilder),
                ConstantBufferSlot = constantBufferSlot,
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

        private FrameResourceGroupLayout CreateFrameResourceGroupLayout(NextGenRenderSystem RenderSystem, DescriptorSetLayoutBuilder bindingBuilder, EffectBytecode effectBytecode)
        {
            if (bindingBuilder == null)
                return null;

            // TODO: This code might need some improvements (waiting to have better visibility on how we define resource groups and descriptor layouts)
            // TODO: For now, assume cbuffer is always in slot 0 (if it exists)
            var constantBufferSlot = 0;
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
                    ConstantBufferSlot = constantBufferSlot,
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

        private ViewResourceGroupLayout CreateViewResourceGroupLayout(NextGenRenderSystem RenderSystem, DescriptorSetLayoutBuilder bindingBuilder, EffectBytecode effectBytecode)
        {
            if (bindingBuilder == null)
                return null;

            // TODO: This code might need some improvements (waiting to have better visibility on how we define resource groups and descriptor layouts)
            // TODO: For now, assume cbuffer is always in slot 0 (if it exists)
            var constantBufferSlot = 0;
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
                    ConstantBufferSlot = constantBufferSlot,
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

        protected override int ComputeDataArrayExpectedSize(NextGenRenderSystem RenderSystem, DataType type)
        {
            switch (type)
            {
                case DataType.EffectObject:
                    return EffectObjectNodes.Count;
                case DataType.EffectView:
                    return base.ComputeDataArrayExpectedSize(RenderSystem, DataType.View) * EffectPermutationSlotCount;
                case DataType.StaticEffectObject:
                    return base.ComputeDataArrayExpectedSize(RenderSystem, DataType.StaticObject) * EffectPermutationSlotCount;
            }

            return base.ComputeDataArrayExpectedSize(RenderSystem, type);
        }

        protected override void RemoveRenderObjectFromDataArray(DataArray dataArray, int removedIndex)
        {
            if (dataArray.Info.Type == DataType.StaticEffectObject)
            {
                // SwapRemove all the items related to this RenderObject (count: EffectPermutationSlotCount)
                dataArray.Info.SwapRemoveItems(dataArray.Array, removedIndex, (RenderObjects.Count - 1) * EffectPermutationSlotCount, EffectPermutationSlotCount);
            }

            base.RemoveRenderObjectFromDataArray(dataArray, removedIndex);
        }

        struct ConstantBufferOffsetDefinition
        {
            public readonly string Variable;

            public ConstantBufferOffsetDefinition(string variable)
            {
                Variable = variable;
            }
        }

        struct DescriptorSetLayoutEntry
        {
            public string Name;
            public DescriptorSetLayout Layout;
            public ObjectId LayoutHash;
        }
    }
}