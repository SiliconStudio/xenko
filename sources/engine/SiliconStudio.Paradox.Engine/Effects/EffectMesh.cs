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
    public class EffectMesh : DynamicEffectInstance
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

        /// <summary>
        /// A shortcut to <see cref="Effects.Mesh.Parameters"/>.
        /// </summary>
        // TODO: We should not put this Parameters here
        public readonly ParameterCollection Parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectMesh" /> class.
        /// </summary>
        /// <param name="renderModel">The render model.</param>
        /// <param name="mesh">The mesh data.</param>
        /// <exception cref="System.ArgumentNullException">mesh</exception>
        public EffectMesh(RenderModel renderModel, Mesh mesh)
        {
            if (renderModel == null) throw new ArgumentNullException("renderModel");
            if (mesh == null) throw new ArgumentNullException("mesh");
            RenderModel = renderModel;
            Mesh = mesh;
            Parameters = mesh.Parameters;
            Enabled = true;
        }

        /// <summary>
        /// Enable or disable this particular effect mesh.
        /// </summary>
        public bool Enabled { get; set; }

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
                // - EffectMesh.Parameters (originally copied from mesh parameters)
                // The order is based on the granularity level of each element and how shared it can be. Material is heavily shared, a model contains many meshes. An effectMesh is unique.
                // TODO: really copy mesh parameters into effectMesh instead of just referencing the meshDraw parameters.

                var modelComponent = this.RenderModel.ModelInstance;
                var hasMaterialParams = this.Mesh.Material != null && this.Mesh.Material.Parameters != null;
                var hasModelComponentParams = modelComponent != null && modelComponent.Parameters != null;
                if (hasMaterialParams)
                {
                    if (hasModelComponentParams)
                        this.Effect.Apply(currentPass.Parameters, this.Mesh.Material.Parameters, modelComponent.Parameters, this.Parameters, true);
                    else
                        this.Effect.Apply(currentPass.Parameters, this.Mesh.Material.Parameters, this.Parameters, true);
                }
                else if (hasModelComponentParams)
                    this.Effect.Apply(currentPass.Parameters, modelComponent.Parameters, this.Parameters, true);
                else
                    this.Effect.Apply(currentPass.Parameters, this.Parameters, true);
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
            Utilities.Dispose(ref vertexArrayObject);
            vertexArrayObject = VertexArrayObject.New(device, Effect.InputSignature, Mesh.Draw.IndexBuffer, Mesh.Draw.VertexBuffers);
        }

        protected internal override void FillParameterCollections(IList<ParameterCollection> parameterCollections)
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

            if (Mesh.Parameters != null)
            {
                parameterCollections.Add(Mesh.Parameters);
            }            
        }
    }
}
