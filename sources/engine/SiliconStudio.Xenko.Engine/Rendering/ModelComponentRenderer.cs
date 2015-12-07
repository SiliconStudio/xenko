// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Rendering
{

    public class ModelComponentRendererCallback
    {
        public delegate void UpdateMeshesDelegate(RenderContext context, ref FastListStruct<RenderMesh> meshes);

        public delegate void PreRenderMeshDelegate(RenderContext context, RenderMesh renderMesh);

        public delegate void PostEffectUpdateDelegate(RenderContext context, RenderMesh renderMesh);

        public delegate bool PreRenderModelDelegate(RenderContext context, RenderModel renderModel);

        public PreRenderModelDelegate PreRenderModel;

        public UpdateMeshesDelegate UpdateMeshes;

        public PreRenderMeshDelegate PreRenderMesh;
    }

    /// <summary>
    /// Renders a collection of <see cref="RenderModel"/>.
    /// </summary>
    public class ModelComponentRenderer : EntityComponentRendererBase
    {
        private readonly static Logger Log = GlobalLogger.GetLogger("ModelComponentRenderer");
        private static readonly PropertyKey<RenderModelEffectSlotManager> RenderModelManagerKey = new PropertyKey<RenderModelEffectSlotManager>("ModelProcessor.RenderModelManagerKey", typeof(ModelComponentRenderer));
        private static readonly PropertyKey<ModelComponentRenderer> Current = new PropertyKey<ModelComponentRenderer>("ModelComponentRenderer.Current", typeof(ModelComponentRenderer));

        private int modelRenderSlot;

        private FastListStruct<RenderMesh> meshesToRender;

        private readonly List<RenderModelCollection> renderModelCollections = new List<RenderModelCollection>();

        private DynamicEffectCompiler dynamicEffectCompiler;
        private string effectName;

        private MeshSkinningUpdater skinningUpdater;

        private RenderModelEffectSlotManager slotManager;

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
            Callbacks = new ModelComponentRendererCallback();
            skinningUpdater = new MeshSkinningUpdater(256);
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
        /// View-Projection Matrix used for culling and calculating the z depth of the meshes to render.
        /// </summary>
        public Matrix ViewProjectionMatrix;

        /// <summary>
        /// Gets or sets the list of <see cref="RenderModel"/> to be used for rendering. If null, the list from <see cref="ModelProcessor"/> is used.
        /// </summary>
        /// <value>The custom render model list.</value>
        public List<RenderModel> RenderModels { get; set; }

        /// <summary>
        /// Gets or sets the state of the rasterizer to overrides the default one.
        /// </summary>
        /// <value>The state of the rasterizer.</value>
        public RasterizerState RasterizerState { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the rasterizer state set on this instance is overriding any rasterizer states defines at the material level.
        /// </summary>
        public bool ForceRasterizer { get; set; }

        /// <summary>
        /// Gets or sets the rasterizer state used for meshes with an inverted geometry. If not set, use the <see cref="RasterizerState"/>
        /// </summary>
        /// <value>The rasterizer state for inverted geometry.</value>
        public RasterizerState RasterizerStateForInvertedGeometry { get; set; }

        /// <summary>
        /// Allows to override the culling mode. If null, takes the culling mode from the current <see cref="SceneCameraRenderer"/>
        /// </summary>
        /// <value>The culling mode override.</value>
        public CameraCullingMode? CullingModeOverride { get; set; }

        /// <summary>
        /// Gets the dynamic effect compiler created by this instance. This value may be null if <see cref="EffectName"/> is null and the renderer hasn't been called.
        /// </summary>
        /// <value>The dynamic effect compiler.</value>
        public DynamicEffectCompiler DynamicEffectCompiler
        {
            get
            {
                return dynamicEffectCompiler;
            }
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
        
        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Create a slot manager
            var sceneInstance = SceneInstance.GetCurrent(Context);
            if (sceneInstance == null)
            {
                // If we don't have a scene instance (unlikely, but possible)
                slotManager = new RenderModelEffectSlotManager();
            }
            else
            {
                // If we have a scene instance, we can store our state there, as we expect to share render models 
                slotManager = sceneInstance.Tags.Get(RenderModelManagerKey);
                if (slotManager == null)
                {
                    slotManager = new RenderModelEffectSlotManager();
                    sceneInstance.Tags.Set(RenderModelManagerKey, slotManager);
                }
            }

            if (dynamicEffectCompiler == null)
            {
                dynamicEffectCompiler = new DynamicEffectCompiler(Services, effectName);

                // Allocate (or reuse) a slot for the pass of this processor
                // Note: The slot is passed as out, so that when ModelRendererState.ModelSlotAdded callback is fired,
                // ModelRenderer.modelRenderSlot is valid (it might call PrepareRenderMeshes recursively).
                modelRenderSlot = slotManager.AllocateSlot(EffectName);
            }
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            // If there is a list of models to render, use this list directly
            if (RenderModels != null)
            {
                PrepareModels(context, RenderModels, opaqueList, transparentList);
            }
            else
            {
                // Otherwise, use the models from the ModelProcessor
                var modelProcessor = SceneInstance.GetProcessor<ModelProcessor>();
                renderModelCollections.Clear();
                modelProcessor.QueryModelGroupsByMask(CurrentCullingMask, renderModelCollections);
                foreach (var renderModelGroup in renderModelCollections)
                {
                    PrepareModels(context, renderModelGroup, opaqueList, transparentList);
                }
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItemList, int fromIndex, int toIndex)
        {
            if (dynamicEffectCompiler == null)
            {
                throw new InvalidOperationException("This instance is not correctly initialized (no EffectName)");
            }

            // Get all meshes from render models
            meshesToRender.Clear();
            for(int i = fromIndex; i <= toIndex; i++)
            {
                meshesToRender.Add((RenderMesh)renderItemList[i].DrawContext);
            }

            // Slow path there is a callback
            if (Callbacks.UpdateMeshes != null)
            {
                Callbacks.UpdateMeshes(context, ref meshesToRender);
            }

            // Fetch callback on PreRenderGroup
            var preRenderMesh = Callbacks.PreRenderMesh;

            for (int i = 0; i < meshesToRender.Count; i++)
            {
                var renderMesh = meshesToRender[i];

                // If the EntityGroup is changing, call the callback to allow to plug specific parameters for this group
                if (preRenderMesh != null)
                {
                    preRenderMesh(context, renderMesh);
                }

                // Update Effect and mesh
                UpdateEffect(context, renderMesh, context.Parameters);

                // Draw the mesh
                renderMesh.Draw(context);
            }
        }

        protected override void Unload()
        {
            if (dynamicEffectCompiler != null)
            {
                // Release the slot (note: if shared, it will wait for all its usage to be released)
                slotManager.ReleaseSlot(modelRenderSlot);
                // TODO: Remove RenderMeshes
            }

            base.Unload();
        }

        private void PrepareModels(RenderContext context, List<RenderModel> renderModels, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            var preRenderModel = Callbacks.PreRenderModel;
            foreach (var renderModel in renderModels)
            {
                var modelComponent = renderModel.ModelComponent;
                if (modelComponent.Model == null)
                {
                    continue;
                }

                var meshes = modelComponent.Model.Meshes;
                int meshCount = meshes.Count;
                if (meshCount == 0)
                {
                    continue;
                }

                if (preRenderModel != null && !preRenderModel(context, renderModel))
                {
                    continue;
                }

                // Always prepare the slot for the render meshes even if they are not used.
                for (int i = renderModel.RenderMeshesPerEffectSlot.Count; i <= modelRenderSlot; i++)
                {
                    renderModel.RenderMeshesPerEffectSlot.Add(new FastListStruct<RenderMesh>(meshCount));
                }

                PrepareRenderMeshes(renderModel, meshes, ref renderModel.RenderMeshesPerEffectSlot.Items[modelRenderSlot], opaqueList, transparentList);
            }
        }

        private void PrepareRenderMeshes(RenderModel renderModel, List<Mesh> meshes, ref FastListStruct<RenderMesh> renderMeshes, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            // Add new render meshes
            for (int i = renderMeshes.Count; i < meshes.Count; i++)
            {
                var renderMesh = new RenderMesh(renderModel, meshes[i]);
                renderMeshes.Add(renderMesh);
            }

            // Create the bounding frustum locally on the stack, so that frustum.Contains is performed with boundingBox that is also on the stack
            var frustum = new BoundingFrustum(ref ViewProjectionMatrix);

            var sceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current);
            var cullingMode = CullingModeOverride ?? (sceneCameraRenderer?.CullingMode ?? CameraCullingMode.None);

            for (int i = 0; i < renderMeshes.Count; i++)
            {
                var renderMesh = renderMeshes[i];
                // Update the model hierarchy
                var modelViewHierarchy = renderModel.ModelComponent.Skeleton;
                modelViewHierarchy.UpdateRenderMesh(renderMesh);

                if (!renderMesh.Enabled || !renderMesh.UpdateMaterial())
                {
                    continue;
                }

                // Upload skinning blend matrices
                BoundingBoxExt boundingBox;
                skinningUpdater.Update(modelViewHierarchy, renderMesh, out boundingBox);

                // Fast AABB transform: http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
                // Compute transformed AABB (by world)
                // TODO: CameraCullingMode should be pluggable
                // TODO: This should not be necessary. Add proper bounding boxes to gizmos etc.
                if (cullingMode == CameraCullingMode.Frustum && boundingBox.Extent != Vector3.Zero && !frustum.Contains(ref boundingBox))
                {
                    continue;
                }

                // Project the position
                // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                var worldPosition = new Vector4(renderMesh.WorldMatrix.TranslationVector, 1.0f);
                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref ViewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                renderMesh.RasterizerState = renderMesh.IsGeometryInverted ? RasterizerStateForInvertedGeometry : RasterizerState;
                renderMesh.ForceRasterizer = ForceRasterizer;

                var list = renderMesh.HasTransparency ? transparentList : opaqueList;
                list.Add(new RenderItem(this, renderMesh, projectedZ));
            }
        }

        /// <summary>
        /// Create or update the Effect of the effect mesh.
        /// </summary>
        protected void UpdateEffect(RenderContext context, RenderMesh renderMesh, ParameterCollection passParameters)
        {
            if (dynamicEffectCompiler.Update(renderMesh, passParameters))
            {
                try
                {
                    renderMesh.Initialize(context.GraphicsDevice);
                }
                catch (Exception e)
                {
                    Log.Error("Could not initialize RenderMesh, trying again with error fallback effect", e);

                    // Try again with error effect to show user something failed with this model
                    // TODO: What if an exception happens in this case too? Mark renderMesh as ignored or null?
                    dynamicEffectCompiler.SwitchFallbackEffect(FallbackEffectType.Error, renderMesh, passParameters);
                    renderMesh.Initialize(context.GraphicsDevice);
                }
            }
        }
    }
}