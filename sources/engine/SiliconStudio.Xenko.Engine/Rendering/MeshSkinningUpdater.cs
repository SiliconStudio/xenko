// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Performs blend matrix skinning.
    /// </summary>
    public struct MeshSkinningUpdater
    {
        Matrix[] boneMatrices;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshSkinningUpdater"/> struct.
        /// </summary>
        /// <param name="skinningCapacity">The skinning capacity.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">skinningCapacity;Must be >= 0</exception>
        public MeshSkinningUpdater(int skinningCapacity)
        {
            if (skinningCapacity < 0) throw new ArgumentOutOfRangeException("skinningCapacity", skinningCapacity, "Must be >= 0");

            boneMatrices = new Matrix[skinningCapacity];
        }

        public void Update(SkeletonUpdater hierarchyUpdater, RenderMesh renderMesh, out BoundingBoxExt boundingBox)
        {
            var mesh = renderMesh.Mesh;
            var skinning = mesh.Skinning;

            if (skinning == null)
            {
                // For unskinned meshes, use the original bounding box
                var boundingBoxExt = (BoundingBoxExt)mesh.BoundingBox;
                boundingBoxExt.Transform(renderMesh.WorldMatrix);
                boundingBox = boundingBoxExt;
                return;
            }

            var bones = skinning.Bones;

            // Make sure there is enough spaces in boneMatrices
            if (bones.Length > boneMatrices.Length)
                boneMatrices = new Matrix[bones.Length];

            var bindPoseBoundingBox = new BoundingBoxExt(renderMesh.Mesh.BoundingBox);
            boundingBox = BoundingBoxExt.Empty;

            for (int index = 0; index < bones.Length; index++)
            {
                var nodeIndex = bones[index].NodeIndex;

                // Compute bone matrix
                Matrix.Multiply(ref bones[index].LinkToMeshMatrix, ref hierarchyUpdater.NodeTransformations[nodeIndex].WorldMatrix, out boneMatrices[index]);

                // Calculate and extend bounding box for each bone
                // TODO: Move runtime bounding box into ModelViewHierarchyUpdater?

                // Fast AABB transform: http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
                // Compute transformed AABB (by world)
                var boundingBoxExt = bindPoseBoundingBox;
                boundingBoxExt.Transform(boneMatrices[index]);
                BoundingBoxExt.Merge(ref boundingBox, ref boundingBoxExt, out boundingBox);
            }

            // Upload bones
            renderMesh.Parameters.Set(TransformationSkinningKeys.BlendMatrixArray, boneMatrices, 0, bones.Length);
        }
    }
}