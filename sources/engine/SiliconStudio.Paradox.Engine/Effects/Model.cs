// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Graphics;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Collection of <see cref="Mesh"/>, each one usually being a different LOD of the same Model.
    /// The effect system will select the appropriate LOD depending on distance, current pass, and other effect-specific requirements.
    /// </summary>
    [DataConverter(AutoGenerate = true, ContentReference = true)]
    public class Model : IEnumerable
    {
        private readonly List<Mesh> meshes = new List<Mesh>();
        private IList<Model> children;
        private Model parent;

        /// <summary>
        /// Gets the views.
        /// </summary>
        /// <value>
        /// The views.
        /// </value>
        [DataMemberConvert]
        public IList<Model> Children
        {
            get { return children; }
            set { children = value; }
        }

        /// <summary>
        /// Gets the meshes.
        /// </summary>
        /// <value>
        /// The meshes.
        /// </value>
        [DataMemberConvert]
        public List<Mesh> Meshes
        {
            get { return meshes; }
        }

        /// <summary>
        /// Gets or sets the hierarchy definition, which describes nodes name, default transformation and hierarchical parent.
        /// </summary>
        /// <value>
        /// The hierarchy, which describes nodes name, default transformation and hierarchical parent.
        /// </value>
        [DataMemberConvert]
        public ModelViewHierarchyDefinition Hierarchy { get; set; }

        /// <summary>
        /// Gets or sets the bounding box encompassing all the <see cref="Meshes"/> (not including animation).
        /// </summary>
        /// <value>
        /// The bounding box.
        /// </value>
        [DataMemberConvert]
        public BoundingBox BoundingBox { get; set; }

        // Temporarily removed
        //[DataMemberConvert]
        //public ParameterCollection Parameters
        //{
        //    get { return parameters; }
        //}

        /// <summary>
        /// Adds the specified model view (for collection initializers).
        /// </summary>
        /// <param name="model">The model view.</param>
        public void Add(Model model)
        {
            children.Add(model);
        }

        /// <summary>
        /// Adds the specified mesh (for collection initializers).
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public void Add(Mesh mesh)
        {
            Meshes.Add(mesh);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public static Model FromGeometricMeshData(GraphicsDevice graphicsDevice, GeometricMeshData<VertexPositionNormalTexture> geometryMesh, string effectName = "Default")
        {
            var vertices = geometryMesh.Vertices;

            // compute the bounding box of the primitive
            var boundingBox = BoundingBox.Empty;
            for (int i = 0; i < vertices.Length; i++)
                BoundingBox.Merge(ref boundingBox, ref vertices[i].Position, out boundingBox);

            return FromGeometricMeshData(graphicsDevice, geometryMesh, boundingBox, VertexPositionNormalTexture.Layout, effectName);
        }

        public static Model FromGeometricMeshData(GraphicsDevice graphicsDevice, GeometricMeshData<VertexPositionNormalTangentMultiTexture> geometryMesh, string effectName = "Default")
        {
            var vertices = geometryMesh.Vertices;

            // compute the bounding box of the primitive
            var boundingBox = BoundingBox.Empty;
            for (int i = 0; i < vertices.Length; i++)
                BoundingBox.Merge(ref boundingBox, ref vertices[i].Position, out boundingBox);

            return FromGeometricMeshData(graphicsDevice, geometryMesh, boundingBox, VertexPositionNormalTangentMultiTexture.Layout, effectName);
        }

        public static Model FromGeometricMeshData<T>(GraphicsDevice graphicsDevice, GeometricMeshData<T> geometryMesh, BoundingBox boundingBox, VertexDeclaration layout, string effectName = "Default") where T : struct, IVertex
        {
            var meshDraw = new MeshDraw();

            var vertices = geometryMesh.Vertices;
            var indices = geometryMesh.Indices;

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

            meshDraw.VertexBuffers = new[] { new VertexBufferBinding(Buffer.Vertex.New(graphicsDevice, vertices).RecreateWith(vertices), layout, vertices.Length) };

            meshDraw.DrawCount = indices.Length;
            meshDraw.PrimitiveType = PrimitiveType.TriangleList;

            var mesh = new Mesh { Draw = meshDraw, BoundingBox = boundingBox };
            mesh.Parameters.Set(RenderingParameters.RenderLayer, RenderLayers.RenderLayerAll);

            var model = new Model { BoundingBox = boundingBox };
            model.Add(mesh);

            return model;
        }

        /// <summary>
        /// Create a clone with its own ParameterCollection.
        /// It allows reuse of a single Model for multiple ModelComponent.
        /// </summary>
        public Model Instantiate()
        {
            var result = new Model();
            if (Children != null)
            {
                result.Children = new List<Model>();
                foreach (var child in Children)
                {
                    result.Children.Add(child.Instantiate());
                }
            }

            foreach (var mesh in Meshes)
            {
                var meshCopy = new Mesh(mesh);
                if (meshCopy.Parameters != null)
                    meshCopy.Parameters = meshCopy.Parameters.Clone();
                result.Meshes.Add(meshCopy);
            }

            result.Hierarchy = Hierarchy;
            result.BoundingBox = BoundingBox;

            return result;
        }

        private void children_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var child = (Model)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (child.parent != null)
                        throw new InvalidOperationException("Model already have a parent.");
                    child.parent = this;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (child.parent != this)
                        throw new InvalidOperationException("Model doesn't have expected parent.");
                    child.parent = null;
                    break;
            }
        }
    }
}