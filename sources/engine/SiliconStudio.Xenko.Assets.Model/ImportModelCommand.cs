// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Assets.Model
{
    public abstract class ImportModelCommand : SingleFileImportCommand
    {
        private delegate bool SameGroup(Rendering.Model model, Mesh baseMesh, Mesh newMesh);

        /// <inheritdoc/>
        public override IEnumerable<Tuple<string, string>> TagList { get { yield return Tuple.Create("Texture", "Value of the TextureTag property"); } }

        private static int spawnedFbxCommands;

        public string ExportType { get; set; }
        public bool TessellationAEN { get; set; }
        public string EffectName { get; set; }
        public AnimationRepeatMode AnimationRepeatMode { get; set; }

        public List<ModelMaterial> Materials { get; set; }

        public List<KeyValuePair<string, bool>> SkeletonNodesWithPreserveInfo { get; set; }

        public bool Allow32BitIndex { get; set; }
        public bool AllowUnsignedBlendIndices { get; set; }

        public float ScaleImport { get; set; }

        public string SkeletonUrl { get; set; }

        public static ImportModelCommand Create(string extension)
        {
            if (ImportFbxCommand.IsSupportingExtensions(extension))
                return new ImportFbxCommand();
            if (ImportAssimpCommand.IsSupportingExtensions(extension))
                return new ImportAssimpCommand();

            return null;
        }

        protected ImportModelCommand()
        {
            // Set default values
            ExportType = "model";
            AnimationRepeatMode = AnimationRepeatMode.LoopInfinite;
            ScaleImport = 1.0f;
        }

        private string ContextAsString => $"model [{Location}] from import [{SourcePath}]";

        class SkeletonMapping
        {
            public int[] ModelToSkeleton;
            public int[] SkeletonToModel;
            public int[] ModelToModel;

            public SkeletonMapping(Skeleton skeleton, Skeleton modelSkeleton)
            {
                ModelToSkeleton = new int[modelSkeleton.Nodes.Length]; // model => skeleton mapping
                ModelToModel = new int[modelSkeleton.Nodes.Length]; // model => model mapping
                SkeletonToModel = new int[skeleton.Nodes.Length]; // skeleton => model mapping
                for (int i = 0; i < SkeletonToModel.Length; ++i)
                    SkeletonToModel[i] = -1;

                // Build mapping from model to actual skeleton
                for (int modelIndex = 0; modelIndex < modelSkeleton.Nodes.Length; ++modelIndex)
                {
                    var node = modelSkeleton.Nodes[modelIndex];
                    var parentModelIndex = node.ParentIndex;

                    // Find matching node in skeleton (or map to best parent)
                    var skeletonIndex = skeleton.Nodes.IndexOf(x => x.Name == node.Name);

                    if (skeletonIndex == -1)
                    {
                        // Nothing match, remap to parent node
                        ModelToSkeleton[modelIndex] = parentModelIndex != -1 ? ModelToSkeleton[parentModelIndex] : 0;
                        continue;
                    }

                    // TODO: Check hierarchy for inconsistencies

                    // Name match
                    ModelToSkeleton[modelIndex] = skeletonIndex;
                    SkeletonToModel[skeletonIndex] = modelIndex;
                }

                for (int modelIndex = 0; modelIndex < modelSkeleton.Nodes.Length; ++modelIndex)
                {
                    ModelToModel[modelIndex] = SkeletonToModel[ModelToSkeleton[modelIndex]];
                }
            }
        }

        /// <summary>
        /// The method to override containing the actual command code. It is called by the <see cref="DoCommand" /> function
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>Task{ResultStatus}.</returns>
        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new AssetManager();

            while (Interlocked.Increment(ref spawnedFbxCommands) >= 2)
            {
                Interlocked.Decrement(ref spawnedFbxCommands);
                await Task.Delay(1, CancellationToken);
            }

            try
            {
                object exportedObject;

                if (ExportType == "animation")
                {
                    // Read from model file
                    var modelSkeleton = LoadSkeleton(commandContext, assetManager); // we get model skeleton to compare it to real skeleton we need to map to
                    var animationClips = LoadAnimation(commandContext, assetManager);
                    AnimationClip animationClip = null;

                    if (animationClips.Count > 0)
                    {
                        animationClip = new AnimationClip();

                        // Load asset reference skeleton
                        if (SkeletonUrl != null)
                        {
                            var skeleton = assetManager.Load<Skeleton>(SkeletonUrl);

                            // Process missing nodes
                            foreach (var nodeAnimationClip in animationClips)
                            {
                                var nodeName = nodeAnimationClip.Key;
                                var nodeIndex = skeleton.Nodes.IndexOf(x => x.Name == nodeName);

                                // Node doesn't exist in skeleton? skip it
                                if (nodeIndex == -1)
                                    continue;

                                foreach (var channel in nodeAnimationClip.Value.Channels)
                                {
                                    var curve = nodeAnimationClip.Value.Curves[channel.Value.CurveIndex];
                                    animationClip.AddCurve($"[SiliconStudio.Xenko.Engine.ModelComponent,SiliconStudio.Xenko.Engine.Key].Skeleton.NodeTransformations[{nodeIndex}]." + channel.Key, curve);
                                }

                                // Take max of durations
                                if (animationClip.Duration < nodeAnimationClip.Value.Duration)
                                    animationClip.Duration = nodeAnimationClip.Value.Duration;
                            }

                            // Resolve nodes

                        }
                        else
                        {
                            // No skeleton, map root node only
                            throw new NotImplementedException();
                        }
                    }

                    exportedObject = animationClip;
                    if (animationClip == null)
                    {
                        commandContext.Logger.Info("File {0} has an empty animation.", SourcePath);
                    }
                    else
                    {
                        if (animationClip.Duration.Ticks == 0)
                        {
                            commandContext.Logger.Warning("File {0} has a 0 tick long animation.", SourcePath);
                        }
                        
                        // Optimize and set common parameters
                        animationClip.RepeatMode = AnimationRepeatMode;
                        animationClip.Optimize();
                    }
                }
                else if (ExportType == "skeleton")
                {
                    var skeleton = LoadSkeleton(commandContext, assetManager);

                    var modelNodes = new HashSet<string>(skeleton.Nodes.Select(x => x.Name));
                    var skeletonNodes = new HashSet<string>(SkeletonNodesWithPreserveInfo.Select(x => x.Key));

                    // List missing nodes on both sides, to display warnings
                    var missingNodesInModel = new HashSet<string>(skeletonNodes);
                    missingNodesInModel.ExceptWith(modelNodes);

                    var missingNodesInAsset = new HashSet<string>(modelNodes);
                    missingNodesInAsset.ExceptWith(skeletonNodes);

                    if (missingNodesInAsset.Count > 0)
                        commandContext.Logger.Warning($"{missingNodesInAsset.Count} node(s) were present in model [{SourcePath}] but not in asset [{Location}], please reimport: {string.Join(", ", missingNodesInAsset)}");

                    if (missingNodesInModel.Count > 0)
                        commandContext.Logger.Warning($"{missingNodesInModel.Count} node(s) were present in asset [{Location}] but not in model [{SourcePath}], please reimport: {string.Join(", ", missingNodesInModel)}");

                    // Build node mapping to expected structure
                    var optimizedNodes = new HashSet<string>(SkeletonNodesWithPreserveInfo.Where(x => !x.Value).Select(x => x.Key));

                    // Refresh skeleton updater with loaded skeleton (to be able to compute matrices)
                    var hierarchyUpdater = new SkeletonUpdater(skeleton);
                    hierarchyUpdater.UpdateMatrices();

                    // Removed optimized nodes
                    var filteredSkeleton = new Skeleton { Nodes = skeleton.Nodes.Where(x => !optimizedNodes.Contains(x.Name)).ToArray() };

                    // Fix parent indices (since we removed some nodes)
                    for (int i = 0; i < filteredSkeleton.Nodes.Length; ++i)
                    {
                        var parentIndex = filteredSkeleton.Nodes[i].ParentIndex;
                        if (parentIndex != -1)
                        {
                            // Find appropriate parent to map to
                            var newParentIndex = -1;
                            while (newParentIndex == -1 && parentIndex != -1)
                            {
                                var nodeName = skeleton.Nodes[parentIndex].Name;
                                parentIndex = skeleton.Nodes[parentIndex].ParentIndex;
                                newParentIndex = filteredSkeleton.Nodes.IndexOf(x => x.Name == nodeName);
                            }
                            filteredSkeleton.Nodes[i].ParentIndex = newParentIndex;
                        }
                    }

                    // Generate mapping
                    var skeletonMapping = new SkeletonMapping(filteredSkeleton, skeleton);

                    // Children of remapped nodes need to have their matrices updated
                    for (int i = 0; i < skeleton.Nodes.Length; ++i)
                    {
                        var node = skeleton.Nodes[i];
                        var filteredIndex = skeletonMapping.ModelToSkeleton[i];

                        var oldParentIndex = node.ParentIndex;

                        if (oldParentIndex != -1 && skeletonMapping.ModelToModel[oldParentIndex] != oldParentIndex)
                        {
                            // Compute matrix for intermediate missing nodes
                            var transformMatrix = CombineMatricesFromNodeIndices(hierarchyUpdater.NodeTransformations, skeletonMapping.ModelToModel[oldParentIndex], oldParentIndex);
                            var localMatrix = hierarchyUpdater.NodeTransformations[i].LocalMatrix;

                            // Combine it with local matrix, and use that instead in the new skeleton; resulting node should be same position as before optimized nodes were removed
                            localMatrix = Matrix.Multiply(localMatrix, transformMatrix);
                            localMatrix.Decompose(out filteredSkeleton.Nodes[filteredIndex].Transform.Scaling, out filteredSkeleton.Nodes[filteredIndex].Transform.Rotation, out filteredSkeleton.Nodes[filteredIndex].Transform.Translation);
                        }
                    }

                    exportedObject = filteredSkeleton;
                }
                else if (ExportType == "model")
                {
                    // Read from model file
                    var modelSkeleton = LoadSkeleton(commandContext, assetManager); // we get model skeleton to compare it to real skeleton we need to map to
                    var model = LoadModel(commandContext, assetManager);

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

                    bool hasErrors = false;
                    foreach (var mesh in model.Meshes)
                    {
                        if (TessellationAEN)
                        {
                            // TODO: Generate AEN model view
                            commandContext.Logger.Error("TessellationAEN is not supported in {0}", ContextAsString);
                            hasErrors = true;
                        }
                    }

                    SkeletonMapping skeletonMapping;

                    Skeleton skeleton;
                    if (SkeletonUrl != null)
                    {
                        // Load skeleton and process it
                        skeleton = assetManager.Load<Skeleton>(SkeletonUrl);
                        skeletonMapping = new SkeletonMapping(skeleton, modelSkeleton);

                        // Assign skeleton to model
                        model.Skeleton = AttachedReferenceManager.CreateSerializableVersion<Skeleton>(Guid.Empty, SkeletonUrl);
                    }
                    else
                    {
                        throw new NotImplementedException();
                        //skeleton = null;

                        //// No skeleton, we can compact everything
                        //for (int i = 0; i < modelSkeleton.Nodes.Length; ++i)
                        //{
                        //    // Map everything to root node
                        //    modelToSkeleton[i] = 0;
                        //    modelToModel[i] = 0;
                        //}
                    }

                    // Refresh skeleton updater with model skeleton
                    var hierarchyUpdater = new SkeletonUpdater(modelSkeleton);
                    hierarchyUpdater.UpdateMatrices();

                    // Move meshes in the new nodes
                    foreach (var mesh in model.Meshes)
                    {
                        // Check if there was a remap using model skeleton
                        if (skeletonMapping.ModelToModel[mesh.NodeIndex] != mesh.NodeIndex)
                        {
                            // Transform vertices
                            var transformationMatrix = CombineMatricesFromNodeIndices(hierarchyUpdater.NodeTransformations, skeletonMapping.ModelToModel[mesh.NodeIndex], mesh.NodeIndex);
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
                        mesh.NodeIndex = skeletonMapping.ModelToSkeleton[mesh.NodeIndex];
                    }

                    // Merge meshes with same parent nodes, material and skinning
                    var meshesByNodes = model.Meshes.GroupBy(x => x.NodeIndex).ToList();

                    foreach (var meshesByNode in meshesByNodes)
                    {
                        foreach (var meshesWithSameMaterial in meshesByNode.GroupBy(x => x,
                                    new AnonymousEqualityComparer<Mesh>((x, y) => ArrayExtensions.ArraysEqual(x.Skinning?.Bones, y.Skinning?.Bones) && CompareParameters(model, x, y) && CompareShadowOptions(model, x, y), x => 0)).ToList())
                        {
                            if (meshesWithSameMaterial.Count() == 1)
                            {
                                // Nothing to group, skip to next entry
                                continue;
                            }

                            // Remove old meshes
                            foreach (var mesh in meshesWithSameMaterial)
                            {
                                model.Meshes.Remove(mesh);
                            }

                            // Add new combined mesh(es)
                            var baseMesh = meshesWithSameMaterial.First();
                            var newMeshList = meshesWithSameMaterial.Select(x => x.Draw).ToList().GroupDrawData(Allow32BitIndex);

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
                            var newNodeIndex = skeletonMapping.ModelToModel[nodeIndex];

                            skinning.Bones[i].NodeIndex = skeletonMapping.ModelToSkeleton[nodeIndex];

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

                            drawMesh.VertexBuffers[index] = new VertexBufferBinding(vertexBufferSerializable, vertexBufferBinding.Declaration, vertexBufferBinding.Count, vertexBufferBinding.Stride, vertexBufferNextIndex);

                            vertexBufferNextIndex += oldVertexBuffer.Length;
                        }
                    }


                    // If there were any errors while importing models
                    if (hasErrors)
                    {
                        return ResultStatus.Failed;
                    }

                    // Convert to Entity
                    exportedObject = model;
                }
                else
                {
                    commandContext.Logger.Error("Unknown export type [{0}] {1}", ExportType, ContextAsString);
                    return ResultStatus.Failed;
                }

                if (exportedObject != null)
                    assetManager.Save(Location, exportedObject);

                commandContext.Logger.Verbose("The {0} has been successfully imported.", ContextAsString);

                return ResultStatus.Successful;
            }
            catch (Exception ex)
            {
                commandContext.Logger.Error("Unexpected error while importing {0}", ex, ContextAsString);
                return ResultStatus.Failed;
            }
            finally
            {
                Interlocked.Decrement(ref spawnedFbxCommands);
            }
        }

        /// <summary>
        /// Refine the mesh groups based on the result of the delegate test method.
        /// </summary>
        /// <param name="meshList">The list of mesh groups.</param>
        /// <param name="sameGroupDelegate">The test delegate.</param>
        /// <returns>The new list of mesh groups.</returns>
        private List<GroupList<int, Mesh>> RefineGroups(Rendering.Model model, List<GroupList<int, Mesh>> meshList, SameGroup sameGroupDelegate)
        {
            var finalGroups = new List<GroupList<int, Mesh>>();
            foreach (var meshGroup in meshList)
            {
                var updatedGroups = new List<GroupList<int, Mesh>>();
                foreach (var mesh in meshGroup)
                {
                    var createNewGroup = true;
                    foreach (var sameParamsMeshes in updatedGroups)
                    {
                        if (sameGroupDelegate(model, sameParamsMeshes[0], mesh))
                        {
                            sameParamsMeshes.Add(mesh);
                            createNewGroup = false;
                            break;
                        }
                    }

                    if (createNewGroup)
                    {
                        var newGroup = new GroupList<int, Mesh> { Key = meshGroup.Key };
                        newGroup.Add(mesh);
                        updatedGroups.Add(newGroup);
                    }
                }
                finalGroups.AddRange(updatedGroups);
            }
            return finalGroups;
        }

        /// <summary>
        /// Create groups of mergeable meshes.
        /// </summary>
        /// <param name="model">The current model.</param>
        /// <param name="index">The index of the currently visited node.</param>
        /// <param name="nodeBlackList">List of the nodes that should be kept.</param>
        /// <param name="meshes">The meshes and their node index.</param>
        /// <param name="finalLists">List of mergeable meshes and their root node.</param>
        /// <returns>A list of mergeable meshes in progress.</returns>
        private Dictionary<int, List<Mesh>> GroupFromIndex(Rendering.Model model, int index, HashSet<int> nodeBlackList, List<Mesh> meshes, List<GroupList<int, Mesh>> finalLists)
        {
            var children = GetChildren(model.Skeleton.Nodes, index);
            
            var materialGroups = new Dictionary<int, List<Mesh>>();

            // Get the group from each child
            foreach (var child in children)
            {
                var newMaterialGroups = GroupFromIndex(model, child, nodeBlackList, meshes, finalLists);

                foreach (var group in newMaterialGroups)
                {
                    if (!materialGroups.ContainsKey(group.Key))
                        materialGroups.Add(group.Key, new List<Mesh>());
                    materialGroups[group.Key].AddRange(group.Value);
                }
            }

            // Add the current node if it has a mesh
            foreach (var nodeMesh in meshes.Where(x => x.NodeIndex == index))
            {
                var matId = nodeMesh.MaterialIndex;
                if (!materialGroups.ContainsKey(matId))
                    materialGroups.Add(matId, new List<Mesh>());
                materialGroups[matId].Add(nodeMesh);
            }

            // Store the generated list as final if the node should be kept
            if (nodeBlackList.Contains(index) || index == 0)
            {
                foreach (var materialGroup in materialGroups)
                {
                    var groupList = new GroupList<int, Mesh>();
                    groupList.Key = index;
                    groupList.AddRange(materialGroup.Value);
                    finalLists.Add(groupList);
                }
                materialGroups.Clear();
            }

            return materialGroups;
        }

        /// <summary>
        /// Get the transformation matrix to go from rootIndex to index.
        /// </summary>
        /// <param name="nodes">The nodes containing the local matrices.</param>
        /// <param name="rootIndex">The root index.</param>
        /// <param name="index">The current index.</param>
        /// <returns>The matrix at this index.</returns>
        private Matrix CombineMatricesFromNodeIndices(ModelNodeTransformation[] nodes, int rootIndex, int index)
        {
            if (index == -1 || index == rootIndex)
                return Matrix.Identity;

            var result = nodes[index].LocalMatrix;

            if (index != rootIndex)
            {
                var topMatrix = CombineMatricesFromNodeIndices(nodes, rootIndex, nodes[index].ParentIndex);
                result = Matrix.Multiply(result, topMatrix);
            }

            return result;
        }
        
        /// <summary>
        /// Get the children of the requested node.
        /// </summary>
        /// <param name="hierarchy">The node hierarchy.</param>
        /// <param name="index">The current node index.</param>
        /// <returns>A list of all the indices of the children.</returns>
        private List<int> GetChildren(ModelNodeDefinition[] hierarchy, int index)
        {
            var result = new List<int>();
            for (var i = 0; i < hierarchy.Length; ++i)
            {
                if (hierarchy[i].ParentIndex == index)
                    result.Add(i);
            }
            return result;
        }

        protected abstract Rendering.Model LoadModel(ICommandContext commandContext, AssetManager assetManager);

        protected abstract Dictionary<string, AnimationClip> LoadAnimation(ICommandContext commandContext, AssetManager assetManager);

        protected abstract Skeleton LoadSkeleton(ICommandContext commandContext, AssetManager assetManager);

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            writer.SerializeExtended(this, ArchiveMode.Serialize);
        }

        protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
        {
            // Skeleton is a compile time dependency
            if (SkeletonUrl != null)
                yield return new ObjectUrl(UrlType.Content, SkeletonUrl);

            yield return new ObjectUrl(UrlType.File, SourcePath);
        }

        /// <summary>
        /// Parses a shader source definition
        /// </summary>
        /// <param name="shaderSource">The shader source.</param>
        /// <returns>an instance of ShaderSource.</returns>
        protected static ShaderSource ParseShaderSource(string shaderSource)
        {
            var sources = shaderSource.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

            if (sources.Length == 1)
            {
                return new ShaderClassSource(sources[0]);
            }

            var mixin = new ShaderMixinSource();
            foreach (var source in sources)
            {
                mixin.Mixins.Add(new ShaderClassSource(source));
            }

            return mixin;
        }

        /// <summary>
        /// Compares the parameters of the two meshes.
        /// </summary>
        /// <param name="baseMesh">The base mesh.</param>
        /// <param name="newMesh">The mesh to compare.</param>
        /// <param name="extra">Unused parameter.</param>
        /// <returns>True if all the parameters are the same, false otherwise.</returns>
        private static bool CompareParameters(Rendering.Model model, Mesh baseMesh, Mesh newMesh)
        {
            var localParams = baseMesh.Parameters;
            if (localParams == null && newMesh.Parameters == null)
                return true;
            if (localParams == null || newMesh.Parameters == null)
                return false;
            return AreCollectionsEqual(localParams, newMesh.Parameters);
        }
        
        /// <summary>
        /// Compares the shadow options between the two meshes.
        /// </summary>
        /// <param name="baseMesh">The base mesh.</param>
        /// <param name="newMesh">The mesh to compare.</param>
        /// <param name="extra">Unused parameter.</param>
        /// <returns>True if the options are the same, false otherwise.</returns>
        private static bool CompareShadowOptions(Rendering.Model model, Mesh baseMesh, Mesh newMesh)
        {
            // TODO: Check is Model the same for the two mesh?
            var material1 = model.Materials.GetItemOrNull(baseMesh.MaterialIndex);
            var material2 = model.Materials.GetItemOrNull(newMesh.MaterialIndex);

            return material1 == material2 || (material1 != null && material2 != null && material1.IsShadowCaster == material2.IsShadowCaster &&
                material1.IsShadowReceiver == material2.IsShadowReceiver);
        }

        /// <summary>
        /// Compares the value behind a key in two ParameterCollection.
        /// </summary>
        /// <param name="parameters0">The first ParameterCollection.</param>
        /// <param name="parameters1">The second ParameterCollection.</param>
        /// <param name="key">The ParameterKey.</param>
        /// <returns>True</returns>
        private static bool CompareKeyValue<T>(ParameterCollection parameters0, ParameterCollection parameters1, ParameterKey<T> key)
        {
            var value0 = parameters0 != null && parameters0.ContainsKey(key) ? parameters0[key] : key.DefaultValueMetadataT.DefaultValue;
            var value1 = parameters1 != null && parameters1.ContainsKey(key) ? parameters1[key] : key.DefaultValueMetadataT.DefaultValue;
            return value0 == value1;
        }

        /// <summary>
        /// Test if two ParameterCollection are equal
        /// </summary>
        /// <param name="parameters0">The first ParameterCollection.</param>
        /// <param name="parameters1">The second ParameterCollection.</param>
        /// <returns>True if the collections are the same, false otherwise.</returns>
        private static bool AreCollectionsEqual(ParameterCollection parameters0, ParameterCollection parameters1)
        {
            bool result = true;
            foreach (var paramKey in parameters0)
            {
                result &= parameters1.ContainsKey(paramKey.Key) && parameters1[paramKey.Key].Equals(paramKey.Value);
            }
            foreach (var paramKey in parameters1)
            {
                result &= parameters0.ContainsKey(paramKey.Key) && parameters0[paramKey.Key].Equals(paramKey.Value);
            }
            return result;
        }

        public override string ToString()
        {
            return (SourcePath ?? "[File]") + (ExportType != null ? " (" + ExportType + ")" : "") + " > " + (Location ?? "[Location]");
        }

    }

    /// <summary>
    /// A implementation of IGrouping.
    /// </summary>
    /// <typeparam name="TK">Key type.</typeparam>
    /// <typeparam name="T">Element type.</typeparam>
    public class GroupList<TK,T> : List<T>, IGrouping<TK,T>
    {
        public TK Key { get; set; }
    }
}