using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Native;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Engine.Processors
{
    public class NavigationProcessor : EntityProcessor<NavigationComponent, NavigationProcessor.AssociatedData>
    {
        public class NativeNavmesh : IDisposable
        {
            public IntPtr Pointer;
            private HashSet<object> references = new HashSet<object>();

            public NativeNavmesh(NavigationMesh navigationMesh)
            {
                if (navigationMesh.NavmeshData == null)
                    throw new ArgumentNullException(nameof(navigationMesh));
                GCHandle pinnedArray = GCHandle.Alloc(navigationMesh.NavmeshData, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                Pointer = Navigation.LoadNavmesh(pointer, navigationMesh.NavmeshData.Length);
                if (Pointer == null)
                    throw new Exception("Failed to load navigation mesh");
                pinnedArray.Free();
            }

            public void Dispose()
            {
                Navigation.DestroyNavmesh(Pointer);
                Pointer = IntPtr.Zero;
            }

            /// <summary>
            /// Adds a reference to this object
            /// </summary>
            /// <param name="reference"></param>
            public void AddReference(object reference)
            {
                references.Add(reference);
            }
            /// <summary>
            ///  Removes a reference to this object
            /// </summary>
            /// <param name="reference"></param>
            /// <returns>true if the object is no longer referenced</returns>
            public bool RemoveReference(object reference)
            {
                references.Remove(reference);
                return references.Count == 0;
            }

        }

        public class AssociatedData
        {
            // The original navigation mesh object
            public NavigationComponent Component;
            public NativeNavmesh NativeNavmesh;
            public NavigationMesh LoadedNavigationMesh;
        }

        /// <summary>
        /// Maps navigation meshed to their natively loaded counterparts
        /// </summary>
        private Dictionary<NavigationMesh, NativeNavmesh> loadedNavigationMeshes = new Dictionary<NavigationMesh, NativeNavmesh>();

        private Dictionary<NavigationMesh, Entity> debugEntities = new Dictionary<NavigationMesh, Entity>();

        private bool debugOverlayEnabled = false;
        private SceneSystem sceneSystem;
        private Entity debugEntityScene;
        private ISceneRenderer debugSceneRenderer;
        private List<NavigationComponent> navigationComponents = new List<NavigationComponent>();
        private Scene debugScene;

        public bool RenderNavigationOverlays
        {
            get { return debugOverlayEnabled; }
            set { SetNavigationOverlaysEnabled(value); }
        }

        protected internal override void OnSystemAdd()
        {
            sceneSystem = Services.GetSafeServiceAs<SceneSystem>();
        }

        internal void SetNavigationOverlaysEnabled(bool enabled)
        {
            debugOverlayEnabled = enabled;

            if (!debugOverlayEnabled)
            {
                var mainCompositor = (SceneGraphicsCompositorLayers)sceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
                var scene = debugEntityScene.Get<ChildSceneComponent>().Scene;

                foreach (var element in debugEntities)
                {
                    debugScene.Entities.Remove(element.Value);
                }

                sceneSystem.SceneInstance.Scene.Entities.Remove(debugEntityScene);
                mainCompositor.Master.Renderers.Remove(debugSceneRenderer);
            }
            else
            {
                //we create a child scene to render the shapes, so that they are totally separated from the normal scene
                var mainCompositor = (SceneGraphicsCompositorLayers)sceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;

                var graphicsCompositor = new SceneGraphicsCompositorLayers
                {
                    Cameras = { mainCompositor.Cameras[0] },
                    Master =
                    {
                        Renderers =
                        {
                            // TODO: Add custom render mode
                            new SceneCameraRenderer
                            {
                                Mode = new NavigationDebugCameraRenderMode()
                            }
                        }
                    }
                };

                debugScene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };

                var childComponent = new ChildSceneComponent { Scene = debugScene };
                debugEntityScene = new Entity { childComponent };
                debugSceneRenderer = new SceneChildRenderer(childComponent);

                mainCompositor.Master.Add(debugSceneRenderer);
                sceneSystem.SceneInstance.Scene.Entities.Add(debugEntityScene);

                foreach (var element in loadedNavigationMeshes)
                {
                    AddDebugEntity(element.Key);
                }
            }
        }

        void AddDebugEntity(NavigationMesh mesh)
        {
            if(debugOverlayEnabled)
            {
                ModelComponent debugComponent = mesh.CreateDebugModelComponent(sceneSystem.Game.GraphicsDevice);
                if (debugComponent != null)
                {
                    Entity debugEntity = new Entity("Debug Entity");
                    debugEntity.Add(debugComponent);
                    debugScene.Entities.Add(debugEntity);
                    debugEntities.Add(mesh, debugEntity);
                }
            }
        }

        void RemoveDebugEntity(NavigationMesh mesh)
        {
            if(debugOverlayEnabled)
            {
                Entity entity;
                if(debugEntities.TryGetValue(mesh, out entity))
                {
                    debugScene.Entities.Remove(entity);
                }
            }
        }

        protected override AssociatedData GenerateComponentData(Entity entity, NavigationComponent component)
        {
            AssociatedData data = new AssociatedData();
            data.Component = component;
            return data;
        }

        protected override void OnEntityComponentAdding(Entity entity, NavigationComponent component, AssociatedData data)
        {
            if (component.NavigationMesh != null)
            {
                NativeNavmesh nativeNavmesh;
                if(!loadedNavigationMeshes.TryGetValue(component.NavigationMesh, out nativeNavmesh) && component.NavigationMesh != null)
                {
                    nativeNavmesh = new NativeNavmesh(component.NavigationMesh);
                    loadedNavigationMeshes.Add(component.NavigationMesh, nativeNavmesh);
                }
                data.LoadedNavigationMesh = component.NavigationMesh;
                data.NativeNavmesh = nativeNavmesh;
                nativeNavmesh?.AddReference(component);

                // Store a pointer to the native navmesh object in the navmesh component
                component.nativeNavmesh = nativeNavmesh.Pointer;

                AddDebugEntity(component.NavigationMesh);
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, NavigationComponent component, AssociatedData data)
        {
            RemoveDebugEntity(component?.NavigationMesh);

            // Check if loaded navigation mesh is no longer needed
            if (data.NativeNavmesh != null)
            {
                if(data.NativeNavmesh.RemoveReference(component))
                {
                    loadedNavigationMeshes.Remove(component.NavigationMesh);
                    data.NativeNavmesh.Dispose();
                }
            }

            component.nativeNavmesh = IntPtr.Zero;
        }
        
        protected internal override void OnSystemRemove()
        {
            // Dispose of all loaded navigation meshes
            foreach(var pair in loadedNavigationMeshes)
            {
                pair.Value.Dispose();
            }
            loadedNavigationMeshes.Clear();
        }
    }
}
