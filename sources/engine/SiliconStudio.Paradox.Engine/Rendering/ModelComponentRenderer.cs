// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering
{

    public class ModelComponentRendererCallback
    {
        public delegate void UpdateMeshesDelegate(RenderContext context, FastList<RenderMesh> meshes);

        public delegate void PreRenderMeshDelegate(RenderContext context, RenderMesh renderMesh);

        public delegate void PostEffectUpdateDelegate(RenderContext context, RenderMesh renderMesh);

        public delegate bool PreRenderModelDelegate(RenderContext context, RenderModel renderModel);

        public PreRenderModelDelegate PreRenderModel;

        public UpdateMeshesDelegate UpdateMeshes;

        public PreRenderMeshDelegate PreRenderMesh;
    }

    /// <summary>
    /// This <see cref="EntityComponentRendererBase"/> is responsible to prepare and render meshes for a specific pass.
    /// </summary>
    public class ModelComponentRenderer : EntityComponentRendererBase
    {
        private readonly static PropertyKey<ModelComponentRenderer> Current = new PropertyKey<ModelComponentRenderer>("ModelComponentRenderer.Current", typeof(ModelComponentRenderer));

        private int modelRenderSlot;

        private readonly FastList<RenderMesh> meshesToRender;

        private DynamicEffectCompiler dynamicEffectCompiler;
        private string effectName;
        
        public override bool SupportPicking { get { return true; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelComponentRenderer"/> class.
        /// </summary>
        public ModelComponentRenderer() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelComponentRenderer" /> class.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <exception cref="System.ArgumentNullException">effectName</exception>
        public ModelComponentRenderer(string effectName)
        {
            if (effectName != null)
            {
                EffectName = effectName;
            }

            meshesToRender = new FastList<RenderMesh>();

            modelRenderSlot = -1;

            Callbacks = new ModelComponentRendererCallback();
            CustomRenderModelList = new List<RenderModel>();
        }

        /// <summary>
        /// Gets or sets the name of the effect.
        /// </summary>
        /// <value>The name of the effect.</value>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.InvalidOperationException">Cannot change effect name after first initialize</exception>
        public string EffectName
        {
            get
            {
                return effectName;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (dynamicEffectCompiler != null) throw new InvalidOperationException("Cannot change effect name after first initialize");

                effectName = value;
                Name = string.Format("ModelRenderer [{0}]", effectName);
            }
        }

        /// <summary>
        /// Gets or sets the callbacks.
        /// </summary>
        /// <value>The callbacks.</value>
        public ModelComponentRendererCallback Callbacks { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use custom render model list].
        /// </summary>
        /// <value><c>true</c> if [use custom render model list]; otherwise, <c>false</c>.</value>
        public bool UseCustomRenderModelList { get; set; }

        /// <summary>
        /// Gets the custom render model list to be used instead of the model processor list
        /// </summary>
        /// <value>The custom render model list.</value>
        public List<RenderModel> CustomRenderModelList { get; private set; }

        /// <summary>
        /// Gets or sets the state of the rasterizer to overrides the default one.
        /// </summary>
        /// <value>The state of the rasterizer.</value>
        public RasterizerState RasterizerState { get; set; }

        public DynamicEffectCompiler DynamicEffectCompiler
        {
            get
            {
                return dynamicEffectCompiler;
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            if (effectName != null)
            {
                dynamicEffectCompiler = new DynamicEffectCompiler(Services, effectName);
            }
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            // If EffectName is not setup, exit
            if (effectName == null)
            {
                return;
            }

            if (dynamicEffectCompiler == null)
            {
                dynamicEffectCompiler = new DynamicEffectCompiler(Services, effectName);                
            }

            // If we don't have yet a render slot, create a new one
            if (modelRenderSlot < 0)
            {
                var pipelineModelState = GetOrCreateModelRendererState(context);

                // Allocate (or reuse) a slot for the pass of this processor
                // Note: The slot is passed as out, so that when ModelRendererState.ModelSlotAdded callback is fired,
                // ModelRenderer.modelRenderSlot is valid (it might call PrepareModelForRendering recursively).
                modelRenderSlot = pipelineModelState.AllocateModelSlot(EffectName);
            }

            if (UseCustomRenderModelList)
            {
                PrepareModels(context, CustomRenderModelList, opaqueList, transparentList);
            }
            else
            {
                // Get all meshes from the render model processor
                var modelProcessor = SceneInstance.GetProcessor<ModelProcessor>();
                foreach (var renderModelGroup in modelProcessor.ModelGroups)
                {
                    // Perform culling on group and accept
                    if (!CurrentCullingMask.Contains(renderModelGroup.Group))
                    {
                        continue;
                    }

                    PrepareModels(context, renderModelGroup, opaqueList, transparentList);
                }
            }
        }

        private void PrepareModels(RenderContext context, List<RenderModel> renderModels, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            // If no camera, early exit
            var camera = context.GetCurrentCamera();
            if (camera == null)
            {
                return;
            }

            var viewProjectionMatrix = camera.ViewProjectionMatrix;
            var preRenderModel = Callbacks.PreRenderModel;

            var sceneCameraRenderer = context.Tags.Get(SceneCameraRenderer.Current);
            var cullingMode = sceneCameraRenderer != null ? sceneCameraRenderer.CullingMode : CullingMode.None;
            var frustum = new BoundingFrustum(ref viewProjectionMatrix);

            var cameraRenderMode = sceneCameraRenderer != null ? sceneCameraRenderer.Mode : null;

            foreach (var renderModel in renderModels)
            {
                // If Model is null, then skip it
                if (renderModel.Model == null)
                {
                    continue;
                }

                if (preRenderModel != null)
                {
                    if (!preRenderModel(context, renderModel))
                    {
                        continue;
                    }
                }

                // Always prepare the slot for the render meshes even if they are not used.
                EnsureRenderMeshes(renderModel);

                var meshes = PrepareModelForRendering(context, renderModel);

                foreach (var renderMesh in meshes)
                {
                    if (!renderMesh.Enabled)
                    {
                        continue;
                    }

                    var worldMatrix = renderMesh.WorldMatrix;

                    // Perform frustum culling
                    if (cullingMode == CullingMode.Frustum)
                    {
                        // Always render meshes with unspecified bounds
                        // TODO: This should not be necessary. Add proper bounding boxes to gizmos etc.
                        var boundingBox = renderMesh.Mesh.BoundingBox;
                        if (boundingBox.Extent == Vector3.Zero)
                        {
                            // Fast AABB transform: http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
                            // Compute transformed AABB (by world)
                            var boundingBoxExt = new BoundingBoxExt(boundingBox);
                            boundingBoxExt.Transform(worldMatrix);

                            if (!frustum.Contains(ref boundingBoxExt))
                                continue;
                        }
                    }

                    // Project the position
                    // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                    var worldPosition = new Vector4(worldMatrix.TranslationVector, 1.0f);
                    Vector4 projectedPosition;
                    Vector4.Transform(ref worldPosition, ref viewProjectionMatrix, out projectedPosition);
                    var projectedZ = projectedPosition.Z / projectedPosition.W;

                    // TODO: Should this be set somewhere else?
                    var rasterizerState = cameraRenderMode != null ? cameraRenderMode.GetDefaultRasterizerState(renderMesh.RenderModel.IsGeometryInverted) : null;
                    renderMesh.RasterizerState = RasterizerState ?? rasterizerState;

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

            // Slow path there is a callback
            if (Callbacks.UpdateMeshes != null)
            {
                Callbacks.UpdateMeshes(context, meshesToRender);
            }

            // Fetch callback on PreRenderGroup
            var preRenderMesh = Callbacks.PreRenderMesh;

            foreach (var mesh in meshesToRender)
            {
                // If the EntityGroup is changing, call the callback to allow to plug specific parameters for this group
                if (preRenderMesh != null)
                {
                    preRenderMesh(context, mesh);
                }

                // Update Effect and mesh
                UpdateEffect(context, mesh, context.Parameters);

                // Draw the mesh
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

        /// <summary>
        /// Gets the attached <see cref="ModelComponentRenderer"/> from the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>ModelComponentRenderer.</returns>
        /// <exception cref="System.ArgumentNullException">component</exception>
        public static ModelComponentRenderer GetAttached(ComponentBase component)
        {
            if (component == null) throw new ArgumentNullException("component");
            return component.Tags.Get(Current);
        }

        /// <summary>
        /// Attaches a <see cref="ModelComponentRenderer"/> to the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="renderer">The renderer.</param>
        /// <exception cref="System.ArgumentNullException">component</exception>
        public static void Attach(ComponentBase component, ModelComponentRenderer renderer)
        {
            if (component == null) throw new ArgumentNullException("component");
            component.Tags.Set(Current, renderer);
        }

        private void EnsureRenderMeshes(RenderModel renderModel)
        {
            var renderMeshes = renderModel.RenderMeshesList;
            if (modelRenderSlot < renderMeshes.Count)
            {
                return;
            }
            for (int i = renderMeshes.Count; i <= modelRenderSlot; i++)
            {
                renderMeshes.Add(null);
            }
        }

        private List<RenderMesh> PrepareModelForRendering(RenderContext context, RenderModel renderModel)
        {
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
                    //UpdateEffect(context, renderMesh, null);

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