using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Games;
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
            private float cellTileSize;

            public NativeNavmesh(NavigationMesh navigationMesh)
            {
                cellTileSize = navigationMesh.buildSettings.TileSize*navigationMesh.buildSettings.CellSize;
                Layers = new IntPtr[navigationMesh.Layers.Length];
                for (int i = 0; i < navigationMesh.Layers.Length; i++)
                {
                    Layers[i] = LoadLayer(navigationMesh.Layers[i]);
                }
            }

            private unsafe IntPtr LoadLayer(NavigationMesh.Layer navigationMeshLayer)
            {
                IntPtr layer = Navigation.CreateNavmesh(cellTileSize);
                if (layer == IntPtr.Zero)
                    return layer;

                // Add all the tiles to the navigation mesh
                foreach (var tile in navigationMeshLayer.Tiles)
                {
                    if (tile.Value.NavmeshData == null)
                        continue; // Just skip empty tiles
                    fixed (byte* inputData = tile.Value.NavmeshData)
                    {
                        Navigation.AddTile(layer, tile.Key, new IntPtr(inputData), tile.Value.NavmeshData.Length);
                    }
                }

                return layer;
            }

            public void Dispose()
            {
                if (Layers == null)
                    return;
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
        
        protected internal override void OnSystemAdd()
        {
        }
        protected internal override void OnSystemRemove()
        {
            // Dispose of all loaded navigation meshes
            foreach (var pair in loadedNavigationMeshes)
            {
                pair.Value.Dispose();
            }
            loadedNavigationMeshes.Clear();
        }

        public override void Update(GameTime time)
        {
            foreach (var p in ComponentDatas)
            {
                if (p.Key.NavigationMesh != p.Value.LoadedNavigationMesh)
                {
                    UpdateNavigationMesh(p.Key, p.Value);
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
            UpdateNavigationMesh(component, data);
        }
        protected override void OnEntityComponentRemoved(Entity entity, NavigationComponent component, AssociatedData data)
        {
            RemoveReference(component, data);
        }

        void RemoveReference(NavigationComponent component, AssociatedData data)
        {
            // Check if loaded navigation mesh is no longer needed
            if (data.NativeNavmesh != null)
            {
                if (data.NativeNavmesh.RemoveReference(component))
                {
                    // Remove debug entity
                    //Entity meshVizualizerEntity;
                    //if (visualizedNavigationMeshes.TryGetValue(component.NavigationMesh, out meshVizualizerEntity))
                    //{
                    //    debugEntity.Transform.Children.Remove(meshVizualizerEntity.Transform);
                    //    visualizedNavigationMeshes.Remove(component.NavigationMesh);
                    //}
                    loadedNavigationMeshes.Remove(data.LoadedNavigationMesh);
                    data.NativeNavmesh.Dispose();
                }
            }

            component.nativeNavmesh = IntPtr.Zero;
        }
        void UpdateNavigationMesh(NavigationComponent component, AssociatedData data)
        {
            // Remove old reference
            RemoveReference(component, data);
            
            if (component.NavigationMesh != null)
            {
                NativeNavmesh nativeNavmesh;
                if (!loadedNavigationMeshes.TryGetValue(component.NavigationMesh, out nativeNavmesh))
                {
                    nativeNavmesh = new NativeNavmesh(component.NavigationMesh);
                    loadedNavigationMeshes.Add(component.NavigationMesh, nativeNavmesh);
                }
                data.NativeNavmesh = nativeNavmesh;
                nativeNavmesh.AddReference(component);

                // Store a pointer to the native navmesh object in the navmesh component
                component.nativeNavmesh = component.NavigationMeshLayer < nativeNavmesh.Layers.Length ?
                    nativeNavmesh.Layers[component.NavigationMeshLayer] : IntPtr.Zero;

                //if (!visualizedNavigationMeshes.ContainsKey(component.NavigationMesh))
                //{
                //    // Add debug entity
                //    Entity meshVizualizerEntity = new Entity();
                //    meshVizualizerEntity.Add(component.NavigationMesh.CreateDebugModelComponent(this.sceneSystem.Game.GraphicsDevice));
                //    debugEntity.Transform.Children.Add(meshVizualizerEntity.Transform);
                //    visualizedNavigationMeshes.Add(component.NavigationMesh, meshVizualizerEntity);
                //}
            }

            // Mark new navigation mesh as loaded
            data.LoadedNavigationMesh = component.NavigationMesh;
        }
    }
}
