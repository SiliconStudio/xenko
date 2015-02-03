// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;

using IServiceRegistry = SiliconStudio.Core.IServiceRegistry;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="Renderer"/> is responsible to prepare and render meshes for a specific pass.
    /// </summary>
    public class ModelRenderer : Renderer
    {
        private int meshPassSlot;

        private readonly FastList<RenderMesh> meshesToRender;

        private readonly DynamicEffectCompiler dynamicEffectCompiler;
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

        public delegate bool AcceptRenderMeshDelegate(RenderContext context, RenderMesh renderMesh);

        public delegate bool AcceptRenderModelDelegate(RenderModel renderModel);

        public delegate void UpdateMeshesDelegate(RenderContext context, FastList<RenderMesh> meshes);

        public delegate void PreRenderDelegate(RenderContext context);

        public delegate void PostRenderDelegate(RenderContext context);

        public delegate void PreEffectUpdateDelegate(RenderContext context, RenderMesh renderMesh);

        public delegate void PostEffectUpdateDelegate(RenderContext context, RenderMesh renderMesh);

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

            dynamicEffectCompiler = new DynamicEffectCompiler(services, effectName);

            meshesToRender = new FastList<RenderMesh>();

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
            meshPassSlot = pipelineModelState.GetModelSlot(Pass, EffectName);

            // Register callbacks used by the MeshProcessor
            pipelineModelState.AcceptModel += OnAcceptModel;
            pipelineModelState.PrepareRenderModel += PrepareModelForRendering;
        }

        public override void Unload()
        {
            base.Unload();

            var pipelineModelState = Pass.GetOrCreateModelRendererState();

            // Unregister callbacks
            pipelineModelState.AcceptModel -= OnAcceptModel;
            pipelineModelState.PrepareRenderModel -= PrepareModelForRendering;
        }

        protected override void OnRendering(RenderContext context)
        {
            var state = Pass.GetModelRendererState();

            // Get all meshes from render models
            meshesToRender.Clear();
            foreach (var renderModel in state.RenderModels)
            {
                if (!OnAcceptRenderModel(renderModel))
                {
                    continue;
                }

                var meshes = renderModel.RenderMeshes[meshPassSlot];
                if (meshes != null)
                    meshesToRender.AddRange(meshes);
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

                // Update Effect and mesh
                UpdateEffect(mesh);

                // PostEffectUpdate callbacks
                foreach (var postEffectUpdate in postEffectUpdates)
                {
                    postEffectUpdate(context, mesh);
                }

                mesh.Draw(context);
            }

            // PostRender callbacks
            foreach (var postRender in postRenders)
            {
                postRender(context);
            }
        }

        private void PrepareModelForRendering(RenderModel renderModel)
        {
            // TODO: this is obviously wrong since a pipeline can have several ModelRenderer with the same effect.
            // In that case, a Mesh may be added several times to the list and as a result rendered several time in a single ModelRenderer.
            // We keep it that way for now since we only have two ModelRenderer with the same effect in the deferrent pipeline (splitting between opaque and transparent objects) and their acceptance tests are exclusive.

            // Create the list of RenderMesh objects
            var renderMeshes = renderModel.RenderMeshes[meshPassSlot];
            if (renderMeshes == null)
            {
                renderMeshes = new List<RenderMesh>();
                renderModel.RenderMeshes[meshPassSlot] = renderMeshes;
            }

            foreach (var mesh in renderModel.Model.Meshes)
            {
                if (acceptPrepareMeshForRenderings.Count > 0 && !OnAcceptPrepareMeshForRendering(renderModel, mesh))
                {
                    continue;
                }

                var renderMesh = new RenderMesh(renderModel, mesh);
                UpdateEffect(renderMesh);

                // Register mesh for rendering
                renderMeshes.Add(renderMesh);
            }
        }

        private void UpdateMeshesDefault(RenderContext context, FastList<RenderMesh> meshes)
        {
            for (var i = 0; i < meshes.Count; ++i)
            {
                var mesh = meshes[i];

                if (!mesh.Enabled || (acceptRenderMeshes.Count > 0 && !OnAcceptRenderMesh(context, mesh)))
                {
                    meshes.SwapRemoveAt(i--);
                }
            }
        }

        private void DefaultSort(RenderContext context, FastList<RenderMesh> meshes)
        {
            // Sort based on ModelComponent.DrawOrder
            DefaultRenderMeshSorter.Default.Sort(meshes);
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

        private bool OnAcceptRenderMesh(RenderContext context, RenderMesh renderMesh)
        {
            // NOTICE: Don't use Linq, as It would allocated objects and triggers GC
            foreach (var test in acceptRenderMeshes)
            {
                if (!test(context, renderMesh))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Create or update the Effect of the effect mesh.
        /// </summary>
        protected void UpdateEffect(RenderMesh renderMesh)
        {
            if (dynamicEffectCompiler.Update(renderMesh))
            {
                renderMesh.Initialize(GraphicsDevice);
            }
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

        /// <summary>
        /// Sorts meshes, using material transparency and then <see cref="Engine.ModelComponent.DrawOrder"/>
        /// </summary>
        private class DefaultRenderMeshSorter : RenderMeshSorter
        {
            public static readonly RenderMeshSorter Default = new DefaultRenderMeshSorter();

            /// <inheritdoc />
            public override int GenerateSortKey(RenderMesh renderMesh)
            {
                int result = 0;

                // First, sort using material transparency
                var material = renderMesh.Mesh.Material;
                if (material != null && material.Parameters.Get(MaterialParameters.UseTransparent))
                    result += 0x10000000;

                // Then sort using DrawOrder
                var modelComponent = renderMesh.RenderModel.ModelInstance;
                if (modelComponent != null)
                {
                    var drawOrder = modelComponent.DrawOrder;
                    if (((uint)drawOrder & 0xF0000000) != 0)
                        throw new InvalidOperationException("ModelComponent.DrawOrder is too big for this mesh sorter.");
                    result += drawOrder;
                }

                return result;
            }
        }

        #endregion
    }
}