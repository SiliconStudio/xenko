// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Performs blend matrix skinning.
    /// </summary>
    public static class MeshSkinningUpdater
    {
        [ThreadStatic]
        private static Matrix[] staticBoneMatrices;

        public static void Update(ModelViewHierarchyUpdater hierarchy, RenderModel renderModel, int slot)
        {
            var boneMatrices = staticBoneMatrices;

            var meshes = renderModel.RenderMeshesList[slot];
            {
                if (meshes == null)
                {
                    return;
                }

                foreach (var renderMesh in meshes)
                {
                    var mesh = renderMesh.Mesh;
                    var skinning = mesh.Skinning;
                    if (skinning == null)
                        continue;

                    var bones = skinning.Bones;

                    // Make sure there is enough spaces in boneMatrices
                    if (boneMatrices == null || bones.Length > boneMatrices.Length)
                        staticBoneMatrices = boneMatrices = new Matrix[bones.Length];

                    for (int index = 0; index < bones.Length; index++)
                    {
                        var nodeIndex = bones[index].NodeIndex;

                        // Compute bone matrix
                        Matrix.Multiply(ref bones[index].LinkToMeshMatrix, ref hierarchy.NodeTransformations[nodeIndex].WorldMatrix, out boneMatrices[index]);
                    }

                    // Upload bones
                    renderMesh.Parameters.Set(TransformationSkinningKeys.BlendMatrixArray, boneMatrices, 0, bones.Length);
                }
            }
        }
    }
}