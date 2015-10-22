// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    public class ParticlePlugin : RenderPassPlugin, IRenderPassPluginTarget
    {
        private int capacityCount;

        private int updatersCount;

        private RenderPass updatePasses;

        private RenderPass copyPass;

        private RenderPass sortPass;

        private RenderPass bitonicSort1Pass1;

        private RenderPass bitonicSort1Pass2;

        private RenderPass[] bitonicSort2Passes;

        private RenderPass renderPass;

        private Buffer globalBuffer;

        private Buffer sortBuffer;

        private readonly List<ParticleUpdaterState> updatersToRemove;

        private readonly List<ParticleUpdaterState> currentUpdaters;

        private readonly RenderPassListEnumerator meshesToRender;

        private EffectMesh effectMeshCopyToSortBuffer;

        private EffectMesh effectMeshSort1Pass1;
        private EffectMesh effectMeshSort1Pass2;
        private EffectMesh[] effectMeshBitonicSort2;
        private EffectMesh effectMeshRender;

        private const int MaximumThreadPerGroup = 512;
        private const int MaximumDepthLevel = 512;

        private EffectOld effectCopyToSortBuffer;
        private EffectOld effectBitonicSort1Pass1;
        private EffectOld effectBitonicSort1Pass2;

        private EffectOld[] effectBitonicSort2;

        private int currentParticleCount = 0;

        private IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticlePlugin" /> class.
        /// </summary>
        public ParticlePlugin() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticlePlugin" /> class.
        /// </summary>
        /// <param name="name">Name of this particle manager</param>
        public ParticlePlugin(string name) : base(name)
        {
            meshesToRender = new RenderPassListEnumerator();
            Updaters = new TrackingCollection<ParticleEmitterComponent>();
            currentUpdaters = new List<ParticleUpdaterState>();
            updatersToRemove = new List<ParticleUpdaterState>();
            Updaters.CollectionChanged += UpdatersOnCollectionChanged;
            EnableSorting = true;
        }

        /// <summary>
        /// Gets or sets the maximum particle count for all updaters, must be power of 2.
        /// </summary>
        /// <value>The capacity count.</value>
        /// <remarks>
        /// The sum of ParticlesUpdaters.CapacityCount must be lower than this value.
        /// </remarks>
        public int CapacityCount
        {
            get
            {
                return capacityCount;
            }
            set
            {
                if (ParticleUtils.CalculateMaximumPowerOf2Count(value) != value)
                    throw new ArgumentException("CapacityCount must be a power of 2");

                if (value < updatersCount)
                    throw new ArgumentException(string.Format("Cannot change Maximum Count [{0}] to be lower than the sum of Updaters.MaximumCount [{1}]", value,
                    updatersCount));

                capacityCount = value;
            }
        }

        public int StructureSize { get; set; }

        public string StructureName { get; set; }

        public ShaderClassSource RenderShader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sort is enabled (true by default).
        /// </summary>
        /// <value><c>true</c> if [sort is enabled]; otherwise, <c>false</c>.</value>
        public bool EnableSorting { get; set; }

        public MainPlugin MainPlugin { get; set; }

        public RenderTargetsPlugin MainTargetPlugin { get; set; }

        /// <summary>
        /// Gets the particle updaters.
        /// </summary>
        /// <value>The particle updaters.</value>
        public TrackingCollection<ParticleEmitterComponent> Updaters { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();

            // Create and register render passes
            updatePasses = new RenderPass("Update");
            copyPass = new RenderPass("Copy");
            sortPass = new RenderPass("Sort");

            bitonicSort1Pass1 = new RenderPass("BitonicSort1Pass1");
            bitonicSort1Pass2 = new RenderPass("BitonicSort1Pass2");
            bitonicSort2Passes = new RenderPass[(int)Math.Log(MaximumDepthLevel, 2) - 1];
            effectBitonicSort2 = new EffectOld[bitonicSort2Passes.Length];

            renderPass = new RenderPass("Render");
        }

        public override void Load()
        {
            base.Load();

            if (string.IsNullOrEmpty(StructureName) || StructureSize == 0)
                throw new InvalidOperationException("StructureName and StructureSize must be setup on ParticlePlugin");

            // Add passes to the render pass
            RenderPass.AddPass(updatePasses, copyPass, sortPass, renderPass);

            // Create effect to copy from particle buffer to sort buffer
            effectCopyToSortBuffer = this.EffectSystemOld.BuildEffect("CopyToSortBuffer").Using(
                new ComputeShaderPlugin(new ShaderClassSource("ParticleSortInitializer"), MaximumThreadPerGroup, 1, 1)
                    {
                        RenderPass = copyPass,
                        Macros = { new ShaderMacro("PARTICLE_STRUCT", StructureName) }
                    });
            effectCopyToSortBuffer.KeepAliveBy(ActiveObjects);
            effectCopyToSortBuffer.Parameters.AddSources(MainPlugin.ViewParameters);
            effectMeshCopyToSortBuffer = new EffectMesh(effectCopyToSortBuffer);

            // Clear the sort buffer
            copyPass.StartPass.AddFirst = (context) =>
                {
                    if (CapacityCount > 0)
                    {
                        // TODO handle progressive sorting
                        context.GraphicsDevice.ClearReadWrite(sortBuffer, new UInt4(0xFF7FFFFF)); // 0xFF7FFFFF = - float.MaxValue
                    }
                };

            // Create effect for bitonic sort 1 - pass 1
            effectBitonicSort1Pass1 = this.EffectSystemOld.BuildEffect("ParticleBitonicSort1-Pass1").Using(
                new ComputeShaderPlugin(new ShaderClassSource("ParticleBitonicSort1"), MaximumThreadPerGroup, 1, 1)
                {
                    RenderPass = bitonicSort1Pass1,
                    Macros = { new ShaderMacro("PARTICLE_STRUCT", StructureName),
                                new ShaderMacro("PARTICLE_SORT_PASS", 0)
                    }
                });
            effectBitonicSort1Pass1.KeepAliveBy(this);
            effectMeshSort1Pass1 = new EffectMesh(effectBitonicSort1Pass1);

            // Create effect for bitonic sort 1 - pass 2
            effectBitonicSort1Pass2 = this.EffectSystemOld.BuildEffect("ParticleBitonicSort1-Pass2").Using(
                new ComputeShaderPlugin(new ShaderClassSource("ParticleBitonicSort1"), MaximumThreadPerGroup, 1, 1)
                {
                    RenderPass = bitonicSort1Pass2,
                    Macros = { new ShaderMacro("PARTICLE_STRUCT", StructureName),
                                new ShaderMacro("PARTICLE_SORT_PASS", 1)
                    }
                });
            effectBitonicSort1Pass2.KeepAliveBy(this);
            effectMeshSort1Pass2 = new EffectMesh(effectBitonicSort1Pass2);

            // Creates Effect for bitonic sort 2
            var currentDepth = MaximumDepthLevel;
            for (int i = 0; i < bitonicSort2Passes.Length; i++)
            {
                var bitonicShader = new RenderPass(string.Format("Bitonic-{0}", currentDepth));
                bitonicSort2Passes[i] = bitonicShader;

                // Compile the new shader for this count
                effectBitonicSort2[i] = this.EffectSystemOld.BuildEffect("ParticleBitonicSort2-" + currentDepth).Using(
                    new ComputeShaderPlugin(new ShaderClassSource("ParticleBitonicSort2", currentDepth), MaximumThreadPerGroup, 1, 1)
                        {
                            RenderPass = bitonicShader,
                            Macros = { new ShaderMacro("PARTICLE_STRUCT", StructureName) }
                        });
                effectBitonicSort2[i].KeepAliveBy(this);

                currentDepth /= 2;
            }
            effectMeshBitonicSort2 = new EffectMesh[bitonicSort2Passes.Length];
            for (int i = 0; i < effectMeshBitonicSort2.Length; i++)
                effectMeshBitonicSort2[i] = new EffectMesh(effectBitonicSort2[i]);

            // Creates Effect for rendering
            EffectOld particleRenderEffect = this.EffectSystemOld.BuildEffect("ParticleRender")
                .Using(new StateShaderPlugin() { UseBlendState = true, UseDepthStencilState = true, RenderPass = renderPass })
                .Using(new BasicShaderPlugin(RenderShader)
                        {
                            RenderPass = renderPass,
                            Macros = { new ShaderMacro("PARTICLE_STRUCT", StructureName) }
                        });
            particleRenderEffect.KeepAliveBy(this);

            particleRenderEffect.Parameters.AddSources(this.Parameters);
            particleRenderEffect.Parameters.AddSources(MainPlugin.ViewParameters);
            particleRenderEffect.Parameters.Set(EffectPlugin.BlendStateKey, graphicsDeviceService.GraphicsDevice.BlendStates.AlphaBlend);
            particleRenderEffect.Parameters.Set(EffectPlugin.DepthStencilStateKey, MainTargetPlugin.DepthStencilState);
            particleRenderEffect.Parameters.Set(RenderTargetKeys.DepthStencilSource, MainTargetPlugin.DepthStencil.Texture);

            effectMeshRender = new EffectMesh(particleRenderEffect);

            effectMeshRender.Render += (context) =>
            {
                if (currentParticleCount > 0)
                {
                    context.GraphicsDevice.SetVertexArrayObject(null);
                    context.GraphicsDevice.SetViewport(MainTargetPlugin.Viewport);
                    //context.GraphicsDevice.SetRenderTargets(MainTargetPlugin.DepthStencil, 0, MainTargetPlugin.RenderTarget);
                    
                    context.GraphicsDevice.SetRenderTargets((MainTargetPlugin.DepthStencilReadOnly != null) ? MainTargetPlugin.DepthStencilReadOnly : MainTargetPlugin.DepthStencil, RenderTarget);

                    // TODO HANDLE dynamic count
                    //context.GraphicsDevice.Draw(PrimitiveType.PointList, CapacityCount);
                    context.GraphicsDevice.Draw(PrimitiveType.PointList, currentParticleCount);
                }
                //context.GraphicsDevice.SetRenderTargets(null, 0, new ITexture2D[] { null } );
            };

            if (OfflineCompilation)
                return;

            // Allocate global buffers at register time
            // Buffers can also be reallocated at runtime
            OnCapacityCountChange();

            // Add our local meshes to render RenderContext.RenderPassEnumerators
            RenderSystem.RenderPassEnumerators.Add(meshesToRender);

            // Register update per frame
            RenderSystem.GlobalPass.StartPass += OnFrameUpdate;

            // Register sort pass
            sortPass.EndPass.Set = ComputeBitonicSort;
        }

        public override void Unload()
        {
            if (!OfflineCompilation)
            {

            }

            base.Unload();
        }

        /// <summary>
        /// Allocates global buffers.
        /// </summary>
        private void OnCapacityCountChange()
        {
            // Allocate global buffers
            bool allocateGlobalBuffer = false;
            if (globalBuffer == null)
            {
                allocateGlobalBuffer = true;
            }
            else if (globalBuffer.ElementCount != CapacityCount)
            {
                // Reallocate global buffer if capacity changed
                allocateGlobalBuffer = true;
                // Release previous buffer
                globalBuffer.Release();
                sortBuffer.Release();
            }
            
            // Allocate new buffers
            if (allocateGlobalBuffer)
            {
                if (CapacityCount > 0)
                {
                    globalBuffer = Buffer.Structured.New(graphicsDeviceService.GraphicsDevice, CapacityCount, StructureSize, true);
                    globalBuffer.Name = "ParticleGlobalBuffer";
                    sortBuffer = Buffer.Structured.New(graphicsDeviceService.GraphicsDevice, CapacityCount, sizeof(int) * 2, true);
                    sortBuffer.Name = "ParticleSortBuffer";
                }
            }
        }

        private static void InvokeMeshPass(EffectMesh mesh, ThreadContext context, bool skipEffectPass = false)
        {
            var effectPass = mesh.EffectPass;

            // Execute EffectPass - StartPass
            if (!skipEffectPass)
                effectPass.StartPass.Invoke(context);

            // Execute MeshPass - StartPass - EndPass
            context.EffectMesh = mesh;
            mesh.Render.Invoke(context);
            context.EffectMesh = null;

            // Execute EffectPass - EndPass
            if (!skipEffectPass)
                effectPass.EndPass.Invoke(context);
        }

        private void OnFrameUpdate(ThreadContext context)
        {
            updatePasses.Enabled = true; //!RenderContext.IsPaused;

            // Add new updater to the current list
            foreach (var particleUpdater in Updaters)
            {
                ParticleUpdaterState particleUpdaterState = null;
                foreach (ParticleUpdaterState state in currentUpdaters)
                {
                    if (ReferenceEquals(state.UserState, particleUpdater))
                    {
                        particleUpdaterState = state;
                        break;
                    }
                }
                if (particleUpdaterState == null)
                    currentUpdaters.Add(new ParticleUpdaterState() { UserState = particleUpdater, StructureSize =  StructureSize});
            }

            // Get updater to remove from current list
            foreach (var particleUpdater in currentUpdaters)
            {
                ParticleEmitterComponent particleUpdaterState = null;
                foreach (ParticleEmitterComponent state in Updaters)
                {
                    if (ReferenceEquals(state, particleUpdater.UserState))
                    {
                        particleUpdaterState = state;
                        break;
                    }
                }
                if (particleUpdaterState == null)
                    updatersToRemove.Add(particleUpdater);
            }

            // Remove from the current list
            foreach (var particleUpdaterState in updatersToRemove)
            {
                currentUpdaters.Remove(particleUpdaterState);

                // Remove the mesh to be rendered
                meshesToRender.RemoveMesh(particleUpdaterState.MeshUpdater);

                // Dispose the previous particule updater as it is no longer used
                particleUpdaterState.DisposeBuffers();
            }

            // Reallocate global buffers if needed
            if (updatersCount > capacityCount)
            {
                CapacityCount = updatersCount;
                OnCapacityCountChange();
            }

            int particleUpdaterIndex = 1;

            currentParticleCount = 0;
            // Update start index into global buffers for all CPU/GPU static buffers.
            int startIndex = 0;
            for (int i = 0; i < currentUpdaters.Count; i++)
            {
                var currentState = currentUpdaters[i];
                var userState = currentState.UserState;

                if (!userState.IsDynamicEmitter)
                {
                    // If there is a change from dynamic type to CPU/GPU static buffer, we need to deallocate previous Append/ConsumeBuffers
                    if (currentState.IsDynamicEmitter)
                        currentState.DisposeBuffers();
                }
                else if (userState.Count > currentState.Count)
                {
                    // This is a dynamic buffer and the new buffer is larger than previous one
                    // or we simply need to allocate new consume/append buffers
                    currentState.AllocateBuffers(context.GraphicsDevice);
                }

                // Create Effect shader
                if (!ReferenceEquals(userState.Shader, currentState.Shader) || currentState.Count != userState.Count)
                {
                    // Remove the effect pass
                    updatePasses.RemovePass(currentState.EffectPass);

                    // Dispose previous effects
                    currentState.DisposeEffects();

                    // Remove mesh
                    if (currentState.MeshUpdater != null)
                    {
                        meshesToRender.RemoveMesh(currentState.MeshUpdater);
                        currentState.MeshUpdater = null;
                    }

                    // Compile new shader
                    if (userState.Shader != null)
                    {
                        string name = userState.Name ?? string.Format("{0}{1}", userState.Shader.ClassName, particleUpdaterIndex++);

                        currentState.EffectPass = new RenderPass(name);
                        updatePasses.AddPass(currentState.EffectPass);

                        // Calculate the best dispatch thread count
                        int dispatchCount = MaximumThreadPerGroup;
                        while (dispatchCount > 0)
                        {
                            // If the maximum count is a multiple of the current dispatchCount use it
                            if ((userState.Count & (dispatchCount - 1)) == 0)
                                break;
                            dispatchCount >>= 1;
                        }

                        // Compile the new shader for this count
                        currentState.EffectUpdater = this.EffectSystemOld.BuildEffect(name).Using(
                            new ComputeShaderPlugin(userState.Shader, dispatchCount, 1, 1)
                                {
                                    RenderPass = currentState.EffectPass,
                                    Macros = { new ShaderMacro("PARTICLE_STRUCT", StructureName) }
                                });

                        // Instantiate the mesh updater
                        var meshParams = new ParameterCollection(name);
                        currentState.MeshUpdater = new EffectMesh(currentState.EffectUpdater, new Mesh { Parameters = meshParams }).Dispatch(userState.Count / dispatchCount, 1, 1);
                        currentState.MeshUpdater.Parameters.AddSources(Parameters);
                        currentState.MeshUpdater.Parameters.AddSources(MainPlugin.ViewParameters);
                        currentState.MeshUpdater.Parameters.AddSources(userState.Parameters);

                        // Setup Append/Consume 
                        if (userState.IsDynamicEmitter)
                        {
                            currentState.MeshUpdater.Parameters.Set(ParticleBaseKeys.ParticleInputBuffer, currentState.ConsumeBuffer);
                            currentState.MeshUpdater.Parameters.Set(ParticleBaseKeys.ParticleOutputBuffer, currentState.AppendBuffer);
                            currentState.MeshUpdater.Parameters.Set(ParticleBaseKeys.ParticleStartIndex, (uint)0);
                        }
                        else
                        {
                            // Setup update on global buffer
                            currentState.MeshUpdater.Parameters.Set(ParticleBaseKeys.ParticleGlobalBuffer, globalBuffer);
                            currentState.MeshUpdater.Parameters.Set(ParticleBaseKeys.ParticleStartIndex, (uint)startIndex);
                        }

                        // Add this updater to the rendering list
                        meshesToRender.AddMesh(currentState.MeshUpdater);
                    }
                }

                // Setup Append/Consume 
                if (currentState.MeshUpdater != null && !userState.IsDynamicEmitter && currentState.StartIndex != startIndex)
                {
                    // Setup update on global buffer
                    currentState.MeshUpdater.Parameters.Set(ParticleBaseKeys.ParticleStartIndex, (uint)startIndex);
                }

                // Transfer CPU buffer to GPU
                if (!ReferenceEquals(userState.ParticleData, currentState.ParticleData)
                    || userState.UpdateNextBuffer
                    || userState.Type == ParticleEmitterType.CpuDynamic 
                    || currentState.StartIndex != startIndex
                    || currentState.Count != userState.Count
                    )
                {
                    // Update the data if necessary
                    userState.OnUpdateData();

                    if (userState.ParticleData != null)
                    {
                        var handle = GCHandle.Alloc(userState.ParticleData, GCHandleType.Pinned);
                        globalBuffer.SetData(context.GraphicsDevice, new DataPointer(handle.AddrOfPinnedObject(), userState.Count * StructureSize), startIndex * StructureSize);
                        handle.Free();
                    }
                    userState.UpdateNextBuffer = false;
                }

                // Replicate the shader to the current state
                currentState.Type = userState.Type;
                currentState.Shader = userState.Shader;
                currentState.Count = userState.Count;
                currentState.ParticleElementSize = userState.ParticleElementSize;
                currentState.ParticleData = userState.ParticleData;
                currentState.StartIndex = startIndex;

                // Copy Maximum count to current state
                currentParticleCount += userState.Count;

                // Update start index
                startIndex += userState.Count;
            }

            // Add mesh to the render list
            if (updatersCount > 0)
            {
                if (!isParticleRenderingEnabled)
                {
                    isParticleRenderingEnabled = true;
                    
                    meshesToRender.AddMesh(effectMeshRender);
                    meshesToRender.AddMesh(effectMeshCopyToSortBuffer);
                }
            }
            else
            {
                isParticleRenderingEnabled = false;
                meshesToRender.RemoveMesh(effectMeshRender);
                meshesToRender.RemoveMesh(effectMeshCopyToSortBuffer);
            }

            // Prepare mesh for rendering
            PrepareForRendering();
        }

        private bool isParticleRenderingEnabled;



        private void PrepareForRendering()
        {
            effectMeshCopyToSortBuffer.Parameters.Set(ComputeShaderPlugin.ThreadGroupCount, new Int3(currentParticleCount / MaximumThreadPerGroup, 1, 1));
            effectMeshCopyToSortBuffer.Parameters.Set(ParticleBaseKeys.ParticleCount, (uint)currentParticleCount);

            effectMeshCopyToSortBuffer.Parameters.Set(ParticleBaseKeys.ParticleSortBuffer, sortBuffer);
            effectMeshCopyToSortBuffer.Parameters.Set(ParticleBaseKeys.ParticleGlobalBuffer, globalBuffer);

            effectMeshRender.Parameters.Set(ParticleBaseKeys.ParticleSortBuffer, sortBuffer);
            effectMeshRender.Parameters.Set(ParticleBaseKeys.ParticleGlobalBuffer, globalBuffer);
            effectMeshRender.Parameters.Set(ParticleBaseKeys.ParticleCount, (uint)currentParticleCount);
        }

        /// <summary>
        /// Computes a bitonic sort on the sort buffer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// The sort buffer is a structure containing:
        /// struct SortData { uint ParticleIndex;  float DepthValue; };
        /// This methods reorders the sort buffer based on the depth value, from lowest to highest.
        /// </remarks>
        private void ComputeBitonicSort(ThreadContext context)
        {
            if (!EnableSorting || !isParticleRenderingEnabled)
                return;

            // TODO: handle progressive sorting when no dynamic emitter

            int particleCount = updatersCount;

            // Precalculates values from CapacityCount, MaximumDepthLevel and MaximumThreadPerGroup
            var depthLevelCount = (int)Math.Log(particleCount, 2);
            var optimLevelStart = (int)Math.Log(MaximumDepthLevel, 2);
            int blockSize = 1;
            int numberOfGroupForOptim = particleCount / MaximumThreadPerGroup;
            int numberOfGroups = numberOfGroupForOptim / 2;

            for (int i = 0; i < depthLevelCount; i++, blockSize *= 2)
            {
                // Create dispatch pass for bitonic step1
                effectMeshSort1Pass1.Parameters.Set(ParticleBaseKeys.ParticleStartIndex, (uint)blockSize);
                effectMeshSort1Pass1.Parameters.Set(ParticleBaseKeys.ParticleSortBuffer, sortBuffer);
                effectMeshSort1Pass1.Parameters.Set(ComputeShaderPlugin.ThreadGroupCount, new Int3(numberOfGroups, 1, 1));

                // Invoke Mesh Pass
                InvokeMeshPass(effectMeshSort1Pass1, context);

                // Setup multiple Pass2
                var subOffset = blockSize;

                for (int j = 0; j < i; j++, subOffset /= 2)
                {
                    // Use optimized group version if possible
                    int k = optimLevelStart - i + j;
                    if (k >= 0 && k < effectMeshBitonicSort2.Length)
                    {
                        var subMesh = effectMeshBitonicSort2[k];

                        subMesh.Parameters.Set(ParticleBaseKeys.ParticleSortBuffer, sortBuffer);
                        subMesh.Parameters.Set(ComputeShaderPlugin.ThreadGroupCount, new Int3(numberOfGroupForOptim, 1, 1));

                        InvokeMeshPass(subMesh, context);
                        break;
                    }

                    // Else use standard version
                    effectMeshSort1Pass2.Parameters.Set(ParticleBaseKeys.ParticleStartIndex, (uint)(subOffset / 2));
                    effectMeshSort1Pass2.Parameters.Set(ParticleBaseKeys.ParticleSortBuffer, sortBuffer);
                    effectMeshSort1Pass2.Parameters.Set(ComputeShaderPlugin.ThreadGroupCount, new Int3(numberOfGroups, 1, 1));

                    // When j > 0, we can don't need to reapply the effect pass
                    InvokeMeshPass(effectMeshSort1Pass2, context, j > 0);
                }
            }
        }

        private void UpdatersOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            updatersCount = ParticleUtils.CalculateMaximumPowerOf2Count(Updaters.Sum(particleUpdater => particleUpdater.Count));
        }

        private class ParticleUpdaterState : ParticleEmitterComponent
        {
            public ParticleEmitterComponent UserState;

            public Buffer AppendBuffer;

            public Buffer ConsumeBuffer;

            public RenderPass EffectPass;

            public EffectOld EffectUpdater;

            public EffectMesh MeshUpdater;

            public int StartIndex;

            public int StructureSize;


            /// <summary>
            /// Allocates the buffers.
            /// </summary>
            public void AllocateBuffers(GraphicsDevice graphicsDevice)
            {
                DisposeBuffers();
                AppendBuffer = Buffer.StructuredAppend.New(graphicsDevice, UserState.Count, StructureSize);
                ConsumeBuffer = Buffer.StructuredAppend.New(graphicsDevice, UserState.Count, StructureSize);
            }

            public void DisposeEffects()
            {
                if (EffectUpdater != null)
                {
                    EffectUpdater.Release();
                    EffectUpdater = null;
                }
            }

            public void DisposeBuffers()
            {
                if (AppendBuffer != null)
                {
                    AppendBuffer.Release();
                    AppendBuffer = null;
                }

                if (ConsumeBuffer != null)
                {
                    ConsumeBuffer.Release();
                    ConsumeBuffer = null;
                }
            }
        }

        public RenderTarget RenderTarget { get; set; }
    }
}