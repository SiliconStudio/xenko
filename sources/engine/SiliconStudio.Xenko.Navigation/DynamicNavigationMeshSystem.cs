using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    public class NavigationMeshBuildResult
    {
        public bool Success = false;
        public NavigationMesh NavigationMesh;
    }

    public class NavigationMeshBuilder
    {
        // TODO
        // TODO multiple bounding boxes + thread local
        BoundingBox globalBoundingBox = new BoundingBox(new Vector3(-20), new Vector3(20));
        
        // TODO: Space partitioning
        private List<StaticColliderData> colliders = new List<StaticColliderData>();

        public void Add(StaticColliderData colliderData)
        {
            lock (colliders)
            {
                colliders.Add(colliderData);
            }
        }

        public void Add(IEnumerable<StaticColliderData> colliderData)
        {
            lock (colliders)
            {
                colliders.AddRange(colliderData);
            }
        }

        public void Remove(StaticColliderData colliderData)
        {
            lock (colliders)
            {
                colliders.Remove(colliderData);
            }
        }

        public NavigationMeshBuildResult BuildAsync(CancellationToken cancellationToken)
        {
            var result = new NavigationMeshBuildResult();

            // TODO Expose build settings
            var buildSettings = new NavigationMeshBuildSettings
            {
                CellHeight = 0.3f,
                CellSize = 0.16f,
                TileSize = 16,
                MinRegionArea = 2,
                RegionMergeArea = 20,
                MaxEdgeLen = 12.0f,
                MaxEdgeError = 1.3f,
                DetailSamplingDistance = 6.0f,
                MaxDetailSamplingError = 1.0f,
            };

            var agentSettings = new NavigationAgentSettings
            {
                Height = 1.0f,
                Radius = 0.5f,
                MaxSlope = new AngleSingle(45.0f, AngleType.Degree),
                MaxClimb = 1.0f,
            };
            
            // Copy colliders for thread access
            StaticColliderData[] collidersLocal;
            lock (colliders)
            {
                collidersLocal = colliders.ToArray();
            }

            BuildInput(collidersLocal);

            // Combine input and collect tiles to build
            HashSet<Point> tilesToBuild = new HashSet<Point>();
            NavigationMeshInputBuilder sceneNavigationMeshInputBuilder = new NavigationMeshInputBuilder();
            foreach (var shape in collidersLocal)
            {
                if (shape.Builder == null)
                    continue;

                // TODO incremental
                List<Point> newTileList = NavigationMeshBuildUtils.GetOverlappingTiles(buildSettings, shape.Builder.BoundingBox);
                foreach (Point p in newTileList)
                    tilesToBuild.Add(p);

                sceneNavigationMeshInputBuilder.AppendOther(shape.Builder);
            }

            // TODO: Generate tile local mesh input data
            var inputVertices = sceneNavigationMeshInputBuilder.Points.ToArray();
            var inputIndices = sceneNavigationMeshInputBuilder.Indices.ToArray();

            long buildTimeStamp = DateTime.UtcNow.Ticks;

            // TODO can't use tilesToBuild directly
            ConcurrentCollector<Tuple<Point, NavigationMeshTile>> buildTiles = new ConcurrentCollector<Tuple<Point, NavigationMeshTile>>(tilesToBuild.Count);
            Dispatcher.ForEach(tilesToBuild.ToArray(), tileCoordinate =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                unsafe
                {
                    IntPtr builder = Navigation.CreateBuilder();

                    // Tile bounding box
                    BoundingBox tileBoundingBox = NavigationMeshBuildUtils.ClampBoundingBoxToTile(buildSettings, globalBoundingBox, tileCoordinate);

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
                    if (generatedDataPtr->Success)
                    {
                        // Header of the generated data
                        Navigation.TileHeader* header = (Navigation.TileHeader*)generatedDataPtr->NavmeshData;

                        NavigationMeshTile meshTile = new NavigationMeshTile();

                        // Copy the generated navigationMesh data
                        meshTile.Data = new byte[generatedDataPtr->NavmeshDataLength+sizeof(long)];
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

                        buildTiles.Add(new Tuple<Point, NavigationMeshTile>(tileCoordinate, meshTile));
                    }

                    Navigation.DestroyBuilder(builder);
                }
            });
            if (cancellationToken.IsCancellationRequested)
                return result;
            
            var layer = new NavigationMeshLayer();
            result.NavigationMesh = new NavigationMesh();
            result.NavigationMesh.LayersInternal.Add(layer);
            result.NavigationMesh.BoundingBox = globalBoundingBox; // TODO
            layer.BuildSettings = buildSettings;
            // TODO multiple agent settings
            layer.AgentSettings = agentSettings;
            foreach (var p in buildTiles)
            {
                layer.TilesInternal.Add(p.Item1, p.Item2);
            }

            result.Success = true;
            return result;
        }

        /// <summary>
        /// Rebuilds outdated triangle data for colliders
        /// </summary>
        /// <param name="local"></param>
        private void BuildInput(StaticColliderData[] collidersLocal)
        {
            Vector3 maxSize = globalBoundingBox.Maximum - globalBoundingBox.Minimum;
            float maxDiagonal = Math.Max(maxSize.X, Math.Max(maxSize.Y, maxSize.Z));

            // TODO for some reason this call is ambiguous when called directly with a type of List<StaticColliderData>
            Dispatcher.ForEach(collidersLocal, colliderData =>
            {
                var entity = colliderData.Component.Entity;
                TransformComponent entityTransform = entity.Transform;
                Matrix entityWorldMatrix = entityTransform.WorldMatrix;

                NavigationMeshInputBuilder entityNavigationMeshInputBuilder = colliderData.Builder = new NavigationMeshInputBuilder();

                // Interate through all the colliders shapes while queueing all shapes in compound shapes to process those as well
                Queue<ColliderShape> shapesToProcess = new Queue<ColliderShape>();
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
                        var planeColliderShape = (StaticPlaneColliderShape)shape;
                        var planeDesc = (StaticPlaneColliderShapeDesc)planeColliderShape.Description;
                        Matrix transform = planeColliderShape.PositiveCenterMatrix * entityWorldMatrix;

                        Plane plane = new Plane(planeDesc.Normal, planeDesc.Offset);

                        // Pre-Transform plane parameters
                        plane.Normal = Vector3.TransformNormal(planeDesc.Normal, transform);
                        float offset = Vector3.Dot(transform.TranslationVector, planeDesc.Normal);
                        plane.D += offset;

                        // Generate source plane triangles
                        Vector3[] planePoints;
                        int[] planeInds;
                        NavigationMeshBuildUtils.BuildPlanePoints(ref plane, maxDiagonal, out planePoints, out planeInds);

                        Vector3 tangent, bitangent;
                        NavigationMeshBuildUtils.GenerateTangentBinormal(plane.Normal, out tangent, out bitangent);
                        // Calculate plane offset so that the plane always covers the whole range of the bounding box
                        Vector3 planeOffset = Vector3.Dot(globalBoundingBox.Center, tangent) * tangent;
                        planeOffset += Vector3.Dot(globalBoundingBox.Center, bitangent) * bitangent;

                        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[planePoints.Length];
                        for (int i = 0; i < planePoints.Length; i++)
                        {
                            vertices[i] = new VertexPositionNormalTexture(planePoints[i] + planeOffset, Vector3.UnitY, Vector2.Zero);
                        }

                        GeometricMeshData<VertexPositionNormalTexture> meshData = new GeometricMeshData<VertexPositionNormalTexture>(vertices, planeInds, true);
                        entityNavigationMeshInputBuilder.AppendMeshData(meshData, Matrix.Identity);
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
            });
        }
    }

    public class StaticColliderData
    {
        public StaticColliderComponent Component;
        public int LastStateHash = 0;
        internal NavigationMeshInputBuilder Builder;
    }

    public class StaticColliderCollectorProcessor : EntityProcessor<StaticColliderComponent, StaticColliderData>
    {
        public delegate void CollectionChangedEventHandler(StaticColliderComponent component, StaticColliderData data);

        public event CollectionChangedEventHandler ColliderAdded;
        public event CollectionChangedEventHandler ColliderRemoved;

        protected override StaticColliderData GenerateComponentData(Entity entity, StaticColliderComponent component)
        {
            return new StaticColliderData { Component = component };
        }

        protected override void OnEntityComponentAdding(Entity entity, StaticColliderComponent component, StaticColliderData data)
        {
            base.OnEntityComponentAdding(entity, component, data);
            ColliderAdded?.Invoke(component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, StaticColliderComponent component, StaticColliderData data)
        {
            ColliderRemoved?.Invoke(component, data);
            base.OnEntityComponentRemoved(entity, component, data);
        }
    }

    /// <summary>
    /// System that handles building of navigation meshes at runtime
    /// </summary>
    public class DynamicNavigationMeshSystem : GameSystem
    {
        public bool AutoRebuild = true; // TODO turn off eventually

        private bool pendingRebuild = false;

        private SceneInstance currentSceneInstance = null;

        private NavigationMeshBuilder builder = new NavigationMeshBuilder();

        private Task<NavigationMeshBuildResult> currentBuildTask;
        private CancellationTokenSource buildTaskCancellationTokenSource = null;
        private StaticColliderCollectorProcessor processor;

        public DynamicNavigationMeshSystem(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            EnabledChanged += OnEnabledChanged;
        }

        /// <summary>
        /// Raised when the navigation mesh for the current scene is updated
        /// </summary>
        public event EventHandler NavigationUpdated;

        public NavigationMesh CurrentNavigationMesh { get; private set; } = null;

        public override void Initialize()
        {
            base.Initialize();
            Game.GameSystems.CollectionChanged += GameSystemsOnCollectionChanged;
        }

        public override void Update(GameTime gameTime)
        {
            // This system should before becomming functional
            if (!Enabled)
                return;

            if (currentSceneInstance != Game.SceneSystem?.SceneInstance)
            {
                // ReSharper disable once PossibleNullReferenceException
                UpdateScene(Game.SceneSystem.SceneInstance);
            }

            if (pendingRebuild)
            {
                Rebuild();
                pendingRebuild = false;
            }
        }

        /// <summary>
        /// Starts an asynchronous rebuild of the navigation mesh
        /// </summary>
        public async Task<NavigationMeshBuildResult> Rebuild()
        {
            // Cancel running build, TODO check if the running build can actual satisfy the current rebuild request and don't cancel in that case
            buildTaskCancellationTokenSource?.Cancel();
            buildTaskCancellationTokenSource = new CancellationTokenSource();

            var result = Task.Run(() => builder.BuildAsync(buildTaskCancellationTokenSource.Token));
            await result;

            FinilizeRebuild(result);

            return result.Result;
        }

        private void FinilizeRebuild(Task<NavigationMeshBuildResult> resultTask)
        {
            var result = resultTask.Result;
            if (result.Success)
            {
                CurrentNavigationMesh = result.NavigationMesh;
                NavigationUpdated?.Invoke(this, null);
            }
            else
            {
                // TODO Error log
            }
        }

        private void GameSystemsOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            // Track addition of game systems
            var system = (GameSystemBase)trackingCollectionChangedEventArgs.Item;
            switch (trackingCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void UpdateScene(SceneInstance newSceneInstance)
        {
            // TODO: Unload old navigation, load new navigation
            if (currentSceneInstance != null)
            {
                if (processor != null)
                {
                    currentSceneInstance.Processors.Remove(processor);
                    processor.ColliderAdded -= ProcessorOnColliderAdded;
                    processor.ColliderRemoved -= ProcessorOnColliderRemoved;
                }
                currentSceneInstance.ComponentChanged -= CurrentSceneInstanceOnComponentChanged;
            }

            // Set the currect scene
            currentSceneInstance = newSceneInstance;

            if (currentSceneInstance != null)
            {
                // Scan for components
                // TODO this might not be needed
                //ScanInitialSceneRecursive(currentSceneInstance.RootScene);

                processor = new StaticColliderCollectorProcessor();
                processor.ColliderAdded += ProcessorOnColliderAdded;
                processor.ColliderRemoved += ProcessorOnColliderRemoved;
                currentSceneInstance.Processors.Add(processor);
                currentSceneInstance.ComponentChanged += CurrentSceneInstanceOnComponentChanged;
            }
        }

        private void ProcessorOnColliderAdded(StaticColliderComponent component, StaticColliderData data)
        {
            // TODO: prevent locking of batched collider addition
            builder.Add(data);
            pendingRebuild = true;
        }

        private void ProcessorOnColliderRemoved(StaticColliderComponent component, StaticColliderData data)
        {
            builder.Remove(data);
            pendingRebuild = true;
        }

//        private void ScanInitialSceneRecursive(Scene scene)
//        {
//            foreach (var entity in scene.Entities)
//            {
//                if (entity.Get<StaticColliderComponent>())
//                {
//                    processor.
//                }
//            }
//
//            foreach (var childScene in scene.Children)
//            {
//                ScanInitialSceneRecursive(childScene);
//            }
//        }

        private void CurrentSceneInstanceOnComponentChanged(object sender, EntityComponentEventArgs entityComponentEventArgs)
        {
        }

        private void Cleanup()
        {
            currentSceneInstance = null;

            // TODO
            NavigationUpdated?.Invoke(this, null);
        }

        private void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            if (!Enabled)
            {
                Cleanup();
            }
        }
    }
}