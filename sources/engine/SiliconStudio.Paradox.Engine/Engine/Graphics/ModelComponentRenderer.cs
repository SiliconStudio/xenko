// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="EntityComponentRendererBase"/> is responsible to prepare and render meshes for a specific pass.
    /// </summary>
    public class ModelComponentRenderer : EntityComponentRendererBase
    {
        private int modelRenderSlot;

        private ModelProcessor modelProcessor;

        private readonly FastList<RenderMesh> meshesToRender;

        private DynamicEffectCompiler dynamicEffectCompiler;
        private readonly string effectName;

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
        /// Initializes a new instance of the <see cref="ModelComponentRenderer" /> class.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <exception cref="System.ArgumentNullException">effectName</exception>
        public ModelComponentRenderer(string effectName)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");

            this.effectName = effectName;
            DebugName = string.Format("ModelRenderer [{0}]", effectName);

            meshesToRender = new FastList<RenderMesh>();

            SortMeshes = DefaultSort;

            CullingMask = EntityGroup.All;
            modelRenderSlot = -1;
        }

        public string EffectName
        {
            get
            {
                return effectName;
            }
        }

        public AcceptRenderModelDelegate AcceptRenderModel { get; set; }

        public AcceptRenderMeshDelegate AcceptRenderMesh { get; set; }

        public UpdateMeshesDelegate UpdateMeshes { get; set; }

        public PreRenderDelegate PreRender { get; set; }

        public PostRenderDelegate PostRender { get; set; }

        public PreEffectUpdateDelegate PreEffectUpdate { get; set; }

        public PostEffectUpdateDelegate PostEffectUpdate { get; set; }

        public UpdateMeshesDelegate SortMeshes { get; set; }

        public override void Load(RenderContext context)
        {
            base.Load(context);

            dynamicEffectCompiler = new DynamicEffectCompiler(Services, effectName);
            modelProcessor = EntityManager.GetProcessor<ModelProcessor>();
        }

        public override void Unload()
        {
            if (modelRenderSlot < 0)
            {
                var pipelineModelState = GetOrCreateModelRendererState(Context);

                // Release the slot (note: if shared, it will wait for all its usage to be released)
                pipelineModelState.ReleaseModelSlot(modelRenderSlot);

                // TODO: Remove RenderMeshes
            }

            base.Unload();
        }

        public EntityGroup CullingMask { get; set; }

        protected override void DrawCore(RenderContext context)
        {
            // If we don't have yet a render slot, create a new one
            if (modelRenderSlot < 0)
            {
                var pipelineModelState = GetOrCreateModelRendererState(context);

                // Allocate (or reuse) a slot for the pass of this processor
                // Note: The slot is passed as out, so that when ModelRendererState.ModelSlotAdded callback is fired,
                // ModelRenderer.modelRenderSlot is valid (it might call PrepareModelForRendering recursively).
                modelRenderSlot = pipelineModelState.AllocateModelSlot(EffectName);
            }

            // Get all meshes from render models
            meshesToRender.Clear();
            foreach (var renderModel in modelProcessor.Models)
            {
                // Always prepare the slot for the render meshes even if they are not used.
                EnsureRenderMeshes(renderModel);

                // Perform culling on group and accept
                if ((renderModel.Group & CullingMask) == 0 || (AcceptRenderModel != null && !AcceptRenderModel(renderModel)))
                {
                    continue;
                }

                var meshes = PrepareModelForRendering(context, renderModel);
                meshesToRender.AddRange(meshes);
            }

            // Update meshes
            UpdateMeshesDefault(context, meshesToRender);
            if (UpdateMeshes != null)
            {
                UpdateMeshes(context, meshesToRender);
            }

            // Sort meshes
            if (SortMeshes != null)
            {
                SortMeshes(context, meshesToRender);
            }

            // TODO: separate update effect and render to tightly batch render calls vs 1 cache-friendly loop on meshToRender
            foreach (var mesh in meshesToRender)
            {
                // Update Effect and mesh
                UpdateEffect(context, mesh, context.Parameters);

                mesh.Draw(context);
            }
        }

        private void EnsureRenderMeshes(RenderModel renderModel)
        {
            var renderMeshes = renderModel.RenderMeshes;
            if (modelRenderSlot < renderMeshes.Count)
            {
                return;
            }
            for (int i = 0; i < (modelRenderSlot - renderMeshes.Count + 1); i++)
            {
                renderMeshes.Add(null);
            }
        }

        private List<RenderMesh> PrepareModelForRendering(RenderContext context, RenderModel renderModel)
        {
            // TODO: this is obviously wrong since a pipeline can have several ModelRenderer with the same effect.
            // In that case, a Mesh may be added several times to the list and as a result rendered several time in a single ModelRenderer.
            // We keep it that way for now since we only have two ModelRenderer with the same effect in the deferrent pipeline (splitting between opaque and transparent objects) and their acceptance tests are exclusive.

            // Create the list of RenderMesh objects
            var renderMeshes = renderModel.RenderMeshes[modelRenderSlot];
            if (renderMeshes == null)
            {
                renderMeshes = new List<RenderMesh>();
                renderModel.RenderMeshes[modelRenderSlot] = renderMeshes;

                foreach (var mesh in renderModel.ModelComponent.Model.Meshes)
                {
                    var renderMesh = new RenderMesh(renderModel, mesh);
                    UpdateEffect(context, renderMesh, null);

                    // Register mesh for rendering
                    renderMeshes.Add(renderMesh);
                }
            }
            return renderMeshes;
        }

        private void UpdateMeshesDefault(RenderContext context, FastList<RenderMesh> meshes)
        {
            for (var i = 0; i < meshes.Count; ++i)
            {
                var mesh = meshes[i];

                if (!mesh.Enabled)
                {
                    meshes.SwapRemoveAt(i--);
                    continue;
                }

                mesh.UpdateMaterial();
            }
        }

        private void DefaultSort(RenderContext context, FastList<RenderMesh> meshes)
        {
            // Sort based on ModelComponent.DrawOrder
            meshes.Sort(ModelComponentSorter.Default);
        }


        /// <summary>
        /// Create or update the Effect of the effect mesh.
        /// </summary>
        protected void UpdateEffect(RenderContext context, RenderMesh renderMesh, ParameterCollection passParameters)
        {
            if (dynamicEffectCompiler.Update(renderMesh, passParameters))
            {
                renderMesh.Initialize(context.GraphicsDevice);
            }
        }

        internal ModelRendererState GetOrCreateModelRendererState(RenderContext context, bool createMeshStateIfNotFound = true)
        {
            var pipelineState = EntityManager.Tags.Get(ModelRendererState.Key);
            if (createMeshStateIfNotFound && pipelineState == null)
            {
                pipelineState = new ModelRendererState();
                EntityManager.Tags.Set(ModelRendererState.Key, pipelineState);
            }
            return pipelineState;
        }

        /// <summary>
        /// A list to ensure that all delegates are not null.
        /// </summary>
        /// <typeparam name="T">A delegate</typeparam>
        public class SafeDelegateList<T> : ConstrainedList<T> where T : class
        {
            private const string ExceptionError = "The delegate added to the list cannot be null";
            private readonly ModelComponentRenderer renderer;

            internal SafeDelegateList(ModelComponentRenderer renderer)
                : base(Constraint, true, ExceptionError)
            {
                this.renderer = renderer;
            }

            public new ModelComponentRenderer Add(T item)
            {
                base.Add(item);
                return renderer;
            }

            public new ModelComponentRenderer Insert(int index, T item)
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

        private class ModelComponentSorter : IComparer<RenderMesh>
        {
            #region Constants and Fields

            public static readonly ModelComponentSorter Default = new ModelComponentSorter();

            #endregion

            public int Compare(RenderMesh left, RenderMesh right)
            {
                var xModelComponent = left.RenderModel.ModelComponent;
                var yModelComponent = right.RenderModel.ModelComponent;

                // Ignore if no associated mesh component
                if (xModelComponent == null || yModelComponent == null)
                    return 0;

                // TODO: Add a kind of associated data to an effect mesh to speed up this test?
                var leftMaterial = left.Material;
                var isLeftTransparent = (leftMaterial != null && leftMaterial.Parameters.Get(MaterialKeys.UseTransparent));

                var rightMaterial = right.Material;
                var isRightTransparent = (rightMaterial != null && rightMaterial.Parameters.Get(MaterialKeys.UseTransparent));

                if (isLeftTransparent && !isRightTransparent)
                    return 1;

                if (!isLeftTransparent && isRightTransparent)
                    return -1;

                // Use draw order
                return Math.Sign(xModelComponent.DrawOrder - yModelComponent.DrawOrder);
            }
        }

        #endregion
    }
}