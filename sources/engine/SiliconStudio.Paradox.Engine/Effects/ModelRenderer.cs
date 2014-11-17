// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="Renderer"/> is responsible to prepare and render meshes for a specific pass.
    /// </summary>
    public class ModelRenderer : Renderer
    {
        #region Constants and Fields

        protected int MeshPassSlot;

        private FastList<EffectMesh> meshesToRender = new FastList<EffectMesh>();

        private EffectParameterUpdater updater;

        private ParameterCollection[] parameterCollections;

        private string effectName;

        #endregion

        // Temporary until bytecode names exists
        public ModelRenderer(IServiceRegistry services, string effectName) : base(services)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            this.effectName = effectName;

            updater = new EffectParameterUpdater();
            parameterCollections = new ParameterCollection[5];
        }

        public bool EnableFrustumCulling { get; set; }

        public string EffectName
        {
            get
            {
                return effectName;
            }
            set
            {
                effectName = value;
            }
        }

        #region Public methods

        public override void Load()
        {
            var pipelineMeshState = Pass.GetOrCreateMeshRenderState();

            // Get the slot for the pass of this processor
            MeshPassSlot = pipelineMeshState.GetMeshPassSlot(Pass);

            // Register callback for preparing RenderModel for rendering
            pipelineMeshState.PrepareRenderModel += PrepareModelForRendering;

            // Register callback for rendering meshes extracted from RenderModels
            Pass.StartPass += RenderMeshes;
        }

        public override void Unload()
        {
            var pipelineMeshState = Pass.GetOrCreateMeshRenderState();

            // Register callback for preparing RenderModel for rendering
            pipelineMeshState.PrepareRenderModel -= PrepareModelForRendering;

            // Register callback for rendering meshes extracted from RenderModels
            Pass.StartPass -= RenderMeshes;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Draws the mesh stored in the current <see cref="RenderContext" />
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectMesh">The current effect mesh.</param>
        protected virtual void RenderMesh(RenderContext context, EffectMesh effectMesh)
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

        protected virtual void RenderMeshes(RenderContext context)
        {
            var state = Pass.GetMeshRenderState();

            // Get all meshes from render models
            meshesToRender.Clear();
            foreach (var renderModel in state.RenderModels)
            {
                var meshes = renderModel.InternalMeshes[MeshPassSlot];
                if (meshes != null)
                    meshesToRender.AddRange(meshes);
            }

            foreach (var effectMesh in meshesToRender)
            {
                if (EffectSystem.WasEffectRecompiled(effectMesh.Effect))
                    EffectMeshRefresh(effectMesh, effectMesh.ModelComponent.Parameters);
            }

            // Update meshes
            UpdateMeshes(context, ref meshesToRender);

            PreRender(context);

            // TODO: separate update effect and render to tightly batch render calls vs 1 cache-friendly loop on meshToRender
            foreach (var mesh in meshesToRender)
            {
                PreEffectUpdate(context, mesh);
                UpdateEffectMesh(mesh);
                PostEffectUpdate(context, mesh);
                mesh.Render(context, mesh);
            }

            PostRender(context);
        }

        protected virtual void PrepareMeshesForRendering(RenderModel renderModel, Model model, ParameterCollection modelComponentParameters)
        {
            foreach (var mesh in model.Meshes)
            {
                var effectMesh = new EffectMesh(null, mesh);
                CreateEffect(effectMesh, modelComponentParameters);

                // Register mesh for rendering
                if (renderModel.InternalMeshes[MeshPassSlot] == null)
                {
                    renderModel.InternalMeshes[MeshPassSlot] = new List<EffectMesh>();
                }
                renderModel.InternalMeshes[MeshPassSlot].Add(effectMesh);
            }
        }

        protected virtual void PrepareModelForRendering(RenderModel renderModelView, ParameterCollection modelComponentParameters)
        {
            // TODO:FX Select appropriate view model
            var model = renderModelView.Model;
            PrepareMeshesForRendering(renderModelView, model, modelComponentParameters);
        }

        protected virtual void UpdateMeshes(RenderContext context, ref FastList<EffectMesh> meshes)
        {
            for (var i = 0; i < meshes.Count; ++i)
            {
                var mesh = meshes[i];
                // Remove non-enabled effect meshes
                if (!mesh.Enabled)
                {
                    meshes.SwapRemoveAt(i--);
                    continue;
                }

                ModelComponent modelComponent = mesh.ModelComponent;
                if (modelComponent == null)
                    continue;

                // Remove non-enabled mesh components
                if (!modelComponent.Enabled)
                {
                    meshes.SwapRemoveAt(i--);
                    continue;
                }

                if (meshes[i].MeshData.Layer == RenderLayers.RenderLayerNone)
                {
                    meshes.SwapRemoveAt(i--);
                    continue;
                }
            }

            // Frustum culling
            if (EnableFrustumCulling)
            {
                PerformFrustumCulling(meshes);
            }

            // Sort based on ModelComponent.DrawOrder
            meshes.Sort(ModelComponentSorter.Default);
        }

        private void PerformFrustumCulling(FastList<EffectMesh> meshes)
        {
            Matrix viewProjection, mat1, mat2;

            // Compute view * projection
            Pass.Parameters.Get(TransformationKeys.View, out mat1);
            Pass.Parameters.Get(TransformationKeys.Projection, out mat2);
            Matrix.Multiply(ref mat1, ref mat2, out viewProjection);

            var frustum = new BoundingFrustum(ref viewProjection);

            for (var i = 0; i < meshes.Count; ++i)
            {
                var mesh = meshes[i];

                // Fast AABB transform: http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
                // Get world matrix
                mesh.Parameters.Get(TransformationKeys.World, out mat1);

                // Compute transformed AABB (by world)
                var boundingBox = mesh.MeshData.BoundingBox;
                var center = boundingBox.Center;
                var extent = boundingBox.Extent;

                Vector3.TransformCoordinate(ref center, ref mat1, out center);

                // Update world matrix into absolute form
                unsafe
                {
                    float* matrixData = &mat1.M11;
                    for (int j = 0; j < 16; ++j)
                    {
                        *matrixData = Math.Abs(*matrixData);
                        ++matrixData;
                    }
                }

                Vector3.TransformNormal(ref extent, ref mat1, out extent);

                // Perform frustum culling
                if (!Collision.FrustumContainsBox(ref frustum, ref center, ref extent))
                {
                    meshes.SwapRemoveAt(i--);
                }
            }
        }

        protected virtual void PreRender(RenderContext context)
        {
        }

        protected virtual void PostRender(RenderContext context)
        {
        }

        protected virtual void PreEffectUpdate(RenderContext context, EffectMesh effectMesh)
        {
        }

        protected virtual void PostEffectUpdate(RenderContext context, EffectMesh effectMesh)
        {
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

        #endregion

        #region Private methods

        private void UpdateEffectMeshEffect(EffectMesh effectMesh, Effect effect, ParameterCollection modelComponentParameters)
        {
            if (effect != null && !ReferenceEquals(effect, effectMesh.Effect))
            {
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
                effectMesh.Effect = effect;
                effectMesh.VertexArrayObject = VertexArrayObject.New(GraphicsDevice, effect.InputSignature, mesh.Draw.IndexBuffer, mesh.Draw.VertexBuffers);
                effectMesh.Render = RenderMesh;
                effectMesh.RenderData = mesh.Draw;

                effectMesh.UpdaterDefinition = new EffectParameterUpdaterDefinition(effect.CompilationParameters);
                var keyMapping = new Dictionary<ParameterKey, int>();
                for (int i = 0; i < effectMesh.UpdaterDefinition.SortedKeys.Length; i++)
                    keyMapping.Add(effectMesh.UpdaterDefinition.SortedKeys[i], i);
                effect.CompilationParameters.SetKeyMapping(keyMapping);
                effect.DefaultCompilationParameters.SetKeyMapping(keyMapping);
                effectMesh.UpdaterDefinition.SortedLevels = GetLevels(effectMesh, modelComponentParameters);

                UpdateEffectMeshCounters(effectMesh);
            }
            else if (effectMesh.Effect != null)
            {
                effectMesh.UpdaterDefinition.UpdateCounter(effect.CompilationParameters);
                effectMesh.UpdaterDefinition.SortedLevels = GetLevels(effectMesh, modelComponentParameters);

                UpdateEffectMeshCounters(effectMesh);
            }
        }

        private void UpdateEffectMeshCounters(EffectMesh effectMesh)
        {
            for (var i = 0; i < effectMesh.UpdaterDefinition.SortedLevels.Length; ++i)
            {
                var kvp = updater.GetAtIndex(i);
                effectMesh.UpdaterDefinition.SortedCounters[i] = kvp.Value.Counter;
            }
        }

        private void EffectMeshRefresh(EffectMesh effectMesh, ParameterCollection modelComponentParameters)
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

            effectMesh.UpdaterDefinition = new EffectParameterUpdaterDefinition(effect.CompilationParameters);
            var keyMapping = new Dictionary<ParameterKey, int>();
            for (int i = 0; i < effectMesh.UpdaterDefinition.SortedKeys.Length; i++)
                keyMapping.Add(effectMesh.UpdaterDefinition.SortedKeys[i], i);
            effect.CompilationParameters.SetKeyMapping(keyMapping);
            effect.DefaultCompilationParameters.SetKeyMapping(keyMapping);
            effectMesh.UpdaterDefinition.SortedLevels = GetLevels(effectMesh, modelComponentParameters);
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
            PrepareUpdater(effectMesh, effectMesh.ModelComponent.Parameters);

            for (var i = 0; i < effectMesh.UpdaterDefinition.SortedLevels.Length; ++i)
            {
                var kvp = updater.GetAtIndex(i);
                if (effectMesh.UpdaterDefinition.SortedLevels[i] == kvp.Key)
                {
                    //TODO: use cache to speed up equality check between complex object (such as ShaderMixinSource)
                    if (effectMesh.UpdaterDefinition.SortedCounters[i] != kvp.Value.Counter && !Equals(effectMesh.UpdaterDefinition.SortedCompilationValues[i], kvp.Value.Object))
                        return true;
                }
                else
                {
                    if (!Equals(effectMesh.UpdaterDefinition.SortedCompilationValues[i], kvp.Value.Object))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Prepare the EffectParameterUpdater for the effect mesh.
        /// </summary>
        /// <param name="effectMesh">The effect mesh.</param>
        /// <param name="modelComponentParameters">The ModelComponent parameters.</param>
        private void PrepareUpdater(EffectMesh effectMesh, ParameterCollection modelComponentParameters)
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
            if (modelComponentParameters != null)
                parameterCollections[collectionCount++] = modelComponentParameters;
            if (mesh.Parameters != null)
                parameterCollections[collectionCount++] = mesh.Parameters;

            parameterCollections[collectionCount++] = GraphicsDevice.Parameters;

            updater.Update(effectMesh.UpdaterDefinition, parameterCollections, collectionCount);
        }

        /// <summary>
        /// Get the levels of the parameters.
        /// </summary>
        /// <param name="effectMesh">The effect mesh.</param>
        /// <param name="modelComponentParameters">The ModelComponent parameters.</param>
        /// <returns>A table of levels.</returns>
        private int[] GetLevels(EffectMesh effectMesh, ParameterCollection modelComponentParameters)
        {
            PrepareUpdater(effectMesh, modelComponentParameters);
            
            var levels = new int[effectMesh.UpdaterDefinition.SortedKeyHashes.Length];
            for (var i = 0; i < levels.Length; ++i)
            {
                levels[i] = updater.GetAtIndex(i).Key;
            }
            return levels;
        }

        #endregion

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