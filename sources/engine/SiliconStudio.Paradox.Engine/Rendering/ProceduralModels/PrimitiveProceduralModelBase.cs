// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    /// <summary>
    /// Base class for primitive procedural model.
    /// </summary>
    [DataContract]
    public abstract class PrimitiveProceduralModelBase : IProceduralModel
    {
        protected PrimitiveProceduralModelBase()
        {
            MaterialInstance = new MaterialInstance();
        }
        /// <summary>
        /// Gets the material instance.
        /// </summary>
        /// <value>The material instance.</value>
        [DataMember(500)]
        [NotNull]
        [Display("Material")]
        public MaterialInstance MaterialInstance { get; private set; }

        public void SetMaterial(string name, Material material)
        {
            if (name == "Material")
            {
                MaterialInstance.Material = material;
            }
        }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IEnumerable<KeyValuePair<string, MaterialInstance>> MaterialInstances { get { yield return new KeyValuePair<string, MaterialInstance>("Material", MaterialInstance); } }

        public void Generate(IServiceRegistry services, Model model)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (model == null) throw new ArgumentNullException("model");

            var graphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;

            var data = this.CreatePrimitiveMeshData();

            if (data.Vertices.Length == 0)
            {
                throw new InvalidOperationException("Invalid GeometricPrimitive [{0}]. Expecting non-zero Vertices array");
            }

            var boundingBox = BoundingBox.Empty;
            for (int i = 0; i < data.Vertices.Length; i++)
                BoundingBox.Merge(ref boundingBox, ref data.Vertices[i].Position, out boundingBox);

            BoundingSphere boundingSphere;
            unsafe
            {
                fixed (void* verticesPtr = data.Vertices)
                    BoundingSphere.FromPoints((IntPtr)verticesPtr, 0, data.Vertices.Length, VertexPositionNormalTexture.Size, out boundingSphere);
            }

            var originalLayout = data.Vertices[0].GetLayout();

            // Generate Tangent/BiNormal vectors
            var resultWithTangentBiNormal = VertexHelper.GenerateTangentBinormal(originalLayout, data.Vertices, data.Indices);

            // Generate Multitexcoords
            var result = VertexHelper.GenerateMultiTextureCoordinates(resultWithTangentBiNormal);

            var meshDraw = new MeshDraw();

            var layout = result.Layout;
            var vertexBuffer = result.VertexBuffer;
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

            var mesh = new Mesh { Draw = meshDraw, BoundingBox = boundingBox, BoundingSphere = boundingSphere };

            model.BoundingBox = boundingBox;
            model.BoundingSphere = boundingSphere;
            model.Add(mesh);

            if (MaterialInstance != null && MaterialInstance.Material != null)
            {
                model.Materials.Add(MaterialInstance);
            }
        }

        protected abstract GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData();
    }
}