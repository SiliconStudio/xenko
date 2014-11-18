// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

using IServiceRegistry = SiliconStudio.Core.IServiceRegistry;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="Renderer"/> is responsible to prepare and render meshes for a specific pass.
    /// </summary>
    public class ModelRenderer : Renderer
    {
        private int meshPassSlot;

        private readonly FastList<EffectMesh> meshesToRender;
        private readonly EffectParameterUpdater updater;
        private readonly ParameterCollection[] parameterCollections;
        private readonly string effectName;

        private readonly SafeDelegateList<AcceptModelDelegate> acceptModels;

        private readonly SafeDelegateList<AcceptRenderModelDelegate> acceptRenderModels;

        private readonly SafeDelegateList<AcceptMeshForRenderingDelegate> acceptPrepareMeshForRenderings;

        private readonly SafeDelegateList<AcceptRenderMeshDelegate> acceptRenderMeshes;

        private readonly SafeDelegateList<UpdateMeshesDelegate> updateMeshes;

        private readonly SafeDelegateList<PreRenderDelegate> preRenders;

        private readonly SafeDelegateList<PostRenderDelegate> postRenders;

        private readonly SafeDelegateList<PreEffectUpdateDelegate> preEffectUpdates;

        private readonly SafeDelegateList<PostEffectUpdateDelegate> postEffectUpdates;

        /// <summary>
        /// An accept model callback to test whether a model will be handled by this instance.
        /// </summary>
        /// <param name="modelInstance">The model instance</param>
        /// <returns><c>true</c> if the model instance is going to be handled by this renderer, <c>false</c> otherwise.</returns>
        public delegate bool AcceptModelDelegate(IModelInstance modelInstance);

        public delegate bool AcceptMeshForRenderingDelegate(RenderModel renderModel, Mesh mesh);

        public delegate bool AcceptRenderMeshDelegate(RenderContext context, EffectMesh effectMesh);

        public delegate bool AcceptRenderModelDelegate(RenderModel renderModel);

        public delegate void UpdateMeshesDelegate(RenderContext context, FastList<EffectMesh> meshes);

        public delegate void PreRenderDelegate(RenderContext context);

        public delegate void PostRenderDelegate(RenderContext context);

        public delegate void PreEffectUpdateDelegate(RenderContext context, EffectMesh effectMesh);

        public delegate void PostEffectUpdateDelegate(RenderContext context, EffectMesh effectMesh);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRenderer"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="effectName">Name of the effect.</param>
        public ModelRenderer(IServiceRegistry services, string effectName) : base(services)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            this.effectName = effectName;
            DebugName = string.Format("ModelRenderer [{0}]", effectName);

            meshesToRender = new FastList<EffectMesh>();
            updater = new EffectParameterUpdater();
            parameterCollections = new ParameterCollection[5];

            acceptModels = new SafeDelegateList<AcceptModelDelegate>(this);
            acceptRenderModels = new SafeDelegateList<AcceptRenderModelDelegate>(this);
            acceptPrepareMeshForRenderings = new SafeDelegateList<AcceptMeshForRenderingDelegate>(this);
            acceptRenderMeshes = new SafeDelegateList<AcceptRenderMeshDelegate>(this);
            updateMeshes = new SafeDelegateList<UpdateMeshesDelegate>(this) { UpdateMeshesDefault };
            SortMeshes = DefaultSort;
            preRenders = new SafeDelegateList<PreRenderDelegate>(this);
            postRenders = new SafeDelegateList<PostRenderDelegate>(this);
            preEffectUpdates = new SafeDelegateList<PreEffectUpdateDelegate>(this);
            postEffectUpdates = new SafeDelegateList<PostEffectUpdateDelegate>(this);
        }

        public string EffectName
        {
            get
            {
                return effectName;
            }
        }

        public SafeDelegateList<AcceptModelDelegate> AcceptModel
        {
            get
            {
                return acceptModels;
            }
        }

        public SafeDelegateList<AcceptRenderModelDelegate> AcceptRenderModel
        {
            get
            {
                return acceptRenderModels;
            }
        }

        public SafeDelegateList<AcceptMeshForRenderingDelegate> AcceptPrepareMeshForRendering
        {
            get
            {
                return acceptPrepareMeshForRenderings;
            }
        }

        public SafeDelegateList<AcceptRenderMeshDelegate> AcceptRenderMesh
        {
            get
            {
                return acceptRenderMeshes;
            }
        }

        public SafeDelegateList<UpdateMeshesDelegate> UpdateMeshes
        {
            get
            {
                return updateMeshes;
            }
        }

        public UpdateMeshesDelegate SortMeshes { get; set; }

        public SafeDelegateList<PreRenderDelegate> PreRender
        {
            get
            {
                return preRenders;
            }
        }

        public SafeDelegateList<PostRenderDelegate> PostRender
        {
            get
            {
                return postRenders;
            }
        }

        public SafeDelegateList<PreEffectUpdateDelegate> PreEffectUpdate
        {
            get
            {
                return preEffectUpdates;
            }
        }

        public SafeDelegateList<PostEffectUpdateDelegate> PostEffectUpdate
        {
            get
            {
                return postEffectUpdates;
            }
        }

        public override void Load()
        {
            base.Load();

            var pipelineModelState = Pass.GetOrCreateModelRendererState();

            // Get the slot for the pass of this processor
            meshPassSlot = pipelineModelState.GetModelSlot(Pass);

            // Register callbacks used by the MeshProcessor
            pipelineModelState.AcceptModel += OnAcceptModel;
            pipelineModelState.PrepareRenderModel += PrepareModelForRendering;
            pipelineModelState.AcceptRenderModel += OnAcceptRenderModel;
        }

        public override void Unload()
        {
            base.Unload();

            var pipelineModelState = Pass.GetOrCreateModelRendererState();

            // Unregister callbacks
            pipelineModelState.AcceptModel -= OnAcceptModel;
            pipelineModelState.PrepareRenderModel -= PrepareModelForRendering;
            pipelineModelState.AcceptRenderModel -= OnAcceptRenderModel;
        }

        /// <summary>
        /// Draws the mesh stored in the current <see cref="RenderContext" />
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectMesh">The current effect mesh.</param>
        private void RenderMesh(RenderContext context, EffectMesh effectMesh)
        {
            // Retrieve effect parameters
            var currentPass = context.CurrentPass;
            var currentRenderData = effectMesh.RenderData;

            //using (Profiler.Begin(ProfilingKeys.PrepareMesh))
            {
                // Order of application of parameters:
                // - RenderPass.Parameters
                // - ModelComponent.Parameters
                // - EffectMesh.Parameters (originally copied from mesh parameters)
                // The order is based on the granularity level of each element and how shared it can be. Material is heavily shared, a model contains many meshes. An effectMesh is unique.
                // TODO: really copy mesh parameters into effectMesh instead of just referencing the meshDraw parameters.

                var modelComponent = effectMesh.ModelInstance;
                var hasMaterialParams = effectMesh.Mesh.Material != null && effectMesh.Mesh.Material.Parameters != null;
                var hasModelComponentParams = modelComponent != null && modelComponent.Parameters != null;
                if (hasMaterialParams)
                {
                    if (hasModelComponentParams)
                        effectMesh.Effect.Apply(currentPass.Parameters, effectMesh.Mesh.Material.Parameters, modelComponent.Parameters, effectMesh.Parameters, true);
                    else
                        effectMesh.Effect.Apply(currentPass.Parameters, effectMesh.Mesh.Material.Parameters, effectMesh.Parameters, true);
                }
                else if (hasModelComponentParams)
                    effectMesh.Effect.Apply(currentPass.Parameters, modelComponent.Parameters, effectMesh.Parameters, true);
                else
                    effectMesh.Effect.Apply(currentPass.Parameters, effectMesh.Parameters, true);
            }

            //using (Profiler.Begin(ProfilingKeys.RenderMesh))
            {
                if (currentRenderData != null)
                {
                    var graphicsDevice = context.GraphicsDevice;

                    graphicsDevice.SetVertexArrayObject(effectMesh.VertexArrayObject);

                    if (currentRenderData.IndexBuffer == null)
                    {
                        graphicsDevice.Draw(currentRenderData.PrimitiveType, currentRenderData.DrawCount, currentRenderData.StartLocation);
                    }
                    else
                    {
                        graphicsDevice.DrawIndexed(currentRenderData.PrimitiveType, currentRenderData.DrawCount, currentRenderData.StartLocation);
                    }
                }
            }
        }

        protected override void OnRendering(RenderContext context)
        {
            var state = Pass.GetModelRendererState();

            // Get all meshes from render models
            meshesToRender.Clear();
            foreach (var renderModel in state.RenderModels)
            {
                var meshes = renderModel.InternalMeshes[meshPassSlot];
                if (meshes != null)
                    meshesToRender.AddRange(meshes);
            }

            foreach (var effectMesh in meshesToRender)
            {
                if (EffectSystem.WasEffectRecompiled(effectMesh.Effect))
                    EffectMeshRefresh(effectMesh);
            }

            // Update meshes
            foreach (var updateMeshesToRender in updateMeshes)
            {
                updateMeshesToRender(context, meshesToRender);
            }

            // Sort meshes
            if (SortMeshes != null)
            {
                SortMeshes(context, meshesToRender);
            }

            // PreRender callbacks
            foreach (var preRender in preRenders)
            {
                preRender(context);
            }

            // TODO: separate update effect and render to tightly batch render calls vs 1 cache-friendly loop on meshToRender
            foreach (var mesh in meshesToRender)
            {
                // PreEffectUpdate callbacks
                foreach (var preEffectUpdate in preEffectUpdates)
                {
                    preEffectUpdate(context, mesh);
                }

                UpdateEffectMesh(mesh);

                // PostEffectUpdate callbacks
                foreach (var postEffectUpdate in postEffectUpdates)
                {
                    postEffectUpdate(context, mesh);
                }

                mesh.Render(context, mesh);
            }

            // PostRender callbacks
            foreach (var postRender in postRenders)
            {
                postRender(context);
            }
        }

        private void PrepareModelForRendering(RenderModel renderModel)
        {
            foreach (var mesh in renderModel.Model.Meshes)
            {
                if (acceptPrepareMeshForRenderings.Count > 0 && !OnAcceptPrepareMeshForRendering(renderModel, mesh))
                {
                    continue;
                }

                var effectMesh = new EffectMesh(null, mesh);
                CreateEffect(effectMesh, renderModel.Parameters);

                // Register mesh for rendering
                if (renderModel.InternalMeshes[meshPassSlot] == null)
                {
                    renderModel.InternalMeshes[meshPassSlot] = new List<EffectMesh>();
                }
                renderModel.InternalMeshes[meshPassSlot].Add(effectMesh);
            }
        }

        private void UpdateMeshesDefault(RenderContext context, FastList<EffectMesh> meshes)
        {
            for (var i = 0; i < meshes.Count; ++i)
            {
                var mesh = meshes[i];

                if (!mesh.Enabled || (context.ActiveLayers & meshes[i].Mesh.Layer) == RenderLayers.RenderLayerNone ||
                    (acceptRenderMeshes.Count > 0 && !OnAcceptRenderMesh(context, mesh)))
                {
                    meshes.SwapRemoveAt(i--);
                }
            }
        }

        private void DefaultSort(RenderContext context, FastList<EffectMesh> meshes)
        {
            // Sort based on ModelComponent.DrawOrder
            meshes.Sort(ModelComponentSorter.Default);
        }

        private bool OnAcceptRenderModel(RenderModel renderModel)
        {
            // NOTICE: We don't use Linq, as It would allocated objects and triggers GC
            foreach (var acceptRenderModel in AcceptRenderModel)
            {
                if (!acceptRenderModel(renderModel))
                {
                    return false;
                }
            }
            return true;
        }

        private bool OnAcceptModel(IModelInstance modelInstance)
        {
            // NOTICE: We don't use Linq, as It would allocated objects and triggers GC
            foreach (var test in acceptModels)
            {
                if (!test(modelInstance))
                {
                    return false;
                }
            }
            return true;
        }

        private bool OnAcceptPrepareMeshForRendering(RenderModel renderModel, Mesh mesh)
        {
            // NOTICE: Don't use Linq, as It would allocated objects and triggers GC
            foreach (var test in acceptPrepareMeshForRenderings)
            {
                if (!test(renderModel, mesh))
                {
                    return false;
                }
            }
            return true;
        }

        private bool OnAcceptRenderMesh(RenderContext context, EffectMesh effectMesh)
        {
            // NOTICE: Don't use Linq, as It would allocated objects and triggers GC
            foreach (var test in acceptRenderMeshes)
            {
                if (!test(context, effectMesh))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Create or update the Effect of the effect mesh.
        /// </summary>
        /// <param name="effectMesh">The effect mesh.</param>
        /// <param name="modelComponentParameters">The ModelComponent parameters.</param>
        protected void CreateEffect(EffectMesh effectMesh, ParameterCollection modelComponentParameters)
        {
            var mesh = effectMesh.Mesh;
            var compilerParameters = new CompilerParameters();

            // The same order as the one during compilation is used here
            // 1. Material
            // 2. ModelComponent
            // 3. Mesh

            if (mesh.Material != null)
            {
                foreach (var parameter in mesh.Material.Parameters.InternalValues)
                {
                    compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
                }
            }

            if (modelComponentParameters != null)
            {
                foreach (var parameter in modelComponentParameters.InternalValues)
                {
                    compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
                }
            }

            if (mesh.Parameters != null)
            {
                foreach (var parameter in mesh.Parameters.InternalValues)
                {
                    compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
                }
            }
            
            foreach (var parameter in GraphicsDevice.Parameters.InternalValues)
            {
                compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
            }

            // Compile shader
            // possible exception in LoadEffect
            var effect = EffectSystem.LoadEffect(effectName, compilerParameters);

            // update effect mesh
            UpdateEffectMeshEffect(effectMesh, effect, modelComponentParameters);
        }

        private void UpdateEffectMeshEffect(EffectMesh effectMesh, Effect effect, ParameterCollection modelComponentParameters)
        {
            if (effect == null)
            {
                return;
            }

            if (!ReferenceEquals(effect, effectMesh.Effect))
            {
                // Copy back parameters set on previous effect to new effect
                if (effectMesh.Effect != null)
                {
                    foreach (var parameter in effectMesh.Effect.Parameters.InternalValues)
                    {
                        effect.Parameters.SetObject(parameter.Key, parameter.Value.Object);
                    }
                }
                effectMesh.UpdaterDefinition = new EffectParameterUpdaterDefinition(effect);
                
                var mesh = effectMesh.Mesh;

                // Create EffectMesh and setup its draw data and rendering
                // TODO:FX should later be done inside EffectMesh or in a separate class
                // Note: this was previously done in RenderContext in previous system.
                effectMesh.Effect = effect;
                effectMesh.VertexArrayObject = VertexArrayObject.New(GraphicsDevice, effect.InputSignature, mesh.Draw.IndexBuffer, mesh.Draw.VertexBuffers);
                effectMesh.Render = RenderMesh;
                effectMesh.RenderData = mesh.Draw;
            }
            else
            {
                // Same effect than previous one

                effectMesh.UpdaterDefinition.UpdateCounter(effect.CompilationParameters);
            }

            UpdateLevels(effectMesh);
            updater.UpdateCounters(effectMesh.UpdaterDefinition);
        }

        private void EffectMeshRefresh(EffectMesh effectMesh)
        {
            var effect = effectMesh.Effect;
            if (effectMesh.Effect != null)
            {
                foreach (var parameter in effectMesh.Effect.Parameters.InternalValues)
                {
                    effect.Parameters.SetObject(parameter.Key, parameter.Value.Object);
                }
            }

            var mesh = effectMesh.Mesh;

            // Create EffectMesh and setup its draw data and rendering
            // TODO:FX should later be done inside EffectMesh or in a separate class
            // Note: this was previously done in RenderContext in previous system.
            effectMesh.VertexArrayObject = VertexArrayObject.New(GraphicsDevice, effect.InputSignature, mesh.Draw.IndexBuffer, mesh.Draw.VertexBuffers);
            effectMesh.Render = RenderMesh;
            effectMesh.RenderData = mesh.Draw;

            effectMesh.UpdaterDefinition = new EffectParameterUpdaterDefinition(effect);
            UpdateLevels(effectMesh);
        }

        private void UpdateEffectMesh(EffectMesh effectMesh)
        {
            if (effectMesh.ModelInstance == null)
                return;

            if (HasCollectionChanged(effectMesh))
            {
                CreateEffect(effectMesh, effectMesh.ModelInstance.Parameters);
            }
        }

        /// <summary>
        /// Checks if a collection has changed and the effect needs to be changed.
        /// </summary>
        /// <param name="effectMesh">The effect mesh.</param>
        /// <returns>True if the collection changed.</returns>
        private bool HasCollectionChanged(EffectMesh effectMesh)
        {
            PrepareUpdater(effectMesh);
            return updater.HasChanged(effectMesh.UpdaterDefinition);
        }

        /// <summary>
        /// Prepare the EffectParameterUpdater for the effect mesh.
        /// </summary>
        /// <param name="effectMesh">The effect mesh.</param>
        private void PrepareUpdater(EffectMesh effectMesh)
        {
            var mesh = effectMesh.Mesh;
            var collectionCount = 1;
            parameterCollections[0] = effectMesh.Effect.DefaultCompilationParameters;

            // The same order as the one during compilation is used here
            // 1. Material
            // 2. ModelComponent
            // 3. Mesh

            if (mesh.Material != null)
                parameterCollections[collectionCount++] = mesh.Material.Parameters;
            if (effectMesh.ModelInstance != null && effectMesh.ModelInstance.Parameters != null)
                parameterCollections[collectionCount++] = effectMesh.ModelInstance.Parameters;
            if (mesh.Parameters != null)
                parameterCollections[collectionCount++] = mesh.Parameters;

            parameterCollections[collectionCount++] = GraphicsDevice.Parameters;

            updater.Update(effectMesh.UpdaterDefinition, parameterCollections, collectionCount);
        }

        /// <summary>
        /// Get the levels of the parameters.
        /// </summary>
        /// <param name="effectMesh">The effect mesh.</param>
        /// <returns>A table of levels.</returns>
        private void UpdateLevels(EffectMesh effectMesh)
        {
            PrepareUpdater(effectMesh);
            updater.ComputeLevels(effectMesh.UpdaterDefinition);
        }

        /// <summary>
        /// A list to ensure that all delegates are not null.
        /// </summary>
        /// <typeparam name="T">A delegate</typeparam>
        public class SafeDelegateList<T> : ConstrainedList<T> where T : class
        {
            private const string ExceptionError = "The delegate added to the list cannot be null";
            private readonly ModelRenderer renderer;

            internal SafeDelegateList(ModelRenderer renderer)
                : base(Constraint, true, ExceptionError)
            {
                this.renderer = renderer;
            }

            public new ModelRenderer Add(T item)
            {
                base.Add(item);
                return renderer;
            }

            public new ModelRenderer Insert(int index, T item)
            {
                base.Insert(index, item);
                return renderer;
            }

            private static bool Constraint(ConstrainedList<T> constrainedList, T arg2)
            {
                return arg2 != null;
            }
        }

        #region Helper class

        private class ModelComponentSorter : IComparer<EffectMesh>
        {
            #region Constants and Fields

            public static readonly ModelComponentSorter Default = new ModelComponentSorter();

            #endregion

            public int Compare(EffectMesh x, EffectMesh y)
            {
                var xModelComponent = x.ModelInstance;
                var yModelComponent = y.ModelInstance;

                // Ignore if no associated mesh component
                if (xModelComponent == null || yModelComponent == null)
                    return 0;

                if (x.IsTransparent && !y.IsTransparent)
                    return 1;

                if (!x.IsTransparent && y.IsTransparent)
                    return -1;

                // Use draw order
                return Math.Sign(xModelComponent.DrawOrder - yModelComponent.DrawOrder);
            }
        }

        #endregion
    }
}