// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
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
        
        public override bool SupportPicking { get { return true; } }

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
            Name = string.Format("ModelRenderer [{0}]", effectName);

            meshesToRender = new FastList<RenderMesh>();

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

        public DynamicEffectCompiler DynamicEffectCompiler
        {
            get
            {
                return dynamicEffectCompiler;
            }
        }

        public UpdateMeshesDelegate UpdateMeshes { get; set; }

        public PreRenderDelegate PreRender { get; set; }

        public PostRenderDelegate PostRender { get; set; }

        public PreEffectUpdateDelegate PreEffectUpdate { get; set; }

        public PostEffectUpdateDelegate PostEffectUpdate { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            dynamicEffectCompiler = new DynamicEffectCompiler(Services, effectName);
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            modelProcessor = SceneInstance.GetProcessor<ModelProcessor>();

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
            foreach (var renderModel in modelProcessor.Models)
            {
                // Always prepare the slot for the render meshes even if they are not used.
                EnsureRenderMeshes(renderModel);

                // Perform culling on group and accept
                if ((renderModel.Group & CurrentCullingMask) == 0)
                {
                    continue;
                }

                var meshes = PrepareModelForRendering(context, renderModel);

                foreach (var renderMesh in meshes)
                {
                    if (!renderMesh.Enabled)
                    {
                        continue;
                    }

                    // Project the position
                    // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                    var worldPosition = new Vector4(renderMesh.Parameters.Get(TransformationKeys.World).TranslationVector, 1.0f);
                    Vector4 projectedPosition;
                    Vector4.Transform(ref worldPosition, ref context.ViewProjectionMatrix, out projectedPosition);
                    var projectedZ = projectedPosition.Z / projectedPosition.W;

                    renderMesh.UpdateMaterial();
                    var list = renderMesh.HasTransparency ? transparentList : opaqueList;
                    list.Add(new RenderItem(this, renderMesh, projectedZ));
                }
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItemList, int fromIndex, int toIndex)
        {
            // Get all meshes from render models
            meshesToRender.Clear();
            for(int i = fromIndex; i <= toIndex; i++)
            {
                meshesToRender.Add((RenderMesh)renderItemList[i].DrawContext);
            }

            // Update meshes
            if (UpdateMeshes != null)
            {
                UpdateMeshes(context, meshesToRender);
            }

            // TODO: separate update effect and render to tightly batch render calls vs 1 cache-friendly loop on meshToRender
            foreach (var mesh in meshesToRender)
            {
                // Update Effect and mesh
                UpdateEffect(context, mesh, context.Parameters);

                mesh.Draw(context);
            }
        }

        protected override void Unload()
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

        private void EnsureRenderMeshes(RenderModel renderModel)
        {
            var renderMeshes = renderModel.RenderMeshesList;
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
            var renderMeshes = renderModel.RenderMeshesList[modelRenderSlot];
            var modelMeshes = renderModel.ModelComponent.Model.Meshes;

            // If render mesh is new or the model changed, generate new render mesh
            if (renderMeshes == null || (renderMeshes.Count == 00 && modelMeshes.Count > 0))
            {
                if (renderMeshes == null)
                {
                    renderMeshes = new RenderMeshCollection();
                    renderModel.RenderMeshesList[modelRenderSlot] = renderMeshes;
                }

                foreach (var mesh in modelMeshes)
                {
                    var renderMesh = new RenderMesh(renderModel, mesh);
                    UpdateEffect(context, renderMesh, null);

                    // Register mesh for rendering
                    renderMeshes.Add(renderMesh);
                }
            }

            // Update RenderModel transform
            if (!renderMeshes.TransformUpdated)
            {
                // Update the model hierarchy
                var modelViewHierarchy = renderModel.ModelComponent.ModelViewHierarchy;

                modelViewHierarchy.UpdateToRenderModel(renderModel, modelRenderSlot);

                // Upload skinning blend matrices
                MeshSkinningUpdater.Update(modelViewHierarchy, renderModel, modelRenderSlot);

                renderMeshes.TransformUpdated = true;
            }

            return renderMeshes;
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
            var pipelineState = SceneInstance.Tags.Get(ModelRendererState.Key);
            if (createMeshStateIfNotFound && pipelineState == null)
            {
                pipelineState = new ModelRendererState();
                SceneInstance.Tags.Set(ModelRendererState.Key, pipelineState);
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
                var isLeftTransparent = (leftMaterial != null && leftMaterial.HasTransparency);

                var rightMaterial = right.Material;
                var isRightTransparent = (rightMaterial != null && rightMaterial.HasTransparency);

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