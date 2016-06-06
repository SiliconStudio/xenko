// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    public partial class ImportModelCommand
    {
        public float ScaleImport { get; set; }
        public bool Allow32BitIndex { get; set; }
        public bool AllowUnsignedBlendIndices { get; set; }
        public List<ModelMaterial> Materials { get; set; }
        public string EffectName { get; set; }
        public bool TessellationAEN { get; set; }

        private object ExportModel(ICommandContext commandContext, ContentManager contentManager)
        {
            // Read from model file
            var modelSkeleton = LoadSkeleton(commandContext, contentManager); // we get model skeleton to compare it to real skeleton we need to map to
            var model = LoadModel(commandContext, contentManager);

            // Apply materials
            foreach (var modelMaterial in Materials)
            {
                if (modelMaterial.MaterialInstance?.Material == null)
                {
                    commandContext.Logger.Warning($"The material [{modelMaterial.Name}] is null in the list of materials.");
                    continue;
                }
                model.Materials.Add(modelMaterial.MaterialInstance);
            }

            model.BoundingBox = BoundingBox.Empty;

            foreach (var mesh in model.Meshes)
            {
                if (TessellationAEN)
                {
                    // TODO: Generate AEN model view
                    commandContext.Logger.Error("TessellationAEN is not supported in {0}", ContextAsString);
                }
            }

            SkeletonMapping skeletonMapping;

            Skeleton skeleton;
            if (SkeletonUrl != null)
            {
                // Load skeleton and process it
                skeleton = contentManager.Load<Skeleton>(SkeletonUrl);

                // Assign skeleton to model
                model.Skeleton = AttachedReferenceManager.CreateProxyObject<Skeleton>(Guid.Empty, SkeletonUrl);
            }
            else
            {
                skeleton = null;

            }

            skeletonMapping = new SkeletonMapping(skeleton, modelSkeleton);

            // Refresh skeleton updater with model skeleton
            var hierarchyUpdater = new SkeletonUpdater(modelSkeleton);
            hierarchyUpdater.UpdateMatrices();

            // Move meshes in the new nodes
            foreach (var mesh in model.Meshes)
            {
                // Check if there was a remap using model skeleton
                if (skeletonMapping.SourceToSource[mesh.NodeIndex] != mesh.NodeIndex)
                {
                    // Transform vertices
                    var transformationMatrix = CombineMatricesFromNodeIndices(hierarchyUpdater.NodeTransformations, skeletonMapping.SourceToSource[mesh.NodeIndex], mesh.NodeIndex);
                    mesh.Draw.VertexBuffers[0].TransformBuffer(ref transformationMatrix);

                    // Check if geometry is inverted, to know if we need to reverse winding order
                    // TODO: What to do if there is no index buffer? We should create one... (not happening yet)
                    if (mesh.Draw.IndexBuffer == null)
                        throw new InvalidOperationException();

                    Matrix rotation;
                    Vector3 scale, translation;
                    if (transformationMatrix.Decompose(out scale, out rotation, out translation)
                        && scale.X * scale.Y * scale.Z < 0)
                    {
                        mesh.Draw.ReverseWindingOrder();
                    }
                }

                // Update new node index using real asset skeleton
                mesh.NodeIndex = skeletonMapping.SourceToTarget[mesh.NodeIndex];
            }

            // Merge meshes with same parent nodes, material and skinning
            var meshesByNodes = model.Meshes.GroupBy(x => x.NodeIndex).ToList();

            foreach (var meshesByNode in meshesByNodes)
            {
                // This logic to detect similar material is kept from old code; this should be reviewed/improved at some point
                foreach (var meshesPerDrawCall in meshesByNode.GroupBy(x => x,
                    new AnonymousEqualityComparer<Mesh>((x, y) =>
                    x.MaterialIndex == y.MaterialIndex // Same material
                    && ArrayExtensions.ArraysEqual(x.Skinning?.Bones, y.Skinning?.Bones) // Same bones
                    && CompareParameters(model, x, y) // Same parameters
                    && CompareShadowOptions(model, x, y), // Same shadow parameters
                    x => 0)).ToList())
                {
                    if (meshesPerDrawCall.Count() == 1)
                    {
                        // Nothing to group, skip to next entry
                        continue;
                    }

                    // Remove old meshes
                    foreach (var mesh in meshesPerDrawCall)
                    {
                        model.Meshes.Remove(mesh);
                    }

                    // Add new combined mesh(es)
                    var baseMesh = meshesPerDrawCall.First();
                    var newMeshList = meshesPerDrawCall.Select(x => x.Draw).ToList().GroupDrawData(Allow32BitIndex);

                    foreach (var generatedMesh in newMeshList)
                    {
                        model.Meshes.Add(new Mesh(generatedMesh, baseMesh.Parameters)
                        {
                            MaterialIndex = baseMesh.MaterialIndex,
                            Name = baseMesh.Name,
                            Draw = generatedMesh,
                            NodeIndex = baseMesh.NodeIndex,
                            Skinning = baseMesh.Skinning,
                        });
                    }
                }
            }

            // Remap skinning
            foreach (var skinning in model.Meshes.Select(x => x.Skinning).Where(x => x != null).Distinct())
            {
                // Update node mapping
                // Note: we only remap skinning matrices, but we could directly remap skinning bones instead
                for (int i = 0; i < skinning.Bones.Length; ++i)
                {
                    var nodeIndex = skinning.Bones[i].NodeIndex;
                    var newNodeIndex = skeletonMapping.SourceToSource[nodeIndex];

                    skinning.Bones[i].NodeIndex = skeletonMapping.SourceToTarget[nodeIndex];

                    // If it was remapped, we also need to update matrix
                    if (newNodeIndex != nodeIndex)
                    {
                        var transformationMatrix = CombineMatricesFromNodeIndices(hierarchyUpdater.NodeTransformations, newNodeIndex, nodeIndex);
                        skinning.Bones[i].LinkToMeshMatrix = Matrix.Multiply(skinning.Bones[i].LinkToMeshMatrix, transformationMatrix);
                    }
                }
            }

            // split the meshes if necessary
            model.Meshes = SplitExtensions.SplitMeshes(model.Meshes, Allow32BitIndex);

            // Refresh skeleton updater with asset skeleton
            hierarchyUpdater = new SkeletonUpdater(skeleton);
            hierarchyUpdater.UpdateMatrices();

            // bounding boxes
            var modelBoundingBox = model.BoundingBox;
            var modelBoundingSphere = model.BoundingSphere;
            foreach (var mesh in model.Meshes)
            {
                var vertexBuffers = mesh.Draw.VertexBuffers;
                if (vertexBuffers.Length > 0)
                {
                    // Compute local mesh bounding box (no node transformation)
                    Matrix matrix = Matrix.Identity;
                    mesh.BoundingBox = vertexBuffers[0].ComputeBounds(ref matrix, out mesh.BoundingSphere);

                    // Compute model bounding box (includes node transformation)
                    hierarchyUpdater.GetWorldMatrix(mesh.NodeIndex, out matrix);
                    BoundingSphere meshBoundingSphere;
                    var meshBoundingBox = vertexBuffers[0].ComputeBounds(ref matrix, out meshBoundingSphere);
                    BoundingBox.Merge(ref modelBoundingBox, ref meshBoundingBox, out modelBoundingBox);
                    BoundingSphere.Merge(ref modelBoundingSphere, ref meshBoundingSphere, out modelBoundingSphere);
                }

                // TODO: temporary Always try to compact
                mesh.Draw.CompactIndexBuffer();
            }
            model.BoundingBox = modelBoundingBox;
            model.BoundingSphere = modelBoundingSphere;

            // merges all the Draw VB and IB together to produce one final VB and IB by entity.
            var sizeVertexBuffer = model.Meshes.SelectMany(x => x.Draw.VertexBuffers).Select(x => x.Buffer.GetSerializationData().Content.Length).Sum();
            var sizeIndexBuffer = 0;
            foreach (var x in model.Meshes)
            {
                // Let's be aligned (if there was 16bit indices before, we might be off)
                if (x.Draw.IndexBuffer.Is32Bit && sizeIndexBuffer % 4 != 0)
                    sizeIndexBuffer += 2;

                sizeIndexBuffer += x.Draw.IndexBuffer.Buffer.GetSerializationData().Content.Length;
            }
            var vertexBuffer = new BufferData(BufferFlags.VertexBuffer, new byte[sizeVertexBuffer]);
            var indexBuffer = new BufferData(BufferFlags.IndexBuffer, new byte[sizeIndexBuffer]);

            // Note: reusing same instance, to avoid having many VB with same hash but different URL
            var vertexBufferSerializable = vertexBuffer.ToSerializableVersion();
            var indexBufferSerializable = indexBuffer.ToSerializableVersion();

            var vertexBufferNextIndex = 0;
            var indexBufferNextIndex = 0;
            foreach (var drawMesh in model.Meshes.Select(x => x.Draw))
            {
                // the index buffer
                var oldIndexBuffer = drawMesh.IndexBuffer.Buffer.GetSerializationData().Content;

                // Let's be aligned (if there was 16bit indices before, we might be off)
                if (drawMesh.IndexBuffer.Is32Bit && indexBufferNextIndex % 4 != 0)
                    indexBufferNextIndex += 2;

                Array.Copy(oldIndexBuffer, 0, indexBuffer.Content, indexBufferNextIndex, oldIndexBuffer.Length);

                drawMesh.IndexBuffer = new IndexBufferBinding(indexBufferSerializable, drawMesh.IndexBuffer.Is32Bit, drawMesh.IndexBuffer.Count, indexBufferNextIndex);

                indexBufferNextIndex += oldIndexBuffer.Length;

                // the vertex buffers
                for (int index = 0; index < drawMesh.VertexBuffers.Length; index++)
                {
                    var vertexBufferBinding = drawMesh.VertexBuffers[index];
                    var oldVertexBuffer = vertexBufferBinding.Buffer.GetSerializationData().Content;

                    Array.Copy(oldVertexBuffer, 0, vertexBuffer.Content, vertexBufferNextIndex, oldVertexBuffer.Length);

                    drawMesh.VertexBuffers[index] = new VertexBufferBinding(vertexBufferSerializable, vertexBufferBinding.Declaration, vertexBufferBinding.Count, vertexBufferBinding.Stride,
                        vertexBufferNextIndex);

                    vertexBufferNextIndex += oldVertexBuffer.Length;
                }
            }

            // Convert to Entity
            return model;
        }
    }
}
