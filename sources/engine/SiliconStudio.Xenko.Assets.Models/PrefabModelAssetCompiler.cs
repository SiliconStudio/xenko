using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Assets.Models
{
    internal class PrefabModelAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (PrefabModelAsset)assetItem.Asset;
            var renderingSettings = context.GetGameSettingsAsset().Get<RenderingSettings>();
            result.BuildSteps = new ListBuildStep { new PrefabModelAssetCompileCommand(targetUrlInStorage, asset, assetItem, renderingSettings) };
            result.ShouldWaitForPreviousBuilds = true;
        }

        private class PrefabModelAssetCompileCommand : AssetCommand<PrefabModelAsset>
        {
            private readonly Package package;
            private readonly RenderingSettings renderingSettings;

            public PrefabModelAssetCompileCommand(string url, PrefabModelAsset parameters, AssetItem assetItem, RenderingSettings renderingSettings)
                : base(url, parameters)
            {
                package = assetItem.Package;
                this.renderingSettings = renderingSettings;
            }

            private class MeshData
            {
                public readonly List<byte> VertexData = new List<byte>();
                public int VertexStride;

                public readonly List<byte> IndexData = new List<byte>();
                public int IndexOffset;
            }

            private struct EntityChunk
            {
                public Entity Entity;
                public Model Model;
                public int MaterialIndex;
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);

                if (Parameters.Prefab == null) return;

                // We also want to serialize recursively the compile-time dependent assets
                // (since they are not added as reference but actually embedded as part of the current asset)
                // TODO: Ideally we would want to put that automatically in AssetCommand<>, but we would need access to package
                ComputeCompileTimeDependenciesHash(package, writer, Parameters);
            }

            private static unsafe void ProcessMaterial(ContentManager manager, ICollection<EntityChunk> chunks, MaterialInstance material, Model prefabModel)
            {
                //we need to futher group by VertexDeclaration
                var meshes = new Dictionary<VertexDeclaration, MeshData>();

                //actually create the mesh
                foreach (var chunk in chunks)
                {
                    foreach (var modelMesh in chunk.Model.Meshes)
                    {
                        //process only right material
                        if (modelMesh.MaterialIndex == chunk.MaterialIndex)
                        {
                            MeshData mesh;
                            if (!meshes.TryGetValue(modelMesh.Draw.VertexBuffers[0].Declaration, out mesh))
                            {
                                mesh = new MeshData { VertexStride = modelMesh.Draw.VertexBuffers[0].Stride };
                                meshes.Add(modelMesh.Draw.VertexBuffers[0].Declaration, mesh);
                            }

                            //vertexes
                            var vertexBufferRef = AttachedReferenceManager.GetAttachedReference(modelMesh.Draw.VertexBuffers[0].Buffer);
                            byte[] vertexData;
                            if (vertexBufferRef.Data != null)
                            {
                                vertexData = ((BufferData)vertexBufferRef.Data).Content;
                            }
                            else if (!vertexBufferRef.Url.IsNullOrEmpty())
                            {
                                var dataAsset = manager.Load<Graphics.Buffer>(vertexBufferRef.Url);
                                vertexData = dataAsset.GetSerializationData().Content;
                            }
                            else
                            {
                                throw new Exception($"Failed to get Vertex BufferData for entity {chunk.Entity.Name}'s model.");
                            }

                            //transform the vertexes according to the entity
                            var vertexDataCopy = vertexData.ToArray();
                            chunk.Entity.Transform.UpdateWorldMatrix(); //make sure matrix is computed
                            var worldMatrix = chunk.Entity.Transform.WorldMatrix;
                            var up = Vector3.Cross(worldMatrix.Right, worldMatrix.Forward);
                            bool isScalingNegative = Vector3.Dot(worldMatrix.Up, up) < 0.0f;

                            modelMesh.Draw.VertexBuffers[0].TransformBuffer(vertexDataCopy, ref worldMatrix);

                            //add to the big single array
                            var vertexes = vertexDataCopy
                                .Skip(modelMesh.Draw.VertexBuffers[0].Offset)
                                .Take(modelMesh.Draw.VertexBuffers[0].Count*modelMesh.Draw.VertexBuffers[0].Stride)
                                .ToArray();

                            mesh.VertexData.AddRange(vertexes);

                            //indices
                            var indexBufferRef = AttachedReferenceManager.GetAttachedReference(modelMesh.Draw.IndexBuffer.Buffer);
                            byte[] indexData;
                            if (indexBufferRef.Data != null)
                            {
                                indexData = ((BufferData)indexBufferRef.Data).Content;
                            }
                            else if (!indexBufferRef.Url.IsNullOrEmpty())
                            {
                                var dataAsset = manager.Load<Graphics.Buffer>(indexBufferRef.Url);
                                indexData = dataAsset.GetSerializationData().Content;
                            }
                            else
                            {
                                throw new Exception("Failed to get Indices BufferData for entity {chunk.Entity.Name}'s model.");
                            }

                            var indexSize = modelMesh.Draw.IndexBuffer.Is32Bit ? sizeof(uint) : sizeof(ushort);

                            byte[] indices;
                            if (isScalingNegative)
                            {
                                // Get reversed winding order
                                modelMesh.Draw.GetReversedWindingOrder(out indices);
                                indices = indices.Skip(modelMesh.Draw.IndexBuffer.Offset)
                                    .Take(modelMesh.Draw.IndexBuffer.Count*indexSize)
                                    .ToArray();
                            }
                            else
                            {
                                // Get indices normally
                                indices = indexData
                                    .Skip(modelMesh.Draw.IndexBuffer.Offset)
                                    .Take(modelMesh.Draw.IndexBuffer.Count*indexSize)
                                    .ToArray();
                            }

                            // Convert indices to 32 bits
                            if (indexSize == sizeof(ushort))
                            {
                                var uintIndices = new byte[indices.Length*2];
                                fixed (byte* psrc = indices)
                                fixed (byte* pdst = uintIndices)
                                {
                                    var src = (ushort*)psrc;
                                    var dst = (uint*)pdst;

                                    int numIndices = indices.Length/sizeof(ushort);
                                    for (var i = 0; i < numIndices; i++)
                                    {
                                        dst[i] = src[i];
                                    }
                                }
                                indices = uintIndices;
                            }

                            // Offset indices by mesh.IndexOffset
                            fixed (byte* pdst = indices)
                            {
                                var dst = (uint*)pdst;

                                int numIndices = indices.Length/sizeof(uint);
                                for (var i = 0; i < numIndices; i++)
                                {
                                    // Offset indices
                                    dst[i] += (uint)mesh.IndexOffset;
                                }
                            }

                            mesh.IndexOffset += modelMesh.Draw.VertexBuffers[0].Count;

                            mesh.IndexData.AddRange(indices);
                        }
                    }
                }

                //Sort out material
                var matIndex = prefabModel.Materials.Count;
                prefabModel.Materials.Add(material);

                foreach (var meshData in meshes)
                {
                    //todo need to take care of short index
                    var vertexArray = meshData.Value.VertexData.ToArray();
                    var indexArray = meshData.Value.IndexData.ToArray();

                    var vertexCount = vertexArray.Length/meshData.Value.VertexStride;
                    var indexCount = indexArray.Length/4;

                    var gpuMesh = new Mesh
                    {
                        Draw = new MeshDraw { PrimitiveType = PrimitiveType.TriangleList, DrawCount = indexCount, StartLocation = 0 },
                        MaterialIndex = matIndex
                    };

                    var vertexBuffer = new BufferData(BufferFlags.VertexBuffer, new byte[vertexArray.Length]);
                    var indexBuffer = new BufferData(BufferFlags.IndexBuffer, new byte[indexArray.Length]);

                    var vertexBufferSerializable = vertexBuffer.ToSerializableVersion();
                    var indexBufferSerializable = indexBuffer.ToSerializableVersion();

                    Array.Copy(vertexArray, vertexBuffer.Content, vertexArray.Length);
                    Array.Copy(indexArray, indexBuffer.Content, indexArray.Length);

                    gpuMesh.Draw.VertexBuffers = new VertexBufferBinding[1];
                    gpuMesh.Draw.VertexBuffers[0] = new VertexBufferBinding(vertexBufferSerializable, meshData.Key, vertexCount);
                    gpuMesh.Draw.IndexBuffer = new IndexBufferBinding(indexBufferSerializable, true, indexCount);

                    prefabModel.Meshes.Add(gpuMesh);
                }
            }

            private static MaterialInstance ExtractMaterialInstance(MaterialInstance baseInstance, int index, ModelComponent modelComponent, Material fallbackMaterial)
            {
                var instance = new MaterialInstance
                {
                    Material = modelComponent.Materials.SafeGet(index) ?? baseInstance.Material ?? fallbackMaterial,
                    IsShadowCaster = modelComponent.IsShadowCaster,
                    IsShadowReceiver = modelComponent.IsShadowReceiver
                };

                if (baseInstance != null)
                {
                    instance.IsShadowReceiver = instance.IsShadowReceiver && baseInstance.IsShadowReceiver;
                    instance.IsShadowCaster = instance.IsShadowCaster && baseInstance.IsShadowCaster;
                }

                return instance;
            }

            private static IEnumerable<T> IterateTree<T>(T root, Func<T, IEnumerable<T>> childrenF)
            {
                var q = new List<T>() { root };
                while (q.Any())
                {
                    var c = q[0];
                    q.RemoveAt(0);
                    q.AddRange(childrenF(c) ?? Enumerable.Empty<T>());
                    yield return c;
                }
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var contentManager = new ContentManager();

                var device = GraphicsDevice.New();

                var fallbackMaterial = Material.New(device, new MaterialDescriptor
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor()),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature()
                    }
                });

                var loadSettings = new ContentManagerLoaderSettings
                {
                    ContentFilter = ContentManagerLoaderSettings.NewContentFilterByType(typeof(Mesh), typeof(Skeleton), typeof(Material), typeof(Prefab))
                };

                IList<Entity> allEntities = new List<Entity>();
                if (Parameters.Prefab != null)
                {
                    try
                    {
                        var prefab = contentManager.Load<Prefab>(Parameters.Prefab.Location, loadSettings);
                        if(prefab != null)
                            allEntities = prefab.Entities;
                    }
                    catch (Exception)
                    {
                        //ignored
                    }

                    if (allEntities.Count == 0)
                    {
                        try
                        {
                            var scene = contentManager.Load<Scene>(Parameters.Prefab.Location, loadSettings);
                            if(scene != null)
                                allEntities = scene.Entities;
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }
                }

                var prefabModel = new Model();

                //The objective is to create 1 mesh per material/shadow params
                //1. We group by materials
                //2. Create a mesh per material (might need still more meshes if 16bit indexes or more then 32bit)

                var materials = new Dictionary<MaterialInstance, List<EntityChunk>>();

                var validEntities = new List<Entity>();

                foreach (var rootEntity in allEntities)
                {
                    //collect sub entities as well
                    var collected = IterateTree(rootEntity, subEntity => subEntity.GetChildren() ).ToArray();

                    //first pass, check if compatible with prefabmodel
                    foreach (var subEntity in collected)
                    {
                        //todo for now we collect everything with a model component
                        var modelComponent = subEntity.Get<ModelComponent>();
                        
                        if (modelComponent?.Model == null || (modelComponent.Skeleton != null && modelComponent.Skeleton.Nodes.Length != 1) || !modelComponent.Enabled)
                            continue;
                        
                        var modelAsset = contentManager.Load<Model>(AttachedReferenceManager.GetUrl(modelComponent.Model), loadSettings);
                        if (modelAsset == null ||
                            modelAsset.Meshes.Any(x => x.Draw.PrimitiveType != PrimitiveType.TriangleList || x.Draw.VertexBuffers == null || x.Draw.VertexBuffers.Length != 1) ||
                            modelAsset.Materials.Any(x => x.Material != null && x.Material.HasTransparency) ||
                            modelComponent.Materials.Values.Any(x => x.HasTransparency)) //For now we limit only to TriangleList types and interleaved vertex buffers, also we skip transparent
                        {
                            commandContext.Logger.Info($"Skipped entity {subEntity.Name} since it's not compatible with PrefabModel.");
                            continue;
                        }

                        validEntities.Add(subEntity);
                    }                    
                }

                foreach (var subEntity in validEntities)
                {
                    var modelComponent = subEntity.Get<ModelComponent>();
                    var modelAsset = contentManager.Load<Model>(AttachedReferenceManager.GetUrl(modelComponent.Model), loadSettings);
                    for (var index = 0; index < modelAsset.Materials.Count; index++)
                    {
                        var material = modelAsset.Materials[index];
                        var mat = ExtractMaterialInstance(material, index, modelComponent, fallbackMaterial);

                        var chunk = new EntityChunk { Entity = subEntity, Model = modelAsset, MaterialIndex = index };

                        List<EntityChunk> entities;
                        if (materials.TryGetValue(mat, out entities))
                        {
                            entities.Add(chunk);
                        }
                        else
                        {
                            materials.Add(mat, new List<EntityChunk> { chunk });
                        }
                    }
                }

                foreach (var material in materials)
                {
                    ProcessMaterial(contentManager, material.Value, material.Key, prefabModel);
                }

                // split the meshes if necessary
                prefabModel.Meshes = SplitExtensions.SplitMeshes(prefabModel.Meshes, renderingSettings.DefaultGraphicsProfile > GraphicsProfile.Level_9_3);

                //handle boundng box/sphere
                var modelBoundingBox = prefabModel.BoundingBox;
                var modelBoundingSphere = prefabModel.BoundingSphere;
                foreach (var mesh in prefabModel.Meshes)
                {
                    var vertexBuffers = mesh.Draw.VertexBuffers;
                    if (vertexBuffers.Length > 0)
                    {
                        // Compute local mesh bounding box (no node transformation)
                        var matrix = Matrix.Identity;
                        mesh.BoundingBox = vertexBuffers[0].ComputeBounds(ref matrix, out mesh.BoundingSphere);

                        // Compute model bounding box (includes node transformation)
                        BoundingSphere meshBoundingSphere;
                        var meshBoundingBox = vertexBuffers[0].ComputeBounds(ref matrix, out meshBoundingSphere);
                        BoundingBox.Merge(ref modelBoundingBox, ref meshBoundingBox, out modelBoundingBox);
                        BoundingSphere.Merge(ref modelBoundingSphere, ref meshBoundingSphere, out modelBoundingSphere);
                    }

                    mesh.Draw.CompactIndexBuffer();
                }
                prefabModel.BoundingBox = modelBoundingBox;
                prefabModel.BoundingSphere = modelBoundingSphere;

                //save
                contentManager.Save(Url, prefabModel);

                device.Dispose();

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
