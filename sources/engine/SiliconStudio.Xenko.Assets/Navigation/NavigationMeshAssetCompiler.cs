// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using SharpDX.Text;
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
                if (collider != null)
                {
                    int hash = 0;
                    hash += e.Transform.WorldMatrix.GetHashCode();
                    hash += 379 * collider.CollisionGroup.GetHashCode();
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
                    if (collider != null)
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
                    if(collider != null)
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

        // TODO: Remove this
        // DEBUG FUNCTIONS
        public static void DumpObj(string filePath, Vector3[] meshData, int[] indexData = null)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;

            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < meshData.Length; i++)
                {
                    Vector3 vert = meshData[i];
                    sw.WriteLine("v {0} {1} {2}", vert.X, vert.Y, vert.Z);
                }

                if (indexData == null)
                {
                    int numFaces = meshData.Length/3;
                    for (int i = 0; i < numFaces; i++)
                    {
                        int start = 1 + i*3;
                        sw.WriteLine("f {0} {1} {2}",
                            start + 0,
                            start + 1,
                            start + 2);
                    }
                }
                else
                {
                    int numFaces = indexData.Length/3;
                    for (int i = 0; i < numFaces; i++)
                    {
                        sw.WriteLine("f {0} {1} {2}",
                            indexData[i*3] + 1,
                            indexData[i*3 + 1] + 1,
                            indexData[i*3 + 2] + 1);
                    }
                }
                sw.Flush();
                file.Flush();
            }
        }
        
        public static void DumpObj(string filePath, GeometricMeshData<VertexPositionNormalTexture> meshData)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;

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

        public static void DumpBinary(string filePath, byte[] data)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;
            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                file.Write(data, 0, data.Length);
            }
        }

        public static unsafe void DumpTiles(float tileCellSize, NavigationMesh.Tile[] tiles)
        {
            string filePath = "F:\\Projects\\recast\\RecastDemo\\Bin\\all_tiles_navmesh.bin";
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;

            using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter stream = new BinaryWriter(file))
            {
                byte[] NAVMESHSET_MAGIC = Encoding.ASCII.GetBytes("MSET");
                int version = 1;
                stream.Write(NAVMESHSET_MAGIC);
                stream.Write(version);
                stream.Write(tiles.Length);
                // Orig
                stream.Write(0.0f);
                stream.Write(0.0f);
                stream.Write(0.0f);
                stream.Write(tileCellSize); // W
                stream.Write(tileCellSize); // H
                int maxTiles = 1 << 14;
                int maxPolys = 1 << (22 - 14);
                stream.Write(maxTiles); // MaxTiles
                stream.Write(maxPolys);

                // Write tiles
                for (int i = 0; i < tiles.Length; i++)
                {
                    var tile = tiles[i];
                    stream.Write(0); // Tile Ref
                    stream.Write(tile.NavmeshData.Length); // Data size
                    stream.Write(tile.NavmeshData);
                }
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

        public static void ExtendBoundingBox(ref BoundingBox boundingBox, Vector3 offsets)
        {
            boundingBox.Minimum -= offsets;
            boundingBox.Maximum += offsets;
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
            private VertexDataBuilder sceneVertexDataBuilder;
            private BoundingBox globalBoundingBox;
            private bool generateBoundingBox;

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
                public VertexDataBuilder VertexDataBuilder;
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
                // We also want to serialize the compile-time dependent assets recursively
                // (since they are not added as a reference but are actually embedded as a part of the current asset)
                // TODO: Ideally we would want to put that automatically in AssetCommand<>, but we would need access to package
                ComputeCompileTimeDependenciesHash(package, writer, Parameters);
            }
            
            private void CollectInputGeometry(List<Entity> sceneEntities)
            {
                // Reset state
                updatedAreas.Clear();
                fullRebuild = oldBuild == null;

                sceneVertexDataBuilder = new VertexDataBuilder();

                // Generate collision triangles for all static colliders
                List<StaticColliderComponent> staticColliders = new List<StaticColliderComponent>();

                foreach (var entity in sceneEntities)
                {
                    StaticColliderComponent collider = entity.Get<StaticColliderComponent>();
                    
                    bool colliderEnabled = collider != null && ((CollisionFilterGroupFlags)collider.CollisionGroup & asset.AllowedCollisionGroups) != 0 && collider.Enabled;
                    if (!colliderEnabled) // Removed or disabled
                    {
                        // Check for old object
                        NavigationMeshBuildCache.Build.Object oldObject = null;
                        if (oldBuild?.Objects.TryGetValue(entity.Id, out oldObject) ?? false)
                        {
                            // This object has been disabled, update the area because it was removed and continue to the next object
                            updatedAreas.Add(oldObject.Data.BoundingBox);
                        }
                    }
                    else if (oldBuild?.IsUpdatedOrNew(entity) ?? true) // Updated?
                    {
                        TransformComponent entityTransform = entity.Transform;
                        Matrix entityWorldMatrix = entityTransform.WorldMatrix;

                        VertexDataBuilder entityVertexDataBuilder = new VertexDataBuilder();

                        collider.ComposeShape();
                        if (collider.ColliderShape == null)
                            continue; // No collider

                        bool isDeferred = false;

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
                                deferredShapes.Add(new DeferredShape
                                {
                                    Description = planeDesc,
                                    Transform = transform,
                                    Entity = entity,
                                    VertexDataBuilder = entityVertexDataBuilder

                                });
                                isDeferred = true;
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

                        if (!isDeferred)
                        {
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
                        }
                    }
                    else // Not updated
                    {
                        // Copy old data into vertex buffer
                        NavigationMeshBuildCache.Build.Object oldObject = oldBuild.Objects[entity.Id];
                        sceneVertexDataBuilder.AppendOther(oldObject.Data);
                        currentBuild.Add(entity, oldObject.Data);
                    }
                }

                // Store calculated bounding box
                if(generateBoundingBox)
                    globalBoundingBox = sceneVertexDataBuilder.BoundingBox;

                // Process deferred shapes
                Vector3 maxSize = globalBoundingBox.Maximum - globalBoundingBox.Minimum;
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
                    Vector3 planeOffset = Vector3.Dot(globalBoundingBox.Center, tangent)*tangent;
                    planeOffset += Vector3.Dot(globalBoundingBox.Center, bitangent)*bitangent;

                    VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[planePoints.Length];
                    for (int i = 0; i < planePoints.Length; i++)
                    {
                        vertices[i] = new VertexPositionNormalTexture(planePoints[i] + planeOffset, Vector3.UnitY, Vector2.Zero);
                    }

                    GeometricMeshData<VertexPositionNormalTexture> meshData = new GeometricMeshData<VertexPositionNormalTexture>(vertices, planeInds, false);
                    shape.VertexDataBuilder.AppendMeshData(meshData, Matrix.Identity);
                    sceneVertexDataBuilder.AppendMeshData(meshData, Matrix.Identity);

                    // Store deferred shape in build cahce just like normal onesdddd
                    currentBuild.Add(shape.Entity, shape.VertexDataBuilder);


                    // NOTE: Force a full rebuild when moving unbound shapes such as ininite planes
                    fullRebuild = true;
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

                if (asset.AutoGenerateBoundingBox)
                {
                    generateBoundingBox = true;
                    globalBoundingBox = BoundingBox.Empty;
                }
                else
                {
                    generateBoundingBox = false;
                    globalBoundingBox = asset.BoundingBox;
                }

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
                
                BoundingBox boundingBox = globalBoundingBox;
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
                string objPath = @"F:\Projects\recast\RecastDemo\Bin\Meshes\input.obj";
                DumpObj(objPath, meshVertices.ToArray(), meshIndices.ToArray());

                // Check if settings changed to trigger a full rebuild
                int currentSettingsHash = asset.GetHashCode();
                currentBuild.SettingsHash = currentSettingsHash;
                if (oldBuild != null && oldBuild.SettingsHash != currentBuild.SettingsHash)
                {
                    fullRebuild = true;
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

                // Generate all the layers corresponding to the various agent settings
                for(int layer = 0; layer < asset.NavigationMeshAgentSettings.Count; layer++)
                {
                    Stopwatch layerBuildTimer = new Stopwatch();
                    layerBuildTimer.Start();
                    var agentSetting = asset.NavigationMeshAgentSettings[layer];

                    // Flag tiles to build for this specific layer
                    HashSet<Point> tilesToBuild = new HashSet<Point>();
                    Vector3 boundingBoxOffset = new Vector3(0, 0, 0);
                    if (fullRebuild)
                    {
                        // For full rebuild just take the root bounding box for selecting tiles to build
                        List<Point> newTileList = NavigationMesh.GetOverlappingTiles(buildSettings, boundingBox);
                        foreach (Point p in newTileList)
                            tilesToBuild.Add(p);
                    }
                    else
                    {
                        // Apply an offset so their neighbouring tiles which are affected by the agent radius also get rebuild
                        Vector3 agentOffset = new Vector3(agentSetting.Radius, 0, agentSetting.Radius);
                        if(removedAreas != null)
                            updatedAreas.AddRange(removedAreas);
                        foreach (var update in updatedAreas)
                        {
                            BoundingBox agentSpecificBoundingBox = new BoundingBox
                            {
                                Minimum = update.Minimum - agentOffset,
                                Maximum = update.Maximum + agentOffset,
                            };
                            List<Point> newTileList = NavigationMesh.GetOverlappingTiles(buildSettings, agentSpecificBoundingBox);
                            foreach (Point p in newTileList)
                                tilesToBuild.Add(p);
                        }
                    }

                    // Build tiles
                    foreach (var tileToBuild in tilesToBuild)
                    {
                        BoundingBox tileBoundingBox = NavigationMesh.ClampBoundingBoxToTile(buildSettings, boundingBox, tileToBuild);
                        if (boundingBox.Contains(ref tileBoundingBox) == ContainmentType.Disjoint)
                        {
                            generatedNavigationMesh.RemoveLayerTile(layer, tileToBuild);
                            continue;
                        }
                        NavigationMesh.Tile buildTile = generatedNavigationMesh.BuildLayerTile(layer,
                            meshVertices.ToArray(), meshIndices.ToArray(), tileBoundingBox, tileToBuild);

                        // TODO: Remove this
                        //if(buildTile.MeshVertices != null)
                        //    DumpObj($"Tiles\\layer_{layer}_tile_{tileToBuild.X}_{tileToBuild.Y}", buildTile.MeshVertices);
                    }
                    // TODO: Remove this
                    Debug.WriteLine($"Rebuilt {tilesToBuild.Count} tiles for layer {layer} for navmesh for {sceneUrl}/{assetUrl} in {layerBuildTimer.Elapsed.TotalMilliseconds}ms");

                    // TODO: Remove
                    // Dump layer data
                    //NavigationMesh.Tile[] tiles = generatedNavigationMesh.Layers[layer].Tiles.Select((p) => p.Value).ToArray();
                    //DumpTiles(buildSettings.TileSize*buildSettings.CellSize, tiles);
                }

                assetManager.Save(assetUrl, generatedNavigationMesh);
                buildCache.AddBuild(assetUrl, currentBuild);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}