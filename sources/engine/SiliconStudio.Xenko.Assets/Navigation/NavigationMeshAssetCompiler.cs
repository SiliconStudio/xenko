// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Extensions;
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
    public class VertexDataBuilder
    {
        public BoundingBox BoundingBox = BoundingBox.Empty;
        public List<Vector3> Points = new List<Vector3>();
        public List<int> Indices = new List<int>();

        public VertexDataBuilder()
        {
            
        }

        /// <summary>
        /// Appends another vertex data builder
        /// </summary>
        /// <param name="other"></param>
        public void AppendOther(VertexDataBuilder other)
        {
            // Copy vertices
            int vbase = Points.Count;
            for (int i = 0; i < other.Points.Count; i++)
            {
                Vector3 point = other.Points[i];
                Points.Add(point);
                BoundingBox.Merge(ref BoundingBox, ref point, out BoundingBox);
            }

            // Send indices
            foreach (int index in other.Indices)
            {
                Indices.Add(index + vbase);
            }
        }

        public void AppendArrays(Vector3[] vertices, int[] indices)
        {
            // Copy vertices
            int vbase = Points.Count;
            for (int i = 0; i < vertices.Length; i++)
            {
                Points.Add(vertices[i]);
                BoundingBox.Merge(ref BoundingBox, ref vertices[i], out BoundingBox);
            }

            // Send indices
            foreach (int index in indices)
            {
                Indices.Add(index + vbase);
            }
        }

        /// <summary>
        /// Appends local mesh data transformed with and object transform
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="objectTransform"></param>
        public void AppendMeshData(GeometricMeshData<VertexPositionNormalTexture> meshData, Matrix objectTransform)
        {
            // Transform box points
            int vbase = Points.Count;
            for (int i = 0; i < meshData.Vertices.Length; i++)
            {
                VertexPositionNormalTexture point = meshData.Vertices[i];
                point.Position = Vector3.Transform(point.Position, objectTransform).XYZ();
                Points.Add(point.Position);
                BoundingBox.Merge(ref BoundingBox, ref point.Position, out BoundingBox);
            }

            // Send indices
            for (int i = 0; i < meshData.Indices.Length; i++)
            {
                Indices.Add(meshData.Indices[i] + vbase);
            }
        }
    }

    public class NavigationMeshBuildCache
    {
        public class Build
        {
            public class Object
            {
                public Guid Guid;
                public int ParameterHash;
                public VertexDataBuilder Data;
            }

            public Dictionary<Guid, Object> Objects = new Dictionary<Guid, Object>();

            public NavigationMesh NavigationMesh;

            public int SettingsHash = 0;

            // Adds a new object that is build into the navigation mesh
            public void Add(Entity e, VertexDataBuilder data)
            {
                StaticColliderComponent collider = e.Get<StaticColliderComponent>();
                if (collider != null && collider.IsBlocking)
                {
                    int hash = 0;
                    hash += e.Transform.WorldMatrix.GetHashCode();
                    foreach (var shape in collider.ColliderShapes)
                    {
                        hash += shape.GetHashCode();
                    }
                    Objects.Add(e.Id, new Object
                    {
                        Guid = e.Id,
                        ParameterHash = hash,
                        Data = data
                    });
                }
            }

            public bool IsUpdatedOrNew(Entity newEntity)
            {
                Object existingObject;
                StaticColliderComponent collider = newEntity.Get<StaticColliderComponent>();
                if (Objects.TryGetValue(newEntity.Id, out existingObject))
                {
                    if (collider != null && collider.IsBlocking)
                    {
                        int hash = 0;
                        hash += newEntity.Transform.WorldMatrix.GetHashCode();
                        foreach (var shape in collider.ColliderShapes)
                        {
                            hash += shape.GetHashCode();
                        }
                        return (hash != existingObject.ParameterHash);
                    }
                }
                return true;
            }

            public List<BoundingBox> GetRemovedAreas(List<Entity> entities)
            {
                List<BoundingBox> ret = new List<BoundingBox>();

                HashSet<Guid> inputHashSet = new HashSet<Guid>();
                foreach (Entity e in entities)
                {
                    StaticColliderComponent collider = e.Get<StaticColliderComponent>();
                    if(collider != null && collider.IsBlocking)
                        inputHashSet.Add(e.Id);
                }
                foreach (var p in Objects)
                {
                    if (!inputHashSet.Contains(p.Key))
                    {
                        ret.Add(p.Value.Data.BoundingBox);
                    }
                }

                return ret;
            }
        }

        private Dictionary<string, Build> buildAssets = new Dictionary<string, Build>();

        public void AddBuild(string targetUrl, Build build)
        {
            buildAssets.Remove(targetUrl);
            buildAssets.Add(targetUrl, build);
        }
        public Build FindBuild(string targetUrl)
        {
            Build build;
            if (!buildAssets.TryGetValue(targetUrl, out build))
                return null;
            return build;
        }
    }

    class NavigationMeshAssetCompiler : AssetCompilerBase
    {
        private NavigationMeshBuildCache buildCache = new NavigationMeshBuildCache();

        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (NavigationMeshAsset)assetItem.Asset;
            result.BuildSteps = new AssetBuildStep(assetItem) { new NavmeshBuildCommand(targetUrlInStorage, assetItem, asset, context, buildCache) };
        }

        // DEBUG FUNCTIONS
        public static void DumpObj(string name, Vector3[] meshData)
        {
            string filePath = @"C:\Users\g-gj-waals\Desktop\" + name + ".obj";
            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < meshData.Length; i++)
                {
                    Vector3 vert = meshData[i];
                    sw.WriteLine("v {0} {1} {2}", vert.X, vert.Y, vert.Z);
                }

                int numFaces = meshData.Length/3;
                for (int i = 0; i < numFaces; i++)
                {
                    int start = 1 + i*3;
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
            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < meshData.Vertices.Length; i++)
                {
                    VertexPositionNormalTexture vert = meshData.Vertices[i];
                    sw.WriteLine("v {0} {1} {2}", vert.Position.X, vert.Position.Y, vert.Position.Z);
                }

                int numFaces = meshData.Indices.Length/3;
                for (int i = 0; i < numFaces; i++)
                {
                    sw.WriteLine("f {0} {1} {2}",
                        meshData.Indices[i*3 + 0] + 1,
                        meshData.Indices[i*3 + 1] + 1,
                        meshData.Indices[i*3 + 2] + 1);
                }
                sw.Flush();
                file.Flush();
            }
        }

        public static void DumpBinary(string name, byte[] data)
        {
            string filePath = @"C:\Users\g-gj-waals\Desktop\" + name;
            using (FileStream file = File.OpenWrite(filePath))
            {
                file.Write(data, 0, data.Length);
            }
        }

        public static void GenerateTangentBitangent(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
        {
            if (Math.Abs(normal.Y) < 0.01f)
                tangent = new Vector3(normal.Z, normal.Y, -normal.X);
            else
                tangent = new Vector3(-normal.Y, normal.X, normal.Z);
            tangent.Normalize();
            bitangent = Vector3.Cross(normal, tangent);
            tangent = Vector3.Cross(bitangent, normal);
        }

        public static void BuildPlanePoints(ref Plane plane, float size, out Vector3[] points, out int[] inds)
        {
            Vector3 up = plane.Normal;
            Vector3 right;
            Vector3 forward;
            GenerateTangentBitangent(up, out right, out forward);

            points = new Vector3[4];
            points[0] = -forward*size - right*size + up*plane.D;
            points[1] = -forward*size + right*size + up*plane.D;
            points[2] = forward*size - right*size + up*plane.D;
            points[3] = forward*size + right*size + up*plane.D;

            inds = new int[6];
            // CCW
            inds[0] = 0;
            inds[1] = 2;
            inds[2] = 1;
            inds[3] = 1;
            inds[4] = 2;
            inds[5] = 3;
        }

        private class NavmeshBuildCommand : AssetCommand<NavigationMeshAsset>
        {
            private NavigationMeshBuildCache buildCache;
            private NavigationMeshBuildCache.Build oldBuild;
            private NavigationMeshBuildCache.Build currentBuild;

            private UFile assetUrl;
            private readonly AssetItem assetItem;
            private NavigationMeshAsset asset;
            private readonly Package package;

            // Combined scene data to create input meshData
            public VertexDataBuilder sceneVertexDataBuilder;

            List<BoundingBox> updatedAreas = new List<BoundingBox>();
            private bool fullRebuild = false;

            // Automatically calculated bounding box
            private NavigationMeshBuildSettings buildSettings;

            // Deferred shapes such as infinite planes which should be added after the bounding box of the scene is generated
            struct DeferredShape
            {
                public Matrix Transform;
                public IColliderShapeDesc Description;
                public Entity Entity;
            }

            private List<DeferredShape> deferredShapes = new List<DeferredShape>();

            public NavmeshBuildCommand(string url, AssetItem assetItem, NavigationMeshAsset value, AssetCompilerContext context, NavigationMeshBuildCache buildCache)
                : base(url, value)
            {
                this.buildCache = buildCache;
                this.asset = value;
                this.assetItem = assetItem;
                this.package = assetItem.Package;
                assetUrl = url;
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                // We also want to serialize recursively the compile-time dependent assets
                // (since they are not added as reference but actually embedded as part of the current asset)
                // TODO: Ideally we would want to put that automatically in AssetCommand<>, but we would need access to package
                ComputeCompileTimeDependenciesHash(package, writer, Parameters);
            }
            
            private void CollectInputGeometry(List<Entity> sceneEntities)
            {
                // Reset state
                updatedAreas.Clear();
                fullRebuild = oldBuild == null ? true : false;

                sceneVertexDataBuilder = new VertexDataBuilder();

                // Generate collision triangles for all static colliders
                List<StaticColliderComponent> staticColliders = new List<StaticColliderComponent>();

                foreach (var entity in sceneEntities)
                {
                    if (oldBuild?.IsUpdatedOrNew(entity) ?? true)
                    {
                        TransformComponent entityTransform = entity.Transform;
                        Matrix entityWorldMatrix = entityTransform.WorldMatrix;

                        VertexDataBuilder entityVertexDataBuilder = new VertexDataBuilder();

                        StaticColliderComponent collider = entity.Get<StaticColliderComponent>();
                        if (collider != null && collider.IsBlocking && collider.Enabled)
                        {
                            collider.ComposeShape();
                            if (collider.ColliderShape == null)
                                continue; // No collider

                            // Interate through all the colliders shapes while queueing all shapes in compound shapes to process those as well
                            Queue<ColliderShape> shapesToProcess = new Queue<ColliderShape>();
                            shapesToProcess.Enqueue(collider.ColliderShape);
                            while (!shapesToProcess.IsEmpty())
                            {
                                var shape = shapesToProcess.Dequeue();
                                var shapeType = shape.GetType();
                                if (shapeType == typeof(BoxColliderShape))
                                {
                                    var box = (BoxColliderShape)shape;
                                    var boxDesc = (BoxColliderShapeDesc)box.Description;
                                    Matrix transform = box.PositiveCenterMatrix*entityWorldMatrix;

                                    var meshData = GeometricPrimitive.Cube.New(boxDesc.Size);
                                    entityVertexDataBuilder.AppendMeshData(meshData, transform);
                                }
                                else if (shapeType == typeof(SphereColliderShape))
                                {
                                    var sphere = (SphereColliderShape)shape;
                                    var sphereDesc = (SphereColliderShapeDesc)sphere.Description;
                                    Matrix transform = sphere.PositiveCenterMatrix*entityWorldMatrix;

                                    var meshData = GeometricPrimitive.Sphere.New(sphereDesc.Radius);
                                    entityVertexDataBuilder.AppendMeshData(meshData, transform);
                                }
                                else if (shapeType == typeof(CylinderColliderShape))
                                {
                                    var cylinder = (CylinderColliderShape)shape;
                                    var cylinderDesc = (CylinderColliderShapeDesc)cylinder.Description;
                                    Matrix transform = cylinder.PositiveCenterMatrix*entityWorldMatrix;

                                    var meshData = GeometricPrimitive.Cylinder.New(cylinderDesc.Height, cylinderDesc.Radius);
                                    entityVertexDataBuilder.AppendMeshData(meshData, transform);
                                }
                                else if (shapeType == typeof(CapsuleColliderShape))
                                {
                                    var capsule = (CapsuleColliderShape)shape;
                                    var capsuleDesc = (CapsuleColliderShapeDesc)capsule.Description;
                                    Matrix transform = capsule.PositiveCenterMatrix*entityWorldMatrix;

                                    var meshData = GeometricPrimitive.Capsule.New(capsuleDesc.Length, capsuleDesc.Radius);
                                    entityVertexDataBuilder.AppendMeshData(meshData, transform);
                                }
                                else if (shapeType == typeof(StaticPlaneColliderShape))
                                {
                                    var plane = (StaticPlaneColliderShape)shape;
                                    var planeDesc = (StaticPlaneColliderShapeDesc)plane.Description;
                                    Matrix transform = plane.PositiveCenterMatrix*entityWorldMatrix;

                                    // Defer infinite planes because their size is not defined yet
                                    deferredShapes.Add(new DeferredShape { Description = planeDesc, Transform = transform, Entity = entity });
                                }
                                else if (shapeType == typeof(ConvexHullColliderShape))
                                {
                                    // TODO: Fix loading of hull assets
                                    var hull = (ConvexHullColliderShape)shape;
                                    var hullDesc = (ConvexHullColliderShapeDesc)hull.Description;
                                    Matrix transform = hull.PositiveCenterMatrix*entityWorldMatrix;
                                }
                                else if (shapeType == typeof(CompoundColliderShape))
                                {
                                    // Unroll compound collider shapes
                                    var compound = (CompoundColliderShape)shape;
                                    for (int i = 0; i < compound.Count; i++)
                                    {
                                        shapesToProcess.Enqueue(compound[i]);
                                    }
                                }
                            }
                            
                            // Store current entity in build cache
                            currentBuild.Add(entity, entityVertexDataBuilder);
                            
                            // Add (?old) and new bounding box to modified areas
                            sceneVertexDataBuilder.AppendOther(entityVertexDataBuilder);
                            NavigationMeshBuildCache.Build.Object oldObject = null;
                            if (oldBuild?.Objects.TryGetValue(entity.Id, out oldObject) ?? false)
                            {
                                updatedAreas.Add(oldObject.Data.BoundingBox);
                            }
                            updatedAreas.Add(entityVertexDataBuilder.BoundingBox);

                            // TODO: Remove this
                            string fullEntityName = entity.Name;
                            Entity findParentEntity = entity.GetParent();
                            while (findParentEntity != null)
                            {
                                fullEntityName = findParentEntity.Name + "/" + fullEntityName;
                                findParentEntity = findParentEntity.GetParent();
                            }
                            System.Diagnostics.Debug.WriteLine($"Entity changed \"{fullEntityName}\" TL:{entityTransform.WorldMatrix.TranslationVector}");
                        }
                    }
                    else
                    {
                        // Copy old data into vertex buffer
                        NavigationMeshBuildCache.Build.Object oldObject = oldBuild.Objects[entity.Id];
                        sceneVertexDataBuilder.AppendOther(oldObject.Data);
                        currentBuild.Add(entity, oldObject.Data);
                    }
                }
                
                // Store calculated bounding box
                buildSettings.BoundingBox = sceneVertexDataBuilder.BoundingBox;

                // Process deferred shapes
                Vector3 bbExtent = buildSettings.BoundingBox.Extent;
                BoundingBox boundingBox = sceneVertexDataBuilder.BoundingBox;
                Vector3 maxSize = boundingBox.Maximum - boundingBox.Minimum;
                float maxDiagonal = Math.Max(maxSize.X, Math.Max(maxSize.Y, maxSize.Z));
                foreach (DeferredShape shape in deferredShapes)
                {
                    StaticPlaneColliderShapeDesc planeDesc = (StaticPlaneColliderShapeDesc)shape.Description;
                    Plane plane = new Plane(planeDesc.Normal, planeDesc.Offset);

                    // Pre-Transform plane parameters
                    plane.Normal = Vector3.TransformNormal(plane.Normal, shape.Transform);
                    float offset = Vector3.Dot(shape.Transform.TranslationVector, plane.Normal);
                    plane.D += offset;

                    // Generate source plane triangles
                    Vector3[] planePoints;
                    int[] planeInds;
                    BuildPlanePoints(ref plane, maxDiagonal, out planePoints, out planeInds);

                    // TODO: Cache this and use it for the BuildPlanePoints as well
                    Vector3 tangent, bitangent;
                    GenerateTangentBitangent(plane.Normal, out tangent, out bitangent);
                    // Calculate plane offset so that the plane always covers the whole range of the bounding box
                    Vector3 planeOffset = Vector3.Dot(boundingBox.Center, tangent)*tangent;
                    planeOffset += Vector3.Dot(boundingBox.Center, bitangent)*bitangent;

                    VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[planePoints.Length];
                    for (int i = 0; i < planePoints.Length; i++)
                    {
                        vertices[i] = new VertexPositionNormalTexture(planePoints[i] + planeOffset, Vector3.UnitY, Vector2.Zero);
                    }

                    GeometricMeshData<VertexPositionNormalTexture> meshData = new GeometricMeshData<VertexPositionNormalTexture>(vertices, planeInds, false);
                    sceneVertexDataBuilder.AppendMeshData(meshData, Matrix.Identity);

                    if (oldBuild?.IsUpdatedOrNew(shape.Entity) ?? false)
                    {
                        // NOTE: Force a full rebuild when moving unbound shapes such as ininite planes
                        fullRebuild = true;
                    }
                }
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // Build cache items to build incrementally
                currentBuild = new NavigationMeshBuildCache.Build();
                oldBuild = buildCache.FindBuild(assetUrl);

                // The output object of the compilation
                NavigationMesh generatedNavigationMesh = new NavigationMesh();

                // No scene specified, result in failure
                if (asset.DefaultScene == null)
                    return Task.FromResult(ResultStatus.Failed);

                // Copy build settings so we can modify them
                buildSettings = asset.BuildSettings;

                // Check for tile size
                if (buildSettings.TileSize <= 0)
                    return Task.FromResult(ResultStatus.Failed);

                var assetManager = new ContentManager();
                string sceneUrl = AttachedReferenceManager.GetUrl(asset.DefaultScene);
                var sceneAsset = (SceneAsset)package.Session.FindAsset(sceneUrl)?.Asset;

                if (sceneAsset == null)
                    return Task.FromResult(ResultStatus.Failed);

                // Turn the entire entity hierarchy into a single list
                List<Entity> sceneEntities = sceneAsset.Hierarchy.Parts.Select(x => x.Entity).ToList();

                // Update world matrices
                foreach (Entity e in sceneEntities)
                {
                    e.Transform.UpdateWorldMatrix();
                }

                // This collects all the input geometry, calculates the modified areas and calculates the scene bounds
                CollectInputGeometry(sceneEntities);
                List<BoundingBox> removedAreas = oldBuild?.GetRemovedAreas(sceneEntities);
                
                BoundingBox boundingBox = sceneVertexDataBuilder.BoundingBox;
                buildSettings.BoundingBox = boundingBox;
                // Can't generate when no bounding box is specified
                if (boundingBox == BoundingBox.Empty)
                    return Task.FromResult(ResultStatus.Failed);

                // Turn generated data into arrays
                Vector3[] meshVertices = sceneVertexDataBuilder.Points.ToArray();
                int[] meshIndices = sceneVertexDataBuilder.Indices.ToArray();

                // NOTE: Reversed winding order as input to recast
                int[] flipIndices = { 0, 2, 1 };
                int numSrcTriangles = meshIndices.Length / 3;
                for (int i = 0; i < numSrcTriangles; i++)
                {
                    int j = meshIndices[i * 3 + 1];
                    meshIndices[i * 3 + 1] = meshIndices[i * 3 + 2];
                    meshIndices[i * 3 + 2] = j;
                }

                // Debug output input mesh
                // TODO: Remove this
                //DumpObj("input",  meshVertices.ToArray());

                // Check if settings changed to trigger a full rebuild
                int currentSettingsHash = asset.BuildSettings.GetHashCode();
                foreach (var agentSetting in asset.NavigationMeshAgentSettings)
                {
                    currentSettingsHash += agentSetting.GetHashCode();
                }
                currentBuild.SettingsHash = currentSettingsHash;
                if (oldBuild != null && oldBuild.SettingsHash != currentBuild.SettingsHash)
                {
                    fullRebuild = true;
                }

                // Flag tiles to build
                HashSet<Point> tilesToBuild = new HashSet<Point>();
                if (fullRebuild)
                {
                    // For full rebuild just take the root bounding box for selecting tiles to build
                    List<Point> newTileList = NavigationMesh.GetOverlappingTiles(buildSettings, boundingBox);
                    foreach (Point p in newTileList)
                        tilesToBuild.Add(p);

                    generatedNavigationMesh.ClearTiles();
                }
                else
                {
                    foreach (var update in updatedAreas)
                    {
                        List<Point> newTileList = NavigationMesh.GetOverlappingTiles(buildSettings, update);
                        foreach (Point p in newTileList)
                            tilesToBuild.Add(p);
                    }
                    foreach (var update in removedAreas)
                    {
                        List<Point> newTileList = NavigationMesh.GetOverlappingTiles(buildSettings, update);
                        foreach (Point p in newTileList)
                            tilesToBuild.Add(p);
                    }
                }

                if (oldBuild == null || fullRebuild)
                {
                    // Initialize navigation mesh
                    generatedNavigationMesh.Initialize(buildSettings, asset.NavigationMeshAgentSettings.ToArray());
                }
                else
                {
                    // Perform incremental build on old navigation mesh
                    generatedNavigationMesh = oldBuild.NavigationMesh;
                }
                currentBuild.NavigationMesh = generatedNavigationMesh;

                // Build tiles
                System.Diagnostics.Debug.WriteLine($"Rebuilding {tilesToBuild.Count} tiles for {asset.NavigationMeshAgentSettings.Count} agent settings for navmesh {assetUrl}");
                foreach (var tileToBuild in tilesToBuild)
                {
                    BoundingBox tileBoundingBox = NavigationMesh.ClampBoundingBoxToTile(buildSettings, boundingBox, tileToBuild);
                    List<NavigationMesh.Tile> buildTiles = generatedNavigationMesh.BuildTile(
                        meshVertices.ToArray(), meshIndices.ToArray(), 
                        tileBoundingBox, tileToBuild);

                    // TODO: Remove this
                    //int layer = 0;
                    //foreach (var tile in buildTiles)
                    //{
                    //    if(tile.MeshVertices != null)
                    //        DumpObj($"Tiles\\tile_{tileToBuild.X}_{tileToBuild.Y}_{layer++}", tile.MeshVertices);
                    //}
                }
                
                assetManager.Save(assetUrl, generatedNavigationMesh);
                buildCache.AddBuild(assetUrl, currentBuild);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}