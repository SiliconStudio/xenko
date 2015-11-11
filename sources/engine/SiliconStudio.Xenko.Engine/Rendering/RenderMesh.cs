// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Rendering.Utils;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// An mesh associated with an effect to be rendered.
    /// </summary>
    public class RenderMesh : DynamicEffectInstance
    {
        private VertexArrayObject vertexArrayObject;
        private VertexArrayObject vertexArrayObjectAEN;

        /// <summary>
        /// The model instance associated to this effect mesh.
        /// </summary>
        /// <value>The model instance.</value>
        public readonly RenderModel RenderModel;

        /// <summary>
        /// The mesh associated with this instance.
        /// </summary>
        public readonly Mesh Mesh;

        public Material Material;

        public bool IsShadowCaster;

        public bool IsShadowReceiver;

        /// <summary>
        /// A Rasterizer state setup before <see cref="Draw"/> when rendering this mesh.
        /// </summary>
        public RasterizerState RasterizerState;

        /// <summary>
        /// A boolean indicating whether the RasterizerState defines by this instance is overloading all other rasterizer states (coming from Material for example).
        /// </summary>
        public bool ForceRasterizer;

        public bool HasTransparency { get; private set; }

        public bool IsGeometryInverted;

        public Matrix WorldMatrix;

        public int MatrixCounter;

        private readonly ParameterCollection parameters;
        private FastListStruct<ParameterCollection> parameterCollections;
        private EffectParameterCollectionGroup parameterCollectionGroup;
        private FastListStruct<ParameterCollection> previousParameterCollections;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderMesh" /> class.
        /// </summary>
        /// <param name="renderModel">The render model.</param>
        /// <param name="mesh">The mesh data.</param>
        /// <exception cref="System.ArgumentNullException">mesh</exception>
        public RenderMesh(RenderModel renderModel, Mesh mesh)
        {
            if (renderModel == null) throw new ArgumentNullException("renderModel");
            if (mesh == null) throw new ArgumentNullException("mesh");
            RenderModel = renderModel;
            Mesh = mesh;
            Enabled = true;
            parameterCollections = new FastListStruct<ParameterCollection>(8);
            previousParameterCollections = new FastListStruct<ParameterCollection>(8);

            UpdateMaterial();

            // A RenderMesh is inheriting values from Mesh.Parameters
            // We are considering that Mesh.Parameters is not updated frequently (should be almost immutable)
            parameters = new ParameterCollection();
            if (mesh.Parameters != null)
            {
                parameters.AddSources(mesh.Parameters);
            }
        }

        /// <summary>
        /// Enable or disable this particular effect mesh.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Draw this effect mesh.
        /// </summary>
        public void Draw(RenderContext context)
        {
            // Retrieve effect parameters
            var graphicsDevice = context.GraphicsDevice;
            var mesh = Mesh;
            var currentRenderData = mesh.Draw;
            var material = Material;
            var vao = vertexArrayObject;
            var drawCount = currentRenderData.DrawCount;
            var primitiveType = currentRenderData.PrimitiveType;

            parameters.Set(TransformationKeys.World, WorldMatrix);

            // TODO: We should clarify exactly how to override rasterizer states. Currently setup here on Context.Parameters to allow Material/ModelComponent overrides, but this is ugly
            // Apply rasterizer
            var rasterizer = RasterizerState; 
            if (!ForceRasterizer && Material.CullMode.HasValue && Material.CullMode.Value != RasterizerState.Description.CullMode)
            {
                switch (Material.CullMode.Value)
                {
                    case CullMode.Back:
                        rasterizer = graphicsDevice.RasterizerStates.CullBack;
                        break;
                    case CullMode.Front:
                        rasterizer = graphicsDevice.RasterizerStates.CullFront;
                        break;
                    case CullMode.None:
                        rasterizer = graphicsDevice.RasterizerStates.CullNone;
                        break;
                }
            }
            context.Parameters.Set(Effect.RasterizerStateKey, rasterizer);

            // Handle picking
            if (context.IsPicking()) // TODO move this code corresponding to picking outside of the runtime code!
            {
                parameters.Set(ModelComponentPickingShaderKeys.ModelComponentId, new Color4(RenderModel.ModelComponent.Id.ToRuntimeId()));
                parameters.Set(ModelComponentPickingShaderKeys.MeshId, new Color4(RenderModel.ModelComponent.Model.Meshes.IndexOf(Mesh)));
                parameters.Set(ModelComponentPickingShaderKeys.MaterialId, new Color4(Mesh.MaterialIndex));

                // Don't use the materials blend state on picking targets
                parameters.Set(Effect.BlendStateKey, null);
            }

            if (material != null && material.TessellationMethod != XenkoTessellationMethod.None)
            {
                var tessellationMethod = material.TessellationMethod;

                // adapt the primitive type and index buffer to the tessellation used
                if (tessellationMethod.PerformsAdjacentEdgeAverage())
                {
                    vao = GetOrCreateVertexArrayObjectAEN(context);
                    drawCount = 12 / 3 * drawCount;
                }
                primitiveType = tessellationMethod.GetPrimitiveType();
            }

            //using (Profiler.Begin(ProfilingKeys.PrepareMesh))
            {
                // Order of application of parameters:
                // - RenderPass.Parameters
                // - ModelComponent.Parameters
                // - RenderMesh.Parameters (originally copied from mesh parameters)
                // The order is based on the granularity level of each element and how shared it can be. Material is heavily shared, a model contains many meshes. An renderMesh is unique.
                // TODO: really copy mesh parameters into renderMesh instead of just referencing the meshDraw parameters.

                //var modelComponent = RenderModel.ModelComponent;
                //var hasModelComponentParams = modelComponent != null && modelComponent.Parameters != null;
                
                //var materialParameters = material != null && material.Parameters != null ? material.Parameters : null;

                parameterCollections.Clear();

                parameterCollections.Add(context.Parameters);
                FillParameterCollections(ref parameterCollections);

                // Check if we need to recreate the EffectParameterCollectionGroup
                // TODO: We can improve performance by redesigning FillParameterCollections to avoid ArrayExtensions.ArraysReferenceEqual (or directly check the appropriate parameter collections)
                // This also happens in another place: DynamicEffectCompiler (we probably want to factorize it when doing additional optimizations)
                if (parameterCollectionGroup == null || parameterCollectionGroup.Effect != Effect || !ArrayExtensions.ArraysReferenceEqual(ref previousParameterCollections, ref parameterCollections))
                {
                    previousParameterCollections.Clear();
                    previousParameterCollections.AddRange(parameterCollections);
                    parameterCollectionGroup = new EffectParameterCollectionGroup(context.GraphicsDevice, Effect, previousParameterCollections.Count, previousParameterCollections.Items);
                }

                Effect.Apply(context.GraphicsDevice, parameterCollectionGroup, true);
            }

            //using (Profiler.Begin(ProfilingKeys.RenderMesh))
            {
                if (currentRenderData != null)
                {
                    graphicsDevice.SetVertexArrayObject(vao);

                    if (currentRenderData.IndexBuffer == null)
                    {
                        graphicsDevice.Draw(primitiveType, drawCount, currentRenderData.StartLocation);
                    }
                    else
                    {
                        graphicsDevice.DrawIndexed(primitiveType, drawCount, currentRenderData.StartLocation);
                    }
                }
            }
        }

        private VertexArrayObject GetOrCreateVertexArrayObjectAEN(RenderContext context)
        {
            if (vertexArrayObjectAEN == null)
            {
                var graphicsDevice = context.GraphicsDevice;
                var indicesAEN = IndexExtensions.GenerateIndexBufferAEN(Mesh.Draw.IndexBuffer, Mesh.Draw.VertexBuffers[0]);
                var indexBufferBinding = new IndexBufferBinding(Buffer.Index.New(graphicsDevice, indicesAEN), true, Mesh.Draw.IndexBuffer.Count * 12 / 3);
                vertexArrayObjectAEN = VertexArrayObject.New(context.GraphicsDevice, Effect.InputSignature, indexBufferBinding, Mesh.Draw.VertexBuffers);
            }

            return vertexArrayObjectAEN;
        }

        public bool UpdateMaterial()
        {
            var materialIndex = Mesh.MaterialIndex;
            Material = RenderModel.GetMaterial(materialIndex);
            var materialInstance = RenderModel.GetMaterialInstance(materialIndex);
            HasTransparency = Material != null && Material.HasTransparency;

            var modelComponent = RenderModel.ModelComponent;
            IsShadowCaster = modelComponent.IsShadowCaster;
            IsShadowReceiver = modelComponent.IsShadowReceiver;
            if (materialInstance != null)
            {
                IsShadowCaster = IsShadowCaster && materialInstance.IsShadowCaster;
                IsShadowReceiver = IsShadowReceiver && materialInstance.IsShadowReceiver;
            }

            return Material != null;
        }

        internal void Initialize(GraphicsDevice device)
        {
            vertexArrayObject = VertexArrayObject.New(device, Effect.InputSignature, Mesh.Draw.IndexBuffer, Mesh.Draw.VertexBuffers);
        }

        public override void FillParameterCollections(ref FastListStruct<ParameterCollection> parameterCollections)
        {
            var material = Material;
            if (material != null && material.Parameters != null)
            {
                parameterCollections.Add(material.Parameters);
            }

            if (RenderModel.ModelComponent.Parameters != null)
            {
                parameterCollections.Add(RenderModel.ModelComponent.Parameters);
            }

            // TODO: Should we add RenderMesh.Parameters before ModelComponent.Parameters to allow user overiddes at component level?
            parameterCollections.Add(parameters);
        }

        public override void Dispose()
        {
            if (vertexArrayObject != null)
            {
                vertexArrayObject.Dispose();
                vertexArrayObject = null;
            }

            if (vertexArrayObjectAEN != null)
            {
                vertexArrayObjectAEN.Dispose();
                vertexArrayObjectAEN = null;
            }
        }
    }
}
