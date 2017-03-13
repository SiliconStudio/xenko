using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Incremental navigation mesh builder. 
    /// Builds the navigation mesh in individual tiles
    /// </summary>
    public class NavigationMeshBuilder
    {
        /// <summary>
        /// Global offset applied to all input colliders processed by this builder
        /// </summary>
        public Vector3 Offset = Vector3.Zero;

        private NavigationMesh lastNavigationMesh;

        // TODO: Space partitioning
        private List<StaticColliderData> colliders = new List<StaticColliderData>();
        private HashSet<Guid> registeredGuids = new HashSet<Guid>();

        /// <summary>
        /// Initializes the builder, optionally with a previous navigation mesh
        /// </summary>
        /// <param name="lastNavigationMesh">The previous navigation mesh, to allow incremental builds</param>
        public NavigationMeshBuilder(NavigationMesh lastNavigationMesh = null)
        {
            this.lastNavigationMesh = lastNavigationMesh;
        }

        public void Add(StaticColliderData colliderData)
        {
            lock (colliders)
            {
                if(registeredGuids.Contains(colliderData.Component.Id))
                    throw new InvalidOperationException("Duplicate collider added");
                colliders.Add(colliderData);
                registeredGuids.Add(colliderData.Component.Id);
            }
        }

        public void Remove(StaticColliderData colliderData)
        {
            lock (colliders)
            {
                if (!registeredGuids.Contains(colliderData.Component.Id))
                    throw new InvalidOperationException("Trying to remove unregistered collider");
                colliders.Remove(colliderData);
                registeredGuids.Remove(colliderData.Component.Id);
            }
        }

        public NavigationMeshBuildResult Build(NavigationMeshBuildSettings buildSettings, ICollection<NavigationAgentSettings> agentSettings, CollisionFilterGroupFlags includedCollisionGroups, 
            ICollection<BoundingBox> boundingBoxes, CancellationToken cancellationToken)
        {
            var lastCache = lastNavigationMesh?.Cache;
            var result = new NavigationMeshBuildResult();

            if (agentSettings.Count == 0)
                return result;

            if (boundingBoxes.Count == 0)
                return new NavigationMeshBuildResult();
            
            var settingsHash = agentSettings?.ComputeHash() ?? 0;
            settingsHash = (settingsHash * 397) ^ buildSettings.GetHashCode();
            if (lastCache != null && lastCache.SettingsHash != settingsHash)
            {
                // Start from scratch if settings changed
                lastNavigationMesh = null;
            }

            // TODO layers
            var agentSettings0 = agentSettings.First();

            // Copy colliders so the collection doesn't get modified
            StaticColliderData[] collidersLocal;
            lock (colliders)
            {
                collidersLocal = colliders.ToArray();
            }

            BuildInput(collidersLocal, includedCollisionGroups);

            // Check if cache was cleared while building the input
            lastCache = lastNavigationMesh?.Cache;

            // The new navigation mesh that will be created
            result.NavigationMesh = new NavigationMesh();

            // Tile cache for this new navigation mesh
            NavigationMeshCache newCache = result.NavigationMesh.Cache = new NavigationMeshCache();
            newCache.SettingsHash = settingsHash;

            // Generate global bounding box for planes
            BoundingBox globalBoundingBox = BoundingBox.Empty;
            foreach (var boundingBox in boundingBoxes)
            {
                globalBoundingBox = BoundingBox.Merge(boundingBox, globalBoundingBox);
            }

            // Combine input and collect tiles to build
            HashSet<Point> tilesToBuild = new HashSet<Point>();
            NavigationMeshInputBuilder sceneNavigationMeshInputBuilder = new NavigationMeshInputBuilder();
            foreach (var colliderData in collidersLocal)
            {
                if (colliderData.InputBuilder == null)
                    continue;

                if (colliderData.Processed)
                {
                    MarkTiles(colliderData.InputBuilder, ref buildSettings, ref agentSettings0, tilesToBuild);
                    if (colliderData.Previous != null)
                        MarkTiles(colliderData.Previous.InputBuilder, ref buildSettings, ref agentSettings0, tilesToBuild);
                }

                // Otherwise, skip building these tiles
                sceneNavigationMeshInputBuilder.AppendOther(colliderData.InputBuilder);
                newCache.Add(colliderData.Component, colliderData.InputBuilder, colliderData.Planes, colliderData.ParameterHash);

                // Generate geometry for planes
                foreach (var plane in colliderData.Planes)
                {
                    sceneNavigationMeshInputBuilder.AppendOther(BuildPlaneGeometry(plane, globalBoundingBox));
                }
            }


            // Check for removed colliders
            if (lastCache != null)
            {
                foreach (var obj in lastCache.Objects)
                {
                    if (!newCache.Objects.ContainsKey(obj.Key))
                    {
                        MarkTiles(obj.Value.InputBuilder, ref buildSettings, ref agentSettings0, tilesToBuild);
                    }
                }
            }

            // Calculate updated/added bounding boxes
            foreach (var boundingBox in boundingBoxes)
            {
                if (!lastCache?.BoundingBoxes.Contains(boundingBox) ?? true) // In the case of no case, mark all tiles in all bounding boxes to be rebuilt
                {
                    var tiles = NavigationMeshBuildUtils.GetOverlappingTiles(buildSettings, boundingBox);
                    foreach (var tile in tiles)
                    {
                        tilesToBuild.Add(tile);
                    }
                }
            }

            // Check for removed bounding boxes
            if (lastCache != null)
            {
                foreach (var boundingBox in lastCache.BoundingBoxes)
                {
                    if (!boundingBoxes.Contains(boundingBox))
                    {
                        var tiles = NavigationMeshBuildUtils.GetOverlappingTiles(buildSettings, boundingBox);
                        foreach (var tile in tiles)
                        {
                            tilesToBuild.Add(tile);
                        }
                    }
                }
            }

            // TODO: Generate tile local mesh input data
            var inputVertices = sceneNavigationMeshInputBuilder.Points.ToArray();
            var inputIndices = sceneNavigationMeshInputBuilder.Indices.ToArray();

            long buildTimeStamp = DateTime.UtcNow.Ticks;

            // TODO can't use tilesToBuild directly
            ConcurrentCollector<Tuple<Point, NavigationMeshTile>> builtTiles = new ConcurrentCollector<Tuple<Point, NavigationMeshTile>>(tilesToBuild.Count);
            Dispatcher.ForEach(tilesToBuild.ToArray(), tileCoordinate =>
            {
                // Allow cancellation while building tiles
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Builds the tile, or returns null when there is nothing generated for this tile (empty tile)
                NavigationMeshTile meshTile = BuildTile(tileCoordinate, buildSettings, agentSettings0, boundingBoxes,
                    inputVertices, inputIndices, buildTimeStamp);

                // Add the result to the list of built tiles
                builtTiles.Add(new Tuple<Point, NavigationMeshTile>(tileCoordinate, meshTile));

            });
            if (cancellationToken.IsCancellationRequested)
                return result;

            // TODO
            int numLayers = 1;
            for (int i = 0; i < numLayers; i++)
            {
                var newLayer = new NavigationMeshLayer();
                result.NavigationMesh.LayersInternal.Add(newLayer);

                // Copy tiles from previous build into new build
                if (lastNavigationMesh != null && lastNavigationMesh.LayersInternal.Count > i)
                {
                    var sourceLayer = lastNavigationMesh.LayersInternal[i];
                    foreach (var sourceTile in sourceLayer.Tiles)
                        newLayer.TilesInternal.Add(sourceTile.Key, sourceTile.Value);
                }
            }

            var layer = result.NavigationMesh.LayersInternal[0];
            {
                layer.BuildSettings = buildSettings;

                // TODO multiple agent settings
                layer.AgentSettings = agentSettings0;

                foreach (var p in builtTiles)
                {
                    if (p.Item2 == null)
                    {
                        // Remove a tile
                        if (layer.TilesInternal.ContainsKey(p.Item1))
                            layer.TilesInternal.Remove(p.Item1);
                    }
                    else
                    {
                        // Set or update tile
                        layer.TilesInternal[p.Item1] = p.Item2;
                    }
                }
            }

            // Store bounding boxes in new tile cache
            newCache.BoundingBoxes = new List<BoundingBox>(boundingBoxes);

            // Update navigation mesh
            lastNavigationMesh = result.NavigationMesh;

            // TODO: Remove build settings
            lastNavigationMesh.BuildSettings = buildSettings;

            result.Success = true;
            return result;
        }

        private NavigationMeshTile BuildTile(Point tileCoordinate, NavigationMeshBuildSettings buildSettings, NavigationAgentSettings agentSettings, 
            ICollection<BoundingBox> boundingBoxes, Vector3[] inputVertices, int[] inputIndices, long buildTimeStamp)
        {
            NavigationMeshTile meshTile = null;

            // Include bounding boxes in tile height range
            BoundingBox tileBoundingBox = NavigationMeshBuildUtils.CalculateTileBoundingBox(buildSettings, tileCoordinate);
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;
            bool shouldBuildTile = false;
            foreach (var boundingBox in boundingBoxes)
            {
                if (boundingBox.Intersects(ref tileBoundingBox))
                {
                    maximumHeight = Math.Max(maximumHeight, boundingBox.Maximum.Y);
                    minimumHeight = Math.Min(minimumHeight, boundingBox.Minimum.Y);
                    shouldBuildTile = true;
                }
            }

            NavigationMeshBuildUtils.SnapBoundingBoxToCellHeight(buildSettings, ref tileBoundingBox);

            // Skip tiles that do not overlap with any bounding box
            if (shouldBuildTile)
            {
                // Set tile's minimum and maximum height
                tileBoundingBox.Minimum.Y = minimumHeight;
                tileBoundingBox.Maximum.Y = maximumHeight;

                unsafe
                {
                    IntPtr builder = Navigation.CreateBuilder();

                    // Turn build settings into native structure format
                    Navigation.BuildSettings internalBuildSettings = new Navigation.BuildSettings
                    {
                        // Tile settings
                        BoundingBox = tileBoundingBox,
                        TilePosition = tileCoordinate,
                        TileSize = buildSettings.TileSize,

                        // General build settings
                        CellHeight = buildSettings.CellHeight,
                        CellSize = buildSettings.CellSize,
                        RegionMinArea = buildSettings.MinRegionArea,
                        RegionMergeArea = buildSettings.RegionMergeArea,
                        EdgeMaxLen = buildSettings.MaxEdgeLen,
                        EdgeMaxError = buildSettings.MaxEdgeError,
                        DetailSampleDist = buildSettings.DetailSamplingDistance,
                        DetailSampleMaxError = buildSettings.MaxDetailSamplingError,

                        // Agent settings
                        AgentHeight = agentSettings.Height,
                        AgentRadius = agentSettings.Radius,
                        AgentMaxClimb = agentSettings.MaxClimb,
                        AgentMaxSlope = agentSettings.MaxSlope.Degrees
                    };

                    Navigation.SetSettings(builder, new IntPtr(&internalBuildSettings));
                    IntPtr buildResultPtr = Navigation.Build(builder, inputVertices, inputVertices.Length, inputIndices, inputIndices.Length);
                    Navigation.GeneratedData* generatedDataPtr = (Navigation.GeneratedData*)buildResultPtr;
                    if (generatedDataPtr->Success && generatedDataPtr->NavmeshDataLength > 0)
                    {
                        meshTile = new NavigationMeshTile();

                        // Copy the generated navigationMesh data
                        meshTile.Data = new byte[generatedDataPtr->NavmeshDataLength + sizeof(long)];
                        Marshal.Copy(generatedDataPtr->NavmeshData, meshTile.Data, 0, generatedDataPtr->NavmeshDataLength);

                        // Append time stamp
                        byte[] timeStamp = BitConverter.GetBytes(buildTimeStamp);
                        for (int i = 0; i < timeStamp.Length; i++)
                            meshTile.Data[meshTile.Data.Length - sizeof(long) + i] = timeStamp[i];

                        List<Vector3> outputVerts = new List<Vector3>();
                        if (generatedDataPtr->NumNavmeshVertices > 0)
                        {
                            Vector3* navmeshVerts = (Vector3*)generatedDataPtr->NavmeshVertices;
                            for (int j = 0; j < generatedDataPtr->NumNavmeshVertices; j++)
                            {
                                outputVerts.Add(navmeshVerts[j]);
                            }

                            meshTile.MeshVertices = outputVerts.ToArray();
                        }

                        Navigation.DestroyBuilder(builder);
                    }
                }
            }

            return meshTile;
        }

        /// <summary>
        /// Rebuilds outdated triangle data for colliders and recalculates hashes storing everything in StaticColliderData
        /// </summary>
        private void BuildInput(StaticColliderData[] collidersLocal, CollisionFilterGroupFlags includedCollisionGroups)
        {
            NavigationMeshCache lastCache = lastNavigationMesh?.Cache;

            Matrix offsetMatrix = Matrix.Translation(Offset);

            bool clearCache = false;

            // TODO for some reason this call is ambiguous when called directly with a type of List<StaticColliderData>
            Dispatcher.ForEach(collidersLocal, colliderData =>
            {
                var entity = colliderData.Component.Entity;
                TransformComponent entityTransform = entity.Transform;
                Matrix entityWorldMatrix = entityTransform.WorldMatrix * offsetMatrix;

                NavigationMeshInputBuilder entityNavigationMeshInputBuilder = colliderData.InputBuilder = new NavigationMeshInputBuilder();

                // Compute hash of collider and compare it with the previous build if there is one
                colliderData.ParameterHash = NavigationMeshBuildUtils.HashEntityCollider(colliderData.Component);
                colliderData.Previous = null;
                if (lastCache?.Objects.TryGetValue(colliderData.Component.Id, out colliderData.Previous) ?? false)
                {
                    if (colliderData.Previous.ParameterHash == colliderData.ParameterHash)
                    {
                        // In this case, we don't need to recalculate the geometry for this shape, since it wasn't changed
                        // here we take the triangle mesh from the previous build as the current
                        colliderData.InputBuilder = colliderData.Previous.InputBuilder;
                        colliderData.Planes.Clear();
                        colliderData.Planes.AddRange(colliderData.Previous.Planes);
                        colliderData.Processed = false;
                        return;
                    }
                }

                // Return empty data for disabled colliders, filtered out colliders or trigger colliders 
                bool passesFilter = ((CollisionFilterGroupFlags)colliderData.Component.CollisionGroup & includedCollisionGroups) != 0;
                if (!colliderData.Component.Enabled || colliderData.Component.IsTrigger || !passesFilter)
                {
                    colliderData.Processed = true;
                    return;
                }

                // Clear cache on removal of infinite planes
                if (colliderData.Planes.Count > 0)
                    clearCache = true;

                // Clear planes
                colliderData.Planes.Clear();

                // Interate through all the colliders shapes while queueing all shapes in compound shapes to process those as well
                Queue<ColliderShape> shapesToProcess = new Queue<ColliderShape>();
                if (colliderData.Component.ColliderShape != null)
                {
                    shapesToProcess.Enqueue(colliderData.Component.ColliderShape);
                    while (shapesToProcess.Count > 0)
                    {
                        var shape = shapesToProcess.Dequeue();
                        var shapeType = shape.GetType();
                        if (shapeType == typeof(BoxColliderShape))
                        {
                            var box = (BoxColliderShape)shape;
                            var boxDesc = (BoxColliderShapeDesc)box.Description;
                            Matrix transform = box.PositiveCenterMatrix * entityWorldMatrix;

                            var meshData = GeometricPrimitive.Cube.New(boxDesc.Size, toLeftHanded: true);
                            entityNavigationMeshInputBuilder.AppendMeshData(meshData, transform);
                        }
                        else if (shapeType == typeof(SphereColliderShape))
                        {
                            var sphere = (SphereColliderShape)shape;
                            var sphereDesc = (SphereColliderShapeDesc)sphere.Description;
                            Matrix transform = sphere.PositiveCenterMatrix * entityWorldMatrix;

                            var meshData = GeometricPrimitive.Sphere.New(sphereDesc.Radius, toLeftHanded: true);
                            entityNavigationMeshInputBuilder.AppendMeshData(meshData, transform);
                        }
                        else if (shapeType == typeof(CylinderColliderShape))
                        {
                            var cylinder = (CylinderColliderShape)shape;
                            var cylinderDesc = (CylinderColliderShapeDesc)cylinder.Description;
                            Matrix transform = cylinder.PositiveCenterMatrix * entityWorldMatrix;

                            var meshData = GeometricPrimitive.Cylinder.New(cylinderDesc.Height, cylinderDesc.Radius, toLeftHanded: true);
                            entityNavigationMeshInputBuilder.AppendMeshData(meshData, transform);
                        }
                        else if (shapeType == typeof(CapsuleColliderShape))
                        {
                            var capsule = (CapsuleColliderShape)shape;
                            var capsuleDesc = (CapsuleColliderShapeDesc)capsule.Description;
                            Matrix transform = capsule.PositiveCenterMatrix * entityWorldMatrix;

                            var meshData = GeometricPrimitive.Capsule.New(capsuleDesc.Length, capsuleDesc.Radius, toLeftHanded: true);
                            entityNavigationMeshInputBuilder.AppendMeshData(meshData, transform);
                        }
                        else if (shapeType == typeof(ConeColliderShape))
                        {
                            var cone = (ConeColliderShape)shape;
                            var coneDesc = (ConeColliderShapeDesc)cone.Description;
                            Matrix transform = cone.PositiveCenterMatrix * entityWorldMatrix;

                            var meshData = GeometricPrimitive.Cone.New(coneDesc.Radius, coneDesc.Height, toLeftHanded: true);
                            entityNavigationMeshInputBuilder.AppendMeshData(meshData, transform);
                        }
                        else if (shapeType == typeof(StaticPlaneColliderShape))
                        {
                            var planeShape = (StaticPlaneColliderShape)shape;
                            var planeDesc = (StaticPlaneColliderShapeDesc)planeShape.Description;
                            Matrix transform = planeShape.PositiveCenterMatrix * entityWorldMatrix;

                            Plane plane = new Plane(planeDesc.Normal, planeDesc.Offset);

                            // Pre-Transform plane parameters
                            plane.Normal = Vector3.TransformNormal(planeDesc.Normal, transform);
                            float offset = Vector3.Dot(transform.TranslationVector, planeDesc.Normal);
                            plane.D += offset;

                            colliderData.Planes.Add(plane);
                        }
                        else if (shapeType == typeof(ConvexHullColliderShape))
                        {
                            var hull = (ConvexHullColliderShape)shape;
                            Matrix transform = hull.PositiveCenterMatrix * entityWorldMatrix;

                            // Convert hull indices to int
                            int[] indices = new int[hull.Indices.Count];
                            if (hull.Indices.Count % 3 != 0) throw new InvalidOperationException("Physics hull does not consist of triangles");
                            for (int i = 0; i < hull.Indices.Count; i += 3)
                            {
                                indices[i] = (int)hull.Indices[i];
                                indices[i + 2] = (int)hull.Indices[i + 1]; // NOTE: Reversed winding to create left handed input
                                indices[i + 1] = (int)hull.Indices[i + 2];
                            }

                            entityNavigationMeshInputBuilder.AppendArrays(hull.Points.ToArray(), indices, transform);
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
                }

                // Clear cache on addition of infinite planes
                if (colliderData.Planes.Count > 0)
                    clearCache = true;

                // Mark collider as processed
                colliderData.Processed = true;
            });

            if (clearCache && lastNavigationMesh != null)
            {
                lastNavigationMesh.Cache = null;
            }
        }

        /// <summary>
        /// Marks tiles that should be built according to how much their geometry affects the navigation mesh and the bounding boxes specified for building
        /// </summary>
        private void MarkTiles(NavigationMeshInputBuilder inputBuilder, ref NavigationMeshBuildSettings buildSettings, ref NavigationAgentSettings agentSettings, HashSet<Point> tilesToBuild)
        {
            // Extend bounding box for agent size
            BoundingBox boundingBoxToCheck = inputBuilder.BoundingBox;
            NavigationMeshBuildUtils.ExtendBoundingBox(ref boundingBoxToCheck, new Vector3(agentSettings.Radius));

            // TODO incremental
            List<Point> newTileList = NavigationMeshBuildUtils.GetOverlappingTiles(buildSettings, boundingBoxToCheck);
            foreach (Point p in newTileList)
            {
                tilesToBuild.Add(p);
            }
        }

        private NavigationMeshInputBuilder BuildPlaneGeometry(Plane plane, BoundingBox boundingBox)
        {
            Vector3 maxSize = boundingBox.Maximum - boundingBox.Minimum;
            float maxDiagonal = Math.Max(maxSize.X, Math.Max(maxSize.Y, maxSize.Z));

            // Generate source plane triangles
            Vector3[] planePoints;
            int[] planeInds;
            NavigationMeshBuildUtils.BuildPlanePoints(ref plane, maxDiagonal, out planePoints, out planeInds);

            Vector3 tangent, bitangent;
            NavigationMeshBuildUtils.GenerateTangentBinormal(plane.Normal, out tangent, out bitangent);
            // Calculate plane offset so that the plane always covers the whole range of the bounding box
            Vector3 planeOffset = Vector3.Dot(boundingBox.Center, tangent) * tangent;
            planeOffset += Vector3.Dot(boundingBox.Center, bitangent) * bitangent;

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[planePoints.Length];
            for (int i = 0; i < planePoints.Length; i++)
            {
                vertices[i] = new VertexPositionNormalTexture(planePoints[i] + planeOffset, Vector3.UnitY, Vector2.Zero);
            }

            GeometricMeshData<VertexPositionNormalTexture> meshData = new GeometricMeshData<VertexPositionNormalTexture>(vertices, planeInds, true);

            NavigationMeshInputBuilder inputBuilder = new NavigationMeshInputBuilder();
            inputBuilder.AppendMeshData(meshData, Matrix.Identity);
            return inputBuilder;
        }
    }
}