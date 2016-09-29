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
            public IntPtr[] Layers;
            private HashSet<object> references = new HashSet<object>();

            public NativeNavmesh(NavigationMesh navigationMesh)
            {
                Layers = new IntPtr[navigationMesh.Layers.Length];
                for (int i = 0; i < navigationMesh.Layers.Length; i++)
                {
                    Layers[i] = navigationMesh.Layers[i].NavmeshData != null ? LoadLayer(navigationMesh.Layers[i]) : IntPtr.Zero;
                }
            }

            private unsafe IntPtr LoadLayer(NavigationMesh.Layer navigationMeshLayer)
            {
                if (navigationMeshLayer.NavmeshData == null)
                    throw new ArgumentNullException(nameof(navigationMeshLayer));
                fixed (void* data = navigationMeshLayer.NavmeshData)
                {
                    return Navigation.LoadNavmesh(new IntPtr(data), navigationMeshLayer.NavmeshData.Length);
                }
            }

            public void Dispose()
            {
                for (int i = 0; i < Layers.Length; i++)
                {
                    if (Layers[i] != IntPtr.Zero)
                        Navigation.DestroyNavmesh(Layers[i]);
                }
                Layers = null;
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

        protected internal override void OnSystemAdd()
        {
            sceneSystem = Services.GetSafeServiceAs<SceneSystem>();
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
                if(!loadedNavigationMeshes.TryGetValue(component.NavigationMesh, out nativeNavmesh))
                {
                    nativeNavmesh = new NativeNavmesh(component.NavigationMesh);
                    loadedNavigationMeshes.Add(component.NavigationMesh, nativeNavmesh);
                }
                data.LoadedNavigationMesh = component.NavigationMesh;
                data.NativeNavmesh = nativeNavmesh;
                nativeNavmesh?.AddReference(component);

                // Store a pointer to the native navmesh object in the navmesh component
                component.nativeNavmesh = component.NavigationMeshLayer < nativeNavmesh.Layers.Length ? 
                    nativeNavmesh.Layers[component.NavigationMeshLayer] : IntPtr.Zero;
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, NavigationComponent component, AssociatedData data)
        {
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
