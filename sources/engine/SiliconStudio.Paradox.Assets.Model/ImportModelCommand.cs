// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Model
{
    public abstract class ImportModelCommand : SingleFileImportCommand
    {
        private delegate bool SameGroup(MeshData baseMesh, MeshData newMesh);

        /// <inheritdoc/>
        public override IEnumerable<Tuple<string, string>> TagList { get { yield return Tuple.Create("Texture", "Value of the TextureTag property"); } }

        private static int spawnedFbxCommands;
        protected TagSymbol TextureTagSymbol;

        public string TextureTag { get; set; }
        
        public string ExportType { get; set; }
        public bool TessellationAEN { get; set; }
        public string EffectName { get; set; }
        public AnimationRepeatMode AnimationRepeatMode { get; set; }

        public Dictionary<string, Tuple<Guid, string>> Materials { get; set; }
        public Dictionary<string, Tuple<Guid, string>> Lightings { get; set; }
        public Dictionary<string, ParameterCollectionData> Parameters;

        public bool Compact { get; set; }
        public List<string> PreservedNodes { get; set; }

        public bool Allow32BitIndex { get; set; }
        public bool AllowUnsignedBlendIndices { get; set; }
        public Vector3 ViewDirectionForTransparentZSort { get; set; }

        protected ImportModelCommand()
        {
            // Set default values
            ExportType = "model";
            TextureTag = "fbx-texture";
            TextureTagSymbol = RegisterTag("Texture", () => TextureTag);
            AnimationRepeatMode = AnimationRepeatMode.LoopInfinite;
        }

        private string ContextAsString
        {
            get
            {
                return string.Format("model [{0}] from import [{1}]", Location, SourcePath);
            }
        }

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
                    var animationClip = LoadAnimation(commandContext, assetManager);
                    exportedObject = animationClip;
                    if (animationClip == null)
                    {
                        commandContext.Logger.Info("File {0} has an empty animation.", SourcePath);
                    }
                    else if (animationClip.Duration.Ticks == 0)
                    {
                        commandContext.Logger.Warning("File {0} has a 0 tick long animation.", SourcePath);
                    }
                    else
                    {
                        animationClip.RepeatMode = AnimationRepeatMode;
                        animationClip.Optimize();
                    }
                }
                else if (ExportType == "model")
                {
                    // Read from model file
                    var model = LoadModel(commandContext, assetManager);

                    model.BoundingBox = BoundingBox.Empty;
                    var hierarchyUpdater = new ModelViewHierarchyUpdater(model.Hierarchy.Nodes);
                    hierarchyUpdater.UpdateMatrices();

                    bool hasErrors = false;
                    foreach (var mesh in model.Meshes)
                    {
                        if (TessellationAEN)
                        {
                            // TODO: Generate AEN model view
                            commandContext.Logger.Error("TessellationAEN is not supported in {0}", ContextAsString);
                            hasErrors = true;
                        }

                        if (Materials.ContainsKey(mesh.Name))
                        {
                            // set the material
                            var materialReference = Materials[mesh.Name];
                            mesh.Material = new ContentReference<MaterialData>(materialReference.Item1, materialReference.Item2);
                        }
                        else
                        {
                            commandContext.Logger.Warning("Mesh material [{0}] was not found in {1}", mesh.Name, ContextAsString);
                        }

                        // set the parameters
                        if (Parameters.ContainsKey(mesh.Name) && Parameters[mesh.Name] != null)
                        {
                            if (mesh.Parameters == null)
                                mesh.Parameters = new ParameterCollectionData();
                            foreach (var keyValue in Parameters[mesh.Name])
                                mesh.Parameters.Set(keyValue.Key, keyValue.Value);
                        }

                        // TODO: remove this when Lighting configuration will be behind a key in mesh parameters. This case will be handled by the code just above
                        // set the lighting configuration description
                        Tuple<Guid, string> lightingReference;
                        if (Lightings.TryGetValue(mesh.Name, out lightingReference))
                            mesh.Parameters.Set(LightingKeys.LightingConfigurations, new ContentReference<LightingConfigurationsSetData>(lightingReference.Item1, lightingReference.Item2));
                    }

                    // split the meshes if necessary
                    model.Meshes = SplitExtensions.SplitMeshes(model.Meshes, Allow32BitIndex);

                    // merge the meshes
                    if (Compact)
                    {
                        var indicesBlackList = new HashSet<int>();
                        if (PreservedNodes != null)
                        {
                            for (var index = 0; index < model.Hierarchy.Nodes.Length; ++index)
                            {
                                var node = model.Hierarchy.Nodes[index];
                                if (PreservedNodes.Contains(node.Name))
                                    indicesBlackList.Add(index);
                            }
                        }

                        // group meshes with same material and same root
                        var sameMaterialMeshes = new List<GroupList<int, MeshData>>();
                        GroupFromIndex(model, 0, indicesBlackList, model.Meshes, sameMaterialMeshes);

                        // remove meshes that cannot be merged
                        var excludedMeshes = new List<MeshData>();
                        var finalMeshGroups = new List<GroupList<int, MeshData>>();
                        foreach (var meshList in sameMaterialMeshes)
                        {
                            var mergeList = new GroupList<int, MeshData> { Key = meshList.Key };

                            foreach (var mesh in meshList)
                            {
                                if (mesh.Skinning != null || indicesBlackList.Contains(mesh.NodeIndex))
                                    excludedMeshes.Add(mesh);
                                else
                                    mergeList.Add(mesh);
                            }

                            if (mergeList.Count <= 1)
                                excludedMeshes.AddRange(mergeList);
                            else
                                finalMeshGroups.Add(mergeList);
                        }

                        var finalMeshes = new List<MeshData>();

                        finalMeshes.AddRange(excludedMeshes);

                        foreach (var meshList in finalMeshGroups)
                        {
                            // transform the buffers
                            foreach (var mesh in meshList)
                            {
                                var transformationMatrix = GetMatrixFromIndex(model.Hierarchy.Nodes, hierarchyUpdater, meshList.Key, mesh.NodeIndex);
                                mesh.Draw.VertexBuffers[0].TransformBuffer(ref transformationMatrix);
                            }

                            // refine the groups base on several tests
                            var newMeshGroups = new List<GroupList<int, MeshData>> { meshList };
                            // only regroup meshes if they share the same parameters
                            newMeshGroups = RefineGroups(newMeshGroups, CompareParameters);
                            // only regroup meshes if they share the shadow options
                            newMeshGroups = RefineGroups(newMeshGroups, CompareShadowOptions);
                            //only regroup meshes if they share the same lighting configurations
                            newMeshGroups = RefineGroups(newMeshGroups, CompareLightingConfigurations);


                            // add to the final meshes groups
                            foreach (var sameParamsMeshes in newMeshGroups)
                            {
                                var baseMesh = sameParamsMeshes[0];
                                var newMeshList = sameParamsMeshes.Select(x => x.Draw).ToList().GroupDrawData(Allow32BitIndex);
                                foreach (var generatedMesh in newMeshList)
                                {
                                    finalMeshes.Add(new MeshData {
                                            Material = baseMesh.Material,
                                            Parameters = baseMesh.Parameters,
                                            Name = baseMesh.Name,
                                            Draw = generatedMesh,
                                            NodeIndex = meshList.Key,
                                            Skinning = null,
                                        });
                                }
                            }
                        }

                        // delete empty nodes (neither mesh nor bone attached)
                        var keptNodes = new bool[model.Hierarchy.Nodes.Length];
                        for (var i = 0; i < keptNodes.Length; ++i)
                        {
                            keptNodes[i] = false;
                        }
                        foreach (var keepIndex in indicesBlackList)
                        {
                            var nodeIndex = keepIndex;
                            while (nodeIndex != -1 && !keptNodes[nodeIndex])
                            {
                                keptNodes[nodeIndex] = true;
                                nodeIndex = model.Hierarchy.Nodes[nodeIndex].ParentIndex;
                            }
                        }
                        foreach (var mesh in finalMeshes)
                        {
                            var nodeIndex = mesh.NodeIndex;
                            while (nodeIndex != -1 && !keptNodes[nodeIndex])
                            {
                                keptNodes[nodeIndex] = true;
                                nodeIndex = model.Hierarchy.Nodes[nodeIndex].ParentIndex;
                            }

                            if (mesh.Skinning != null)
                            {
                                foreach (var bone in mesh.Skinning.Bones)
                                {
                                    nodeIndex = bone.NodeIndex;
                                    while (nodeIndex != -1 && !keptNodes[nodeIndex])
                                    {
                                        keptNodes[nodeIndex] = true;
                                        nodeIndex = model.Hierarchy.Nodes[nodeIndex].ParentIndex;
                                    }
                                }
                            }
                        }

                        var newNodes = new List<ModelNodeDefinition>();
                        var newMapping = new int[model.Hierarchy.Nodes.Length];
                        for (var i = 0; i < keptNodes.Length; ++i)
                        {
                            if (keptNodes[i])
                            {
                                var parentIndex = model.Hierarchy.Nodes[i].ParentIndex;
                                if (parentIndex != -1)
                                    model.Hierarchy.Nodes[i].ParentIndex = newMapping[parentIndex]; // assume that the nodes are well ordered
                                newMapping[i] = newNodes.Count;
                                newNodes.Add(model.Hierarchy.Nodes[i]);
                            }
                        }

                        foreach (var mesh in finalMeshes)
                        {
                            mesh.NodeIndex = newMapping[mesh.NodeIndex];
                            
                            if (mesh.Skinning != null)
                            {
                                for (var i = 0; i < mesh.Skinning.Bones.Length; ++i)
                                {
                                    mesh.Skinning.Bones[i].NodeIndex = newMapping[mesh.Skinning.Bones[i].NodeIndex];
                                }
                            }
                        }

                        model.Meshes = finalMeshes;
                        model.Hierarchy.Nodes = newNodes.ToArray();

                        hierarchyUpdater = new ModelViewHierarchyUpdater(model.Hierarchy.Nodes);
                        hierarchyUpdater.UpdateMatrices();
                    }

                    // bounding boxes
                    foreach (var mesh in model.Meshes)
                    {
                        var vertexBuffers = mesh.Draw.VertexBuffers;
                        if (vertexBuffers.Length > 0)
                        {
                            // Compute local mesh bounding box (no node transformation)
                            Matrix matrix = Matrix.Identity;
                            mesh.BoundingBox = vertexBuffers[0].ComputeBoundingBox(ref matrix);

                            // Compute model bounding box (includes node transformation)
                            hierarchyUpdater.GetWorldMatrix(mesh.NodeIndex, out matrix);
                            var meshBoundingBox = vertexBuffers[0].ComputeBoundingBox(ref matrix);
                            BoundingBox.Merge(ref model.BoundingBox, ref meshBoundingBox, out model.BoundingBox);
                        }

                        // TODO: temporary Always try to compact
                        mesh.Draw.CompactIndexBuffer();
                    }

                    // merges all the Draw VB and IB together to produce one final VB and IB by entity.
                    var sizeVertexBuffer = model.Meshes.SelectMany(x => x.Draw.VertexBuffers).Select(x => x.Buffer.Value.Content.Length).Sum();
                    var sizeIndexBuffer = model.Meshes.Select(x => x.Draw.IndexBuffer).Select(x => x.Buffer.Value.Content.Length).Sum();
                    var vertexBuffer = new BufferData(BufferFlags.VertexBuffer, new byte[sizeVertexBuffer]);
                    var indexBuffer = new BufferData(BufferFlags.IndexBuffer, new byte[sizeIndexBuffer]);
                    var vertexBufferNextIndex = 0;
                    var indexBufferNextIndex = 0;
                    foreach (var drawMesh in model.Meshes.Select(x => x.Draw))
                    {
                        // the index buffer
                        var oldIndexBuffer = drawMesh.IndexBuffer.Buffer.Value.Content;
                    
                        Array.Copy(oldIndexBuffer, 0, indexBuffer.Content, indexBufferNextIndex, oldIndexBuffer.Length);
                    
                        drawMesh.IndexBuffer.Offset = indexBufferNextIndex;
                        drawMesh.IndexBuffer.Buffer = indexBuffer;
                    
                        indexBufferNextIndex += oldIndexBuffer.Length;
                    
                        // the vertex buffers
                        foreach (var vertexBufferBinding in drawMesh.VertexBuffers)
                        {
                            var oldVertexBuffer = vertexBufferBinding.Buffer.Value.Content;
                    
                            Array.Copy(oldVertexBuffer, 0, vertexBuffer.Content, vertexBufferNextIndex, oldVertexBuffer.Length);
                    
                            vertexBufferBinding.Offset = vertexBufferNextIndex;
                            vertexBufferBinding.Buffer = vertexBuffer;
                    
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

                commandContext.Logger.Info("The {0} has been successfully imported.", ContextAsString);

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
        private List<GroupList<int, MeshData>> RefineGroups(List<GroupList<int, MeshData>> meshList, SameGroup sameGroupDelegate)
        {
            var finalGroups = new List<GroupList<int, MeshData>>();
            foreach (var meshGroup in meshList)
            {
                var updatedGroups = new List<GroupList<int, MeshData>>();
                foreach (var mesh in meshGroup)
                {
                    var createNewGroup = true;
                    foreach (var sameParamsMeshes in updatedGroups)
                    {
                        if (sameGroupDelegate(sameParamsMeshes[0], mesh))
                        {
                            sameParamsMeshes.Add(mesh);
                            createNewGroup = false;
                            break;
                        }
                    }

                    if (createNewGroup)
                    {
                        var newGroup = new GroupList<int, MeshData> { Key = meshGroup.Key };
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
        private Dictionary<Guid, List<MeshData>> GroupFromIndex(ModelData model, int index, HashSet<int> nodeBlackList, List<MeshData> meshes, List<GroupList<int, MeshData>> finalLists)
        {
            var children = GetChildren(model.Hierarchy.Nodes, index);
            
            var materialGroups = new Dictionary<Guid, List<MeshData>>();

            // Get the group from each child
            foreach (var child in children)
            {
                var newMaterialGroups = GroupFromIndex(model, child, nodeBlackList, meshes, finalLists);

                foreach (var group in newMaterialGroups)
                {
                    if (!materialGroups.ContainsKey(group.Key))
                        materialGroups.Add(group.Key, new List<MeshData>());
                    materialGroups[group.Key].AddRange(group.Value);
                }
            }

            // Add the current node if it has a mesh
            foreach (var nodeMesh in meshes.Where(x => x.NodeIndex == index))
            {
                var matId = nodeMesh.Material == null ? Guid.Empty : nodeMesh.Material.Id;
                if (!materialGroups.ContainsKey(matId))
                    materialGroups.Add(matId, new List<MeshData>());
                materialGroups[matId].Add(nodeMesh);
            }

            // Store the generated list as final if the node should be kept
            if (nodeBlackList.Contains(index) || index == 0)
            {
                foreach (var materialGroup in materialGroups)
                {
                    var groupList = new GroupList<int, MeshData>();
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
        /// <param name="hierarchy">The node hierarchy.</param>
        /// <param name="updater">The updater containing the local matrices.</param>
        /// <param name="rootIndex">The root index.</param>
        /// <param name="index">The current index.</param>
        /// <returns>The matrix at this index.</returns>
        private Matrix GetMatrixFromIndex(ModelNodeDefinition[] hierarchy, ModelViewHierarchyUpdater updater, int rootIndex, int index)
        {
            if (index == -1 || index == rootIndex)
                return Matrix.Identity;

            Matrix outMatrix;
            updater.GetLocalMatrix(index, out outMatrix);

            if (index != rootIndex)
            {
                var topMatrix = GetMatrixFromIndex(hierarchy, updater, rootIndex, hierarchy[index].ParentIndex);
                outMatrix = Matrix.Multiply(outMatrix, topMatrix);
            }

            return outMatrix;
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

        protected abstract ModelData LoadModel(ICommandContext commandContext, AssetManager assetManager);

        protected abstract AnimationClip LoadAnimation(ICommandContext commandContext, AssetManager assetManager);

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            writer.SerializeExtended(this, ArchiveMode.Serialize);
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
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
        private static bool CompareParameters(MeshData baseMesh, MeshData newMesh)
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
        private static bool CompareShadowOptions(MeshData baseMesh, MeshData newMesh)
        {
            return CompareKeyValue(baseMesh.Parameters, newMesh.Parameters, LightingKeys.CastShadows)
                   && CompareKeyValue(baseMesh.Parameters, newMesh.Parameters, LightingKeys.ReceiveShadows);
        }

        /// <summary>
        /// Compares the value behind a key in two ParameterCollectionData.
        /// </summary>
        /// <param name="parameters0">The first ParameterCollectionData.</param>
        /// <param name="parameters1">The second ParameterCollectionData.</param>
        /// <param name="key">The ParameterKey.</param>
        /// <returns>True</returns>
        private static bool CompareKeyValue<T>(ParameterCollectionData parameters0, ParameterCollectionData parameters1, ParameterKey<T> key)
        {
            var value0 = parameters0 != null && parameters0.ContainsKey(key) ? parameters0[key] : key.DefaultValueMetadataT.DefaultValue;
            var value1 = parameters1 != null && parameters1.ContainsKey(key) ? parameters1[key] : key.DefaultValueMetadataT.DefaultValue;
            return value0 == value1;
        }

        /// <summary>
        /// Compares the lighting configurations of the two meshes.
        /// </summary>
        /// <param name="baseMesh">The base mesh.</param>
        /// <param name="newMesh">The mesh to compare.</param>
        /// <returns>True if all the configurations are the same, false otherwise.</returns>
        private static bool CompareLightingConfigurations(MeshData baseMesh, MeshData newMesh)
        {
            var config0Content = GetLightingConfigurations(baseMesh);
            var config1Content = GetLightingConfigurations(newMesh);
            if (config0Content == null && config1Content == null)
                return true;
            if (config0Content == null || config1Content == null)
                return false;
            return config0Content.Id == config1Content.Id;
        }

        /// <summary>
        /// Retrives the lighting configurations if present.
        /// </summary>
        /// <param name="mesh">The mesh containing the lighting configurations.</param>
        /// <returns>The content reference to the lighting configuration.</returns>
        private static ContentReference GetLightingConfigurations(MeshData mesh)
        {
            if (mesh != null && mesh.Parameters != null && mesh.Parameters.ContainsKey(LightingKeys.LightingConfigurations))
            {
                var config = mesh.Parameters[LightingKeys.LightingConfigurations];
                if (config != null)
                    return config as ContentReference;
            }
            return null;
        }

        /// <summary>
        /// Test if two ParameterCollectionData are equal
        /// </summary>
        /// <param name="parameters0">The first ParameterCollectionData.</param>
        /// <param name="parameters1">The second ParameterCollectionData.</param>
        /// <returns>True if the collections are the same, false otherwise.</returns>
        private static bool AreCollectionsEqual(ParameterCollectionData parameters0, ParameterCollectionData parameters1)
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