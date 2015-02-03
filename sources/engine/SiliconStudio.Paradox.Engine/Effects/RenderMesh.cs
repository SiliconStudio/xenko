// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// An effect mesh.
    /// </summary>
    public class RenderMesh : DynamicEffectInstance
    {
        private VertexArrayObject vertexArrayObject;

        /// <summary>
        /// The model instance associated to this effect mesh.
        /// </summary>
        /// <value>The model instance.</value>
        public readonly RenderModel RenderModel;

        /// <summary>
        /// The mesh associated with this instance.
        /// </summary>
        public readonly Mesh Mesh;

        private readonly ParameterCollection parameters;

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
            var currentPass = context.CurrentPass;
            var currentRenderData = this.Mesh.Draw;

            //using (Profiler.Begin(ProfilingKeys.PrepareMesh))
            {
                // Order of application of parameters:
                // - RenderPass.Parameters
                // - ModelComponent.Parameters
                // - RenderMesh.Parameters (originally copied from mesh parameters)
                // The order is based on the granularity level of each element and how shared it can be. Material is heavily shared, a model contains many meshes. An renderMesh is unique.
                // TODO: really copy mesh parameters into renderMesh instead of just referencing the meshDraw parameters.

                var modelComponent = this.RenderModel.ModelInstance;
                var hasMaterialParams = this.Mesh.Material != null && this.Mesh.Material.Parameters != null;
                var hasModelComponentParams = modelComponent != null && modelComponent.Parameters != null;
                if (hasMaterialParams)
                {
                    if (hasModelComponentParams)
                        this.Effect.Apply(currentPass.Parameters, this.Mesh.Material.Parameters, modelComponent.Parameters, parameters, true);
                    else
                        this.Effect.Apply(currentPass.Parameters, this.Mesh.Material.Parameters, parameters, true);
                }
                else if (hasModelComponentParams)
                    this.Effect.Apply(currentPass.Parameters, modelComponent.Parameters, parameters, true);
                else
                    this.Effect.Apply(currentPass.Parameters, parameters, true);
            }

            //using (Profiler.Begin(ProfilingKeys.RenderMesh))
            {
                if (currentRenderData != null)
                {
                    var graphicsDevice = context.GraphicsDevice;

                    graphicsDevice.SetVertexArrayObject(vertexArrayObject);

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

        internal void Initialize(GraphicsDevice device)
        {
            vertexArrayObject = VertexArrayObject.New(device, Effect.InputSignature, Mesh.Draw.IndexBuffer, Mesh.Draw.VertexBuffers);
        }

        public override void FillParameterCollections(IList<ParameterCollection> parameterCollections)
        {
            if (Mesh.Material != null)
            {
                parameterCollections.Add(Mesh.Material.Parameters);
            }

            var modelInstance = RenderModel.ModelInstance;
            if (modelInstance != null && modelInstance.Parameters != null)
            {
                parameterCollections.Add(modelInstance.Parameters);
            }

            parameterCollections.Add(parameters);
        }
    }
}
