using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    class NavmeshAssetCompiler : AssetCompilerBase<NavmeshAsset>
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, NavmeshAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new AssetBuildStep(assetItem) { new NavmeshBuildCommand(assetItem.Location, assetItem, asset, context) };
        }

        // DEBUG FUNCTIONS
        public static void DumpObj(string name, Vector3[] meshData)
        {
            string filePath = @"C:\Users\g-gj-waals\Desktop\" + name + ".obj";
            using (FileStream file = File.OpenWrite(filePath))
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < meshData.Length; i++)
                {
                    Vector3 vert = meshData[i];
                    sw.WriteLine("v {0} {1} {2}", vert.X, vert.Y, vert.Z);

                }

                int numFaces = meshData.Length / 3;
                for (int i = 0; i < numFaces; i++)
                {
                    int start = 1 + i * 3;
                    sw.WriteLine("f {0} {1} {2}",
                       start + 0,
                       start + 1,
                       start + 2);
                }
                sw.Flush();
                file.Flush();
            }
        }
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

        public static void DumpBinary(string name, byte[] data)
        {
            string filePath = @"C:\Users\g-gj-waals\Desktop\" + name;
            using(FileStream file = File.OpenWrite(filePath))
            {
                file.Write(data, 0, data.Length);
            }
        }

        private class NavmeshBuildCommand : AssetCommand<NavmeshAsset>
        {
            private UFile assetUrl;
            private readonly AssetItem assetItem;
            private NavmeshAsset asset;
            private readonly Package package;

            // Combined scene data to create input meshData
            private List<Vector3> meshVertices = new List<Vector3>();
            private List<int> meshIndices = new List<int>();

            // Automatically calculated bounding box
            private BoundingBox boundingBox = new BoundingBox();
            private bool calculateBoundingBox;

            // Deferred shapes such as infinite planes which should be added after the bounding box of the scene is generated
            struct DeferredShape
            {
                public Matrix Transform;
                public IColliderShapeDesc Description;
            }
            private List<DeferredShape> deferredShapes = new List<DeferredShape>();

            // TODO: Remove this
            private List<VertexPositionNormalTexture> debugVerts = new List<VertexPositionNormalTexture>();

            public NavmeshBuildCommand(string url, AssetItem assetItem, NavmeshAsset value, AssetCompilerContext context)
                : base(url, value)
            {
                this.asset = value;
                this.assetItem = assetItem;
                this.package = assetItem.Package;
                assetUrl = url;
            }

            //protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            //{
            //    foreach (var compileTimeDependency in ((NavmeshAsset)assetItem.Asset).EnumerateCompileTimeDependencies(package.Session))
            //    {
            //        yield return new ObjectUrl(UrlType.ContentLink, compileTimeDependency.Location);
            //    }
            //}

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                // We also want to serialize recursively the compile-time dependent assets
                // (since they are not added as reference but actually embedded as part of the current asset)
                // TODO: Ideally we would want to put that automatically in AssetCommand<>, but we would need access to package
                ComputeCompileTimeDependenciesHash(package, writer, Parameters);
            }

            void AppendInputMeshData(GeometricMeshData<VertexPositionNormalTexture> meshData, Matrix objectTransform)
            {
                // Transform box points
                int vbase = meshVertices.Count;
                for (int i = 0; i < meshData.Vertices.Length; i++)
                {
                    VertexPositionNormalTexture point = meshData.Vertices[i];
                    point.Position = Vector3.Transform(point.Position, objectTransform).XYZ();
                    meshVertices.Add(point.Position);
                    debugVerts.Add(point);

                    // Calculate bounding box?
                    if(calculateBoundingBox)
                    {
                        BoundingBox.Merge(ref boundingBox, ref point.Position, out boundingBox);
                    }
                }

                // Send indices
                for (int i = 0; i < meshData.Indices.Length; i++)
                {
                    meshIndices.Add(meshData.Indices[i] + vbase);
                }
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                if(asset.DefaultScene == null)
                    return Task.FromResult(ResultStatus.Failed);

                var assetManager = new ContentManager();
                string sceneUrl = AttachedReferenceManager.GetUrl(asset.DefaultScene);
                var sceneAsset = (SceneAsset)package.Session.FindAsset(sceneUrl)?.Asset;
                
                // Copy build settings so we can modify them
                NavmeshBuildSettings buildSettings = asset.BuildSettings;

                calculateBoundingBox = asset.AutoGenerateBoundingBox;
                boundingBox = calculateBoundingBox ? BoundingBox.Empty : buildSettings.BoundingBox;

                // Turn the entire entity hierarchy into a single list
                List <Entity> sceneEntities = sceneAsset.Hierarchy.Parts.Select(x => x.Entity).ToList();

                // The output object of the compilation
                Navmesh generatedNavmesh = new Navmesh();

                // Generate collision triangles for all static colliders
                List<StaticColliderComponent> staticColliders = new List<StaticColliderComponent>();
                
                foreach(var entity in sceneEntities)
                {
                    TransformComponent entityTransform = entity.Transform;
                    entityTransform.UpdateWorldMatrix();
                    Matrix entityWorldMatrix = entityTransform.WorldMatrix;

                    StaticColliderComponent collider = entity.Get<StaticColliderComponent>();
                    if(collider != null && collider.IsBlocking)
                    {
                        // TODO: Add compound shapes and types other than box as well
                        collider.ComposeShape();
                        if(collider.ColliderShape == null)
                            continue; // No collider

                        // Interate through all the colliders shapes while queueing all shapes in compound shapes to process those as well
                        Queue<ColliderShape> shapesToProcess = new Queue<ColliderShape>();
                        shapesToProcess.Enqueue(collider.ColliderShape);
                        while(!shapesToProcess.IsEmpty())
                        {
                            var shape = shapesToProcess.Dequeue();
                            var shapeType = shape.GetType();
                            if(shapeType == typeof(BoxColliderShape))
                            {
                                var box = (BoxColliderShape)shape;
                                var boxDesc = (BoxColliderShapeDesc)box.Description;
                                Matrix transform = box.PositiveCenterMatrix * entityWorldMatrix;

                                var meshData = GeometricPrimitive.Cube.New(boxDesc.Size);
                                AppendInputMeshData(meshData, transform);
                            }
                            else if(shapeType == typeof(SphereColliderShape))
                            {
                                var sphere = (SphereColliderShape)shape;
                                var sphereDesc = (SphereColliderShapeDesc)sphere.Description;
                                Matrix transform = sphere.PositiveCenterMatrix * entityWorldMatrix;

                                var meshData = GeometricPrimitive.Sphere.New(sphereDesc.Radius);
                                AppendInputMeshData(meshData, transform);
                            }
                            else if(shapeType == typeof(CylinderColliderShape))
                            {
                                var cylinder = (CylinderColliderShape)shape;
                                var cylinderDesc = (CylinderColliderShapeDesc)cylinder.Description;
                                Matrix transform = cylinder.PositiveCenterMatrix * entityWorldMatrix;

                                var meshData = GeometricPrimitive.Cylinder.New(cylinderDesc.Height, cylinderDesc.Radius);
                                AppendInputMeshData(meshData, transform);
                            }
                            else if(shapeType == typeof(CapsuleColliderShape))
                            {
                                var capsule = (CapsuleColliderShape)shape;
                                var capsuleDesc = (CapsuleColliderShapeDesc)capsule.Description;
                                Matrix transform = capsule.PositiveCenterMatrix * entityWorldMatrix;

                                var meshData = GeometricPrimitive.Capsule.New(capsuleDesc.Length, capsuleDesc.Radius);
                                AppendInputMeshData(meshData, transform);
                            }
                            else if (shapeType == typeof(StaticPlaneColliderShape))
                            {
                                var plane = (StaticPlaneColliderShape)shape;
                                var planeDesc = (StaticPlaneColliderShapeDesc)plane.Description;
                                Matrix transform = plane.PositiveCenterMatrix * entityWorldMatrix;
                                // TODO: Apply plane normal and offset from description?

                                // Defer infinite planes because their size is not defined yet
                                deferredShapes.Add(new DeferredShape { Description = planeDesc, Transform = transform });
                            }
                            else if(shapeType == typeof(ConvexHullColliderShape))
                            {
                                var hull = (ConvexHullColliderShape)shape;
                                var hullDesc = (ConvexHullColliderShapeDesc)hull.Description;
                                Matrix transform = hull.PositiveCenterMatrix * entityWorldMatrix;
                                
                                for(int j = 0; j < hullDesc.ConvexHulls.Count; j++)
                                {
                                    var v0 = hullDesc.ConvexHulls[j];
                                    var i0 = hullDesc.ConvexHullsIndices[j];
                                    for (int k = 0; k < v0.Count; k++)
                                    {
                                        var verts = v0[k];
                                        var inds = i0[k];
                                        VertexPositionNormalTexture[] verts2 = new VertexPositionNormalTexture[verts.Count];
                                        var inds2 = new int[inds.Count];
                                        for(int i = 0; i < verts.Count; i++)
                                        {
                                            verts2[i] = new VertexPositionNormalTexture(verts[i], Vector3.UnitY, Vector2.Zero);
                                        }
                                        for (int i = 0; i < inds.Count; i++)
                                        {
                                            inds2[i] = (int)inds[i];
                                        }
                                        AppendInputMeshData(new GeometricMeshData<VertexPositionNormalTexture>(verts2, inds2.ToArray(), false), transform);
                                    }
                                }
                            }
                            else if(shapeType == typeof(CompoundColliderShape))
                            {
                                var compound = (CompoundColliderShape)shape;
                                for(int i = 0; i < compound.Count; i++)
                                {
                                    shapesToProcess.Enqueue(compound[i]);
                                }
                            }
                        }
                    }

                }

                if(calculateBoundingBox)
                {
                    // Store calculated bounding box
                    buildSettings.BoundingBox = boundingBox;
                }

                // Process deferred shapes
                Vector3 bbExtent = buildSettings.BoundingBox.Extent;
                float maxBoundsSize = Math.Max(bbExtent.X, Math.Max(bbExtent.Y, bbExtent.Z)) * 2.0f;
                foreach(DeferredShape shape in deferredShapes)
                {
                    StaticPlaneColliderShapeDesc planeDesc = (StaticPlaneColliderShapeDesc)shape.Description;
                    // TODO: Fit plane in bounding box properly
                    var meshData = GeometricPrimitive.Plane.New(maxBoundsSize, maxBoundsSize);
                    AppendInputMeshData(meshData, shape.Transform);
                }

                // NOTE: Reversed winding order as input to recast
                int[] flipIndices = { 0, 2, 1 };
                int numSrcTriangles = meshIndices.Count / 3;
                for(int i = 0; i < numSrcTriangles; i++)
                {
                    int j = meshIndices[i * 3 + 1];
                    meshIndices[i * 3 + 1] = meshIndices[i * 3 + 2];
                    meshIndices[i * 3 + 2] = j;
                }

                GeometricMeshData<VertexPositionNormalTexture> inputMeshData = new GeometricMeshData<VertexPositionNormalTexture>(debugVerts.ToArray(), meshIndices.ToArray(), false);

                // TODO: Remove this
                DumpObj("input", inputMeshData);

                if(!generatedNavmesh.Build(buildSettings, meshVertices.ToArray(), meshIndices.ToArray()))
                    return Task.FromResult(ResultStatus.Failed);
                assetManager.Save(assetUrl, generatedNavmesh);

                // TODO: Remove this
                DumpObj("output", generatedNavmesh.MeshVertices);
                DumpBinary("navmesh", generatedNavmesh.NavmeshData);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
