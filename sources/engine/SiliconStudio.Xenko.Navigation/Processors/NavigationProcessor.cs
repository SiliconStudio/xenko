// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Navigation.Processors
{
    /// <summary>
    /// Manages the loading of the native side navigation meshes. Will only load one version of the navigation mesh if it is referenced by multiple components
    /// </summary>
    public class NavigationProcessor : EntityProcessor<NavigationComponent, NavigationProcessor.AssociatedData>
    {
        /// <summary>
        /// Maps navigation meshed to their natively loaded counterparts
        /// </summary>
        private readonly Dictionary<NavigationMesh, NavigationMeshInternal> loadedNavigationMeshes = new Dictionary<NavigationMesh, NavigationMeshInternal>();

        public override void Update(GameTime time)
        {
            foreach (var p in ComponentDatas)
            {
                // Should update selected navigation mesh?
                if (p.Key.NavigationMesh != p.Value.LoadedNavigationMesh)
                {
                    UpdateNavigationMesh(p.Key, p.Value);
                }

                // Should update selected layer?
                if (p.Key.NavigationMeshLayer != p.Value.SelectedLayer)
                {
                    SelectLayer(p.Key, p.Value);
                }
            }
        }

        protected override void OnSystemRemove()
        {
            // Dispose of all loaded navigation meshes
            foreach (var pair in loadedNavigationMeshes)
            {
                pair.Value.Dispose();
            }
            loadedNavigationMeshes.Clear();
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

        private void RemoveReference(NavigationComponent component, AssociatedData data)
        {
            // Check if loaded navigation mesh is no longer needed
            if (data.NavigationMeshInternal != null)
            {
                if (data.NavigationMeshInternal.RemoveReference(component))
                {
                    loadedNavigationMeshes.Remove(data.LoadedNavigationMesh);
                    data.NavigationMeshInternal.Dispose();
                }
            }

            data.NavigationMeshInternal = null;
            data.LoadedNavigationMesh = null;
            component.NavigationMeshInternal = IntPtr.Zero;
        }

        private void UpdateNavigationMesh(NavigationComponent component, AssociatedData data)
        {
            // Remove old reference
            RemoveReference(component, data);

            if (component.NavigationMesh != null)
            {
                NavigationMeshInternal navigationMeshInternal;
                if (!loadedNavigationMeshes.TryGetValue(component.NavigationMesh, out navigationMeshInternal))
                {
                    navigationMeshInternal = new NavigationMeshInternal(component.NavigationMesh);
                    loadedNavigationMeshes.Add(component.NavigationMesh, navigationMeshInternal);
                }
                data.NavigationMeshInternal = navigationMeshInternal;
                navigationMeshInternal.AddReference(component);

                SelectLayer(component, data);

                // Mark new navigation mesh as loaded
                data.LoadedNavigationMesh = component.NavigationMesh;
            }
        }

        private void SelectLayer(NavigationComponent component, AssociatedData data)
        {
            if (data.NavigationMeshInternal?.Layers == null)
                return;

            // Store a pointer to the native navmesh object in the navmesh component
            component.NavigationMeshInternal = component.NavigationMeshLayer < data.NavigationMeshInternal.Layers.Length
                ? data.NavigationMeshInternal.Layers[component.NavigationMeshLayer]
                : IntPtr.Zero;
            data.SelectedLayer = component.NavigationMeshLayer;
        }
        
        /// <summary>
        /// Associated data for navigation mesh components
        /// </summary>
        public class AssociatedData
        {
            public NavigationMesh LoadedNavigationMesh;
            public NavigationComponent Component;
            internal NavigationMeshInternal NavigationMeshInternal;
            internal int SelectedLayer;
        }

        internal class NavigationMeshInternal : IDisposable
        {
            private readonly float cellTileSize;
            private readonly HashSet<object> references = new HashSet<object>();

            public IntPtr[] Layers;

            public NavigationMeshInternal(NavigationMesh navigationMesh)
            {
                cellTileSize = navigationMesh.BuildSettings.TileSize * navigationMesh.BuildSettings.CellSize;
                Layers = new IntPtr[navigationMesh.NumLayers];
                for (int i = 0; i < navigationMesh.NumLayers; i++)
                {
                    Layers[i] = LoadLayer(navigationMesh.Layers[i]);
                }
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

            private unsafe IntPtr LoadLayer(NavigationMeshLayer navigationMeshLayer)
            {
                IntPtr layer = Navigation.CreateNavmesh(cellTileSize);
                if (layer == IntPtr.Zero)
                    return layer;

                // Add all the tiles to the navigation mesh
                foreach (var tile in navigationMeshLayer.Tiles)
                {
                    if (tile.Value.Data == null)
                        continue; // Just skip empty tiles
                    fixed (byte* inputData = tile.Value.Data)
                    {
                        Navigation.AddTile(layer, tile.Key, new IntPtr(inputData), tile.Value.Data.Length);
                    }
                }

                return layer;
            }
        }
    }
}