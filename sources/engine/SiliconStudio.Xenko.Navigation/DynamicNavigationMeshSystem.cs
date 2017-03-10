using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Navigation.Processors;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// System that handles building of navigation meshes at runtime
    /// </summary>
    public class DynamicNavigationMeshSystem : GameSystem
    {
        // TODO turn off eventually
        public bool AutoRebuild = true;

        /// <summary>
        /// Collision filter that indicates which colliders are used in navmesh generation
        /// </summary>
        public CollisionFilterGroupFlags IncludedCollisionGroups { get; set; }

        /// <summary>
        /// Build settings used by Recast
        /// </summary>
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        /// <summary>
        /// Settings for agents used with the dynamic navigation mesh
        /// Every entry corresponds with a layer, which is used by <see cref="NavigationComponent.NavigationMeshLayer"/> to select one from this list
        /// </summary>
        public List<NavigationAgentSettings> AgentSettings { get; private set; } = new List<NavigationAgentSettings>();

        private bool pendingRebuild;

        private SceneInstance currentSceneInstance;

        private NavigationMeshBuilder builder = new NavigationMeshBuilder();

        private CancellationTokenSource buildTaskCancellationTokenSource;

        private StaticColliderProcessor processor;

        public DynamicNavigationMeshSystem(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            EnabledChanged += OnEnabledChanged;
        }

        /// <summary>
        /// Raised when the navigation mesh for the current scene is updated
        /// </summary>
        public event EventHandler NavigationUpdated;

        public NavigationMesh CurrentNavigationMesh { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            Game.GameSystems.CollectionChanged += GameSystemsOnCollectionChanged;

            if (Game.Settings != null)
            {
                InitializeSettingsFromGameSettings(Game.Settings);
            }
            else
            {
                // Initial build settings
                BuildSettings = ObjectFactoryRegistry.NewInstance<NavigationMeshBuildSettings>();
                IncludedCollisionGroups = CollisionFilterGroupFlags.AllFilter;
                AgentSettings = new List<NavigationAgentSettings>
                {
                    ObjectFactoryRegistry.NewInstance<NavigationAgentSettings>()
                };
            }
        }

        /// <summary>
        /// Copies the default settings from the <see cref="GameSettings"/> for building navigation
        /// </summary>
        public void InitializeSettingsFromGameSettings(GameSettings gameSettings)
        {
            if (gameSettings == null)
                throw new ArgumentNullException(nameof(gameSettings));

            // Initialize build settings from game settings
            var navigationSettings = gameSettings.Configurations.Get<NavigationSettings>();
            BuildSettings = navigationSettings.BuildSettings;
            IncludedCollisionGroups = navigationSettings.IncludedCollisionGroups;
            AgentSettings = navigationSettings.NavigationMeshAgentSettings;
            Enabled = navigationSettings.EnableDynamicNavigationMesh;

            // Queue rebuild
            pendingRebuild = true;
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
                Game.Script.AddTask(async () =>
                {
                    // TODO EntityProcessors
                    // Currently have to wait a frame for transformations to update
                    // for example when calling Rebuild from the event that a component was added to the scene, this component will not be in the correct location yet
                    // since the TransformProcessor runs the next frame
                    await Game.Script.NextFrame();
                    await Rebuild();
                });
                pendingRebuild = false;
            }
        }

        /// <summary>
        /// Starts an asynchronous rebuild of the navigation mesh
        /// </summary>
        public async Task<NavigationMeshBuildResult> Rebuild()
        {
            if (currentSceneInstance == null)
                return new NavigationMeshBuildResult();

            // Cancel running build, TODO check if the running build can actual satisfy the current rebuild request and don't cancel in that case
            buildTaskCancellationTokenSource?.Cancel();
            buildTaskCancellationTokenSource = new CancellationTokenSource();

            // Collect bounding boxes
            var boundingBoxProcessor = currentSceneInstance.GetProcessor<BoundingBoxProcessor>();
            if (boundingBoxProcessor == null)
                return new NavigationMeshBuildResult();

            List<BoundingBox> boundingBoxes = new List<BoundingBox>();
            foreach (var boundingBox in boundingBoxProcessor.BoundingBoxes)
            {
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                boundingBox.Entity.Transform.WorldMatrix.Decompose(out scale, out rotation, out translation);
                boundingBoxes.Add(new BoundingBox(translation - scale, translation + scale));
            }

            var result = Task.Run(() =>
            {
                // Only have one active build at a time
                lock (builder)
                {
                    return builder.Build(BuildSettings, AgentSettings, IncludedCollisionGroups,  boundingBoxes, buildTaskCancellationTokenSource.Token);
                }
            });
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

                processor = new StaticColliderProcessor();
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