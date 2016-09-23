using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    class NavmeshAssetCompiler : AssetCompilerBase<NavmeshAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, NavmeshAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new AssetBuildStep(AssetItem) { new NavmeshBuildCommand(urlInStorage, AssetItem, asset, context) };
        }

        // DEBUG FUNCTION
        public static void DumpObj(string name, GeometricMeshData<VertexPositionNormalTexture> meshData)
        {
            string filePath = @"C:\Users\g-gj-waals\Desktop\" + name + ".obj";
            using(FileStream file = File.OpenWrite(filePath))
            using(StreamWriter sw = new StreamWriter(file))
            {
                for(int i = 0; i < meshData.Vertices.Length; i++)
                {
                    VertexPositionNormalTexture vert = meshData.Vertices[i];
                    sw.WriteLine("v {0} {1} {2}", vert.Position.X, vert.Position.Y, vert.Position.Z);

                }

                int numFaces = meshData.Indices.Length / 3;
                for(int i = 0; i < numFaces; i++)
                {
                    sw.WriteLine("f {0} {1} {2}",
                        meshData.Indices[i * 3 + 0] + 1,
                        meshData.Indices[i * 3 + 1] + 1,
                        meshData.Indices[i * 3 + 2] + 1);
                }
                sw.Flush();
                file.Flush();
            }
        }

        private class NavmeshBuildCommand : AssetCommand<NavmeshAsset>
        {
            private UFile assetUrl;
            private readonly AssetItem assetItem;
            private NavmeshAsset asset;
            private readonly Package package;

            public NavmeshBuildCommand(string url, AssetItem assetItem, NavmeshAsset value, AssetCompilerContext context)
                : base(url, value)
            {
                this.asset = value;
                this.assetItem = assetItem;
                this.package = assetItem.Package;
                assetUrl = url;
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                foreach (var compileTimeDependency in ((NavmeshAsset)assetItem.Asset).EnumerateCompileTimeDependencies(package.Session))
                {
                    yield return new ObjectUrl(UrlType.ContentLink, compileTimeDependency.Location);
                }
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                if(asset.DefaultScene == null)
                    return Task.FromResult(ResultStatus.Failed);

                var assetManager = new ContentManager();
                string sceneUrl = AttachedReferenceManager.GetUrl(asset.DefaultScene);
                var sceneAsset = (SceneAsset)package.Session.FindAsset(sceneUrl)?.Asset;
                //Scene scene = assetManager.Load<Scene>(sceneUrl);
                //AssetViewModel sceneAssetViewModel = Session.CurrentPackage.Assets.FirstOrDefault(x => x.AssetType == typeof(SceneAsset) && x.Name == Asset.DefaultScene.Name);

                List<Entity> sceneEntities = sceneAsset.Hierarchy.Parts.Select(x => x.Entity).ToList();
    //            Queue<Entity> entityQueue = new Queue<Entity>();
				//foreach(Entity e in scene.Entities)
				//	entityQueue.Enqueue(e);
    //            while(entityQueue.Count > 0)
    //            {
    //                Entity e = entityQueue.Dequeue();
    //                sceneEntities.Add(e);
    //                foreach(Entity c in e.GetChildren())
				//		entityQueue.Enqueue(c);
    //            }

                Navmesh generatedNavmesh = new Navmesh();

                // Data Storage
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshIndices = new List<int>();

                List<VertexPositionNormalTexture> debugVerts = new List<VertexPositionNormalTexture>();

                // Generate collision triangles for all static colliders
                List<StaticColliderComponent> staticColliders = new List<StaticColliderComponent>();

				// TODO: Make sure scene entities are populated
                foreach(var entity in sceneEntities)
                {
                    TransformComponent entityTransform = entity.Transform;
                    entityTransform.UpdateWorldMatrix();
                    Matrix entityWorldMatrix = entityTransform.WorldMatrix;

                    StaticColliderComponent collider = entity.Get<StaticColliderComponent>();
                    if(collider != null)
                    {
                        // TODO: Add compound shapes as well
                        collider.ComposeShape();
                        if(collider.ColliderShape == null)
                            continue; // No collider
                        if(collider.ColliderShape.GetType() == typeof(BoxColliderShape))
                        {
                            var box = (BoxColliderShape)collider.ColliderShape;
                            var boxDesc = (BoxColliderShapeDesc)box.Description;
                            Matrix boxTransform = box.PositiveCenterMatrix * entityWorldMatrix;

                            GeometricMeshData<VertexPositionNormalTexture> cubeMesh = GeometricPrimitive.Cube.New(boxDesc.Size, 1.0f, 1.0f, false);

                            // Transform box points
                            int vbase = meshVertices.Count;
                            for(int i = 0; i < cubeMesh.Vertices.Length; i++)
                            {
                                VertexPositionNormalTexture point = cubeMesh.Vertices[i];
                                point.Position = Vector3.Transform(point.Position, boxTransform).XYZ();
                                meshVertices.Add(point.Position);
                                debugVerts.Add(point);
                            }

                            // Send indices
                            for(int i = 0; i < cubeMesh.Indices.Length; i++)
                            {
                                meshIndices.Add(cubeMesh.Indices[i] + vbase);
                            }
                        }
                    }
                }

                // NOTE: Reversed winding order
                int[] flipIndices = { 0, 2, 1 };
                int numSrcTriangles = meshIndices.Count / 3;
                for(int i = 0; i < numSrcTriangles; i++)
                {
                    int j = meshIndices[i * 3 + 1];
                    meshIndices[i * 3 + 1] = meshIndices[i * 3 + 2];
                    meshIndices[i * 3 + 2] = j;
                }

                GeometricMeshData<VertexPositionNormalTexture> inputMeshData = new GeometricMeshData<VertexPositionNormalTexture>(debugVerts.ToArray(), meshIndices.ToArray(), false);
                DumpObj("input", inputMeshData);

                generatedNavmesh.Build(asset.BuildSettings, meshVertices.ToArray(), meshIndices.ToArray());

                assetManager.Save(assetUrl, generatedNavmesh);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
