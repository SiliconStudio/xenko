// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Performs hierarchical updates for a given <see cref="Model"/>.
    /// </summary>
    public class ModelViewHierarchyUpdater
    {
        private static ModelNodeDefinition[] defaultModelNodeDefinition
            = { new ModelNodeDefinition { Name = "Root", ParentIndex = -1, Transform = { Scaling = Vector3.One }, Flags = ModelNodeFlags.Default } };

        private ModelNodeDefinition[] nodes;
        private ModelNodeTransformation[] nodeTransformations;

        public ModelNodeDefinition[] Nodes
        {
            get { return nodes; }
        }

        public ModelNodeTransformation[] NodeTransformations
        {
            get { return nodeTransformations; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelViewHierarchyUpdater"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModelViewHierarchyUpdater(Model model)
        {
            Initialize(model);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelViewHierarchyUpdater"/> class.
        /// </summary>
        /// <param name="nodes">The model view nodes.</param>
        public ModelViewHierarchyUpdater(ModelNodeDefinition[] nodes)
        {
            Initialize(nodes);
        }

        public void Initialize(Model model)
        {
            Initialize(model.Hierarchy != null ? model.Hierarchy.Nodes : defaultModelNodeDefinition);

            // First node is directly uploaded as a matrix
            nodeTransformations[0].Flags &= ~ModelNodeFlags.EnableTransform;
        }

        public void Initialize(ModelNodeDefinition[] nodes)
        {
            this.nodes = nodes;

            if (nodeTransformations == null || nodeTransformations.Length < this.nodes.Length)
                nodeTransformations = new ModelNodeTransformation[this.nodes.Length];

            for (int index = 0; index < nodes.Length; index++)
            {
                nodeTransformations[index].ParentIndex = nodes[index].ParentIndex;
                nodeTransformations[index].Transform = nodes[index].Transform;
                nodeTransformations[index].Flags = nodes[index].Flags;
                nodeTransformations[index].RenderingEnabledRecursive = true;
            }
        }

        /// <summary>
        /// Resets initial values.
        /// </summary>
        public void ResetInitialValues()
        {
            var nodesLocal = nodes;
            for (int index = 0; index < nodesLocal.Length; index++)
            {
                nodeTransformations[index].Transform = nodesLocal[index].Transform;
            }
        }

        /// <summary>
        /// For each node, updates the world matrices from local matrices.
        /// </summary>
        public void UpdateMatrices()
        {
            // Compute transformations
            var nodesLength = nodes.Length;
            for (int index = 0; index < nodesLength; index++)
            {
                UpdateNode(ref nodeTransformations[index]);
            }
        }

        /// <summary>
        /// Updates previously computed world matrices to TransformationKeys.World for each <see cref="Mesh"/>.
        /// </summary>
        /// <param name="renderModel">The render model.</param>
        public void UpdateToRenderModel(RenderModel renderModel)
        {
            var nodeTransformationsLocal = this.nodeTransformations;

            // Set World matrices in mesh parameters
            foreach (var meshes in renderModel.RenderMeshes)
            {
                if (meshes == null)
                    continue;

                foreach (var renderMesh in meshes)
                {
                    var enabled = nodeTransformationsLocal[renderMesh.Mesh.NodeIndex].RenderingEnabledRecursive;
                    renderMesh.Enabled = enabled;
                    if (enabled)
                    {
                        renderMesh.Parameters.Set(TransformationKeys.World, nodeTransformationsLocal[renderMesh.Mesh.NodeIndex].WorldMatrix);
                    }
                }
            }
        }

        public void GetWorldMatrix(int index, out Matrix matrix)
        {
            matrix = nodeTransformations[index].WorldMatrix;
        }

        public void GetLocalMatrix(int index, out Matrix matrix)
        {
            matrix = nodeTransformations[index].LocalMatrix;
        }

        private void UpdateNode(ref ModelNodeTransformation node)
        {
            // Compute LocalMatrix
            if ((node.Flags & ModelNodeFlags.EnableTransform) == ModelNodeFlags.EnableTransform)
            {
                TransformationComponent.CreateMatrixTRS(ref node.Transform.Translation, ref node.Transform.Rotation, ref node.Transform.Scaling, out node.LocalMatrix);
            }

            var nodeTransformationsLocal = this.nodeTransformations;

            var parentIndex = node.ParentIndex;

            // Update Enabled
            bool renderingEnabledRecursive = (node.Flags & ModelNodeFlags.EnableRender) == ModelNodeFlags.EnableRender;
            if (parentIndex != -1)
                renderingEnabledRecursive &= nodeTransformationsLocal[parentIndex].RenderingEnabledRecursive;

            node.RenderingEnabledRecursive = renderingEnabledRecursive;

            if (renderingEnabledRecursive)
            {
                // Compute WorldMatrix
                if (parentIndex != -1)
                    Matrix.Multiply(ref node.LocalMatrix, ref nodeTransformationsLocal[parentIndex].WorldMatrix, out node.WorldMatrix);
                else
                    node.WorldMatrix = node.LocalMatrix;
            }
        }
    }
}