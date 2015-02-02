// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Graphics;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects.ProceduralModels
{
    /// <summary>
    /// Base class for primitive procedural model.
    /// </summary>
    [DataContract]
    public abstract class PrimitiveProceduralModelBase : IProceduralModel
    {
        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        [DataMember(500)]
        [NotNull]
        [Display("Material")]
        public Material Material { get; set; }

        public unsafe void Generate(IServiceRegistry services, Model model)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (model == null) throw new ArgumentNullException("model");

            var graphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;

            var data = this.CreatePrimitiveMeshData();

            if (data.Vertices.Length == 0)
            {
                throw new InvalidOperationException("Invalid GeometricPrimitive [{0}]. Expecting non-zero Vertices array");
            }

            var boundingBox = new BoundingBox();
            for (int i = 0; i < data.Vertices.Length; i++)
                BoundingBox.Merge(ref boundingBox, ref data.Vertices[i].Position, out boundingBox);

            var originalLayout = data.Vertices[0].GetLayout();

            fixed (void* indexBuffer = data.Indices)
            fixed (void* originalVertexBuffer = data.Vertices)
            {
                var result = TNBExtensions.GenerateTangentBinormal(originalLayout, (IntPtr)originalVertexBuffer, data.Vertices.Length, 0, originalLayout.VertexStride, (IntPtr)indexBuffer, true, data.Indices.Length);

                var meshDraw = new MeshDraw();

                var layout = result.Key;
                var vertexBuffer = result.Value;
                var indices = data.Indices;

                if (indices.Length < 0xFFFF)
                {
                    var indicesShort = new ushort[indices.Length];
                    for (int i = 0; i < indicesShort.Length; i++)
                    {
                        indicesShort[i] = (ushort)indices[i];
                    }
                    meshDraw.IndexBuffer = new IndexBufferBinding(Buffer.Index.New(graphicsDevice, indicesShort).RecreateWith(indicesShort), false, indices.Length);
                }
                else
                {
                    if (graphicsDevice.Features.Profile <= GraphicsProfile.Level_9_3)
                    {
                        throw new InvalidOperationException("Cannot generate more than 65535 indices on feature level HW <= 9.3");
                    }

                    meshDraw.IndexBuffer = new IndexBufferBinding(Buffer.Index.New(graphicsDevice, indices).RecreateWith(indices), true, indices.Length);
                }

                meshDraw.VertexBuffers = new[] { new VertexBufferBinding(Buffer.New(graphicsDevice, vertexBuffer, BufferFlags.VertexBuffer).RecreateWith(vertexBuffer), layout, data.Vertices.Length) };

                meshDraw.DrawCount = indices.Length;
                meshDraw.PrimitiveType = PrimitiveType.TriangleList;

                var mesh = new Mesh { Draw = meshDraw, BoundingBox = boundingBox };
                mesh.Parameters.Set(RenderingParameters.RenderLayer, RenderLayers.All);

                model.BoundingBox = boundingBox;
                model.Add(mesh);

                if (Material != null)
                {
                    model.Materials.Add(Material);
                }
            }
        }

        protected abstract GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData();
    }
}