using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Assets.Model
{
    internal class PrefabModelAssetCompiler : AssetCompilerBase<PrefabModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, PrefabModelAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new PrefabModelAssetCompileCommand(urlInStorage, asset, AssetItem) };
            result.ShouldWaitForPreviousBuilds = true;
        }

        private class PrefabModelAssetCompileCommand : AssetCommand<PrefabModelAsset>
        {
            private readonly Package package;

            public PrefabModelAssetCompileCommand(string url, PrefabModelAsset assetParameters, AssetItem assetItem) 
                : base(url, assetParameters)
            {
                package = assetItem.Package;
            }

            private class MeshData
            {
                public readonly List<byte> VertexData = new List<byte>();
                public int VertexStride;
                public readonly List<byte> IndexData = new List<byte>();
            }

            private struct EntityChunk
            {
                public Entity Entity;
                public ModelComponent ModelComponent;
                public Rendering.Model Model;
                public int MaterialIndex;
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);

                // We also want to serialize recursively the compile-time dependent assets
                // (since they are not added as reference but actually embedded as part of the current asset)
                // TODO: Ideally we would want to put that automatically in AssetCommand<>, but we would need access to package
                ComputeCompileTimeDependenciesHash(package, writer, AssetParameters);
            }

            private static void ProcessMaterial(ContentManager manager, GraphicsDevice device, ICollection<EntityChunk> chunks, MaterialInstance material, Rendering.Model prefabModel)
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

                            //todo need to actually transform the vertexes

                            mesh.VertexData.AddRange(vertexData.Skip(modelMesh.Draw.VertexBuffers[0].Offset * modelMesh.Draw.VertexBuffers[0].Stride).Take(modelMesh.Draw.VertexBuffers[0].Count * modelMesh.Draw.VertexBuffers[0].Stride));

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

                            //todo handle 32 bit 16 bit?
                            //its actually acting weird
                            mesh.IndexData.AddRange(indexData.Skip(modelMesh.Draw.IndexBuffer.Offset * 4).Take(modelMesh.Draw.IndexBuffer.Count * 4));
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

                    var gpuMesh = new Mesh
                    {
                        Draw = new MeshDraw(),
                        MaterialIndex = matIndex
                    };

                    var vertexBuffer = new BufferData(BufferFlags.VertexBuffer, new byte[vertexArray.Length]);
                    var indexBuffer = new BufferData(BufferFlags.IndexBuffer, new byte[indexArray.Length]);

                    var vertexBufferSerializable = vertexBuffer.ToSerializableVersion();
                    var indexBufferSerializable = indexBuffer.ToSerializableVersion();

                    Array.Copy(vertexArray, vertexBuffer.Content, vertexArray.Length);
                    Array.Copy(indexArray, indexBuffer.Content, indexArray.Length);

                    gpuMesh.Draw.VertexBuffers = new VertexBufferBinding[1];                   
                    gpuMesh.Draw.VertexBuffers[0] = new VertexBufferBinding(vertexBufferSerializable, meshData.Key, vertexArray.Length / meshData.Value.VertexStride);
                    gpuMesh.Draw.IndexBuffer = new IndexBufferBinding(indexBufferSerializable, true, indexArray.Length / 4);

                    prefabModel.Meshes.Add(gpuMesh);
                }
            }

            private static MaterialInstance ExtractMaterialInstance(MaterialInstance baseInstance, int index, ModelComponent modelComponent, Material fallbackMaterial)
            {
                var instance = new MaterialInstance
                {
                    Material = modelComponent.Materials[index] ?? baseInstance.Material ?? fallbackMaterial,
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

                var prefab = contentManager.Load<Prefab>(AssetParameters.Prefab.Location);
                if (prefab == null) throw new Exception("Failed to load prefab.");

                var prefabModel = new Rendering.Model();

                //The objective is to create 1 mesh per material/shadow params
                //1. We group by materials
                //2. Create a mesh per material (might need still more meshes if 16bit indexes or more then 32bit)

                var materials = new Dictionary<MaterialInstance, List<EntityChunk>>();

                foreach (var entity in prefab.Entities)
                {
                    //Hard coded for now to entities that have 2 components , transform + model and only Root node
                    var modelComponent = entity.Get<ModelComponent>();
                    if (entity.Components.Count == 2 && modelComponent != null && modelComponent.Skeleton.Nodes.Length == 1)
                    {
                        //add asset materials
                        var modelAsset = contentManager.Load<Rendering.Model>(AttachedReferenceManager.GetUrl(modelComponent.Model));
                        if (modelAsset == null || 
                            modelAsset.Meshes.Any(x => x.Draw.PrimitiveType != PrimitiveType.TriangleList || x.Draw.VertexBuffers.Length > 1)) //For now we limit only to TriangleList types and interleaved vertex buffers
                            goto Ignore;

                        for (var index = 0; index < modelAsset.Materials.Count; index++)
                        {
                            var material = modelAsset.Materials[index];
                            var mat = ExtractMaterialInstance(material, index, modelComponent, fallbackMaterial);
                            var chunk = new EntityChunk { Entity = entity, ModelComponent = modelComponent, Model = modelAsset, MaterialIndex = index };

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

                        continue;
                    }

                    Ignore:
                    commandContext.Logger.Info($"Ignoring entity {entity.Name} since it is not compatible with PrefabModel.");
                }

                foreach (var material in materials)
                {
                    ProcessMaterial(contentManager, device, material.Value, material.Key, prefabModel);
                }

                var modelBoundingBox = prefabModel.BoundingBox;
                var modelBoundingSphere = prefabModel.BoundingSphere;
                foreach (var mesh in prefabModel.Meshes)
                {
                    var vertexBuffers = mesh.Draw.VertexBuffers;
                    if (vertexBuffers.Length > 0)
                    {
                        // Compute local mesh bounding box (no node transformation)
                        Matrix matrix = Matrix.Identity;
                        mesh.BoundingBox = vertexBuffers[0].ComputeBounds(ref matrix, out mesh.BoundingSphere);

                        // Compute model bounding box (includes node transformation)
                        BoundingSphere meshBoundingSphere;
                        var meshBoundingBox = vertexBuffers[0].ComputeBounds(ref matrix, out meshBoundingSphere);
                        BoundingBox.Merge(ref modelBoundingBox, ref meshBoundingBox, out modelBoundingBox);
                        BoundingSphere.Merge(ref modelBoundingSphere, ref meshBoundingSphere, out modelBoundingSphere);
                    }

                    // TODO: temporary Always try to compact
                    mesh.Draw.CompactIndexBuffer();
                }
                prefabModel.BoundingBox = modelBoundingBox;
                prefabModel.BoundingSphere = modelBoundingSphere;

                contentManager.Save(Url, prefabModel);

                device.Dispose();

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
