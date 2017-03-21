// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Navigation.Processors
{
    /// <summary>
    /// Manages the loading of the native side navigation meshes. Will only load one version of the navigation mesh if it is referenced by multiple components
    /// </summary>
    public class NavigationProcessor : EntityProcessor<NavigationComponent, NavigationProcessor.AssociatedData>
    {
        private readonly HashSet<NavigationComponent> dynamicNavigationComponents = new HashSet<NavigationComponent>();

        private DynamicNavigationMeshSystem dynamicNavigationMeshSystem;

        public override void Update(GameTime time)
        {
            if (dynamicNavigationMeshSystem == null)
            {
                var gameSystemCollection = Services.GetServiceAs<IGameSystemCollection>();
                dynamicNavigationMeshSystem = gameSystemCollection.OfType<DynamicNavigationMeshSystem>().FirstOrDefault();
                if (dynamicNavigationMeshSystem != null)
                {
                    dynamicNavigationMeshSystem.NavigationUpdated += DynamicNavigationMeshSystemOnNavigationUpdated;
                }
            }

            foreach (var p in ComponentDatas)
            {
                // Should update selected navigation mesh?
                if (dynamicNavigationMeshSystem != null)
                {
                    if((dynamicNavigationComponents.Contains(p.Key) || p.Key.NavigationMesh == null) && dynamicNavigationMeshSystem.CurrentNavigationMesh != p.Value.LoadedNavigationMesh)
                    {
                        UpdateNavigationMesh(p.Key, p.Value);
                    }
                }
                else
                {
                    if (p.Key.NavigationMesh != p.Value.LoadedNavigationMesh)
                    {
                        UpdateNavigationMesh(p.Key, p.Value);
                    }
                }
            }
        }

        private void DynamicNavigationMeshSystemOnNavigationUpdated(object sender, EventArgs eventArgs)
        {
            var componentsToUpdate = dynamicNavigationComponents.ToArray();
            foreach (var component in componentsToUpdate)
            {
                UpdateNavigationMesh(component, ComponentDatas[component]);
            }
        }

        protected override void OnSystemRemove()
        {
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
            
            data.LoadedNavigationMesh = null;
            component.NavigationMeshInternal = IntPtr.Zero;
            dynamicNavigationComponents.Remove(component);
        }

        private void UpdateNavigationMesh(NavigationComponent component, AssociatedData data)
        {
            // Remove old reference
            RemoveReference(component, data);

            NavigationMesh targetNavigationMesh = component.NavigationMesh;

            // When the navigation mesh is not specified on the component, use the dynamic navigation mesh instead
            if (targetNavigationMesh == null && dynamicNavigationMeshSystem != null)
            {
                targetNavigationMesh = dynamicNavigationMeshSystem.CurrentNavigationMesh;
                dynamicNavigationComponents.Add(component);
            }

            if (component.NavigationMesh != null)
            {
                // Store scene offset of entity in the component, which will make all the queries local to the baked navigation mesh (for baked navigation only)
                component.SceneOffset = component.Entity.Scene.Offset;
            }
            else
            {
                component.SceneOffset = Vector3.Zero;
            }

            if (targetNavigationMesh != null)
            {
                // Mark new navigation mesh as loaded
                data.LoadedNavigationMesh = targetNavigationMesh;
            }
        }
        
        /// <summary>
        /// Associated data for navigation mesh components
        /// </summary>
        public class AssociatedData
        {
            public NavigationMesh LoadedNavigationMesh;
            public NavigationComponent Component;
            internal int SelectedLayer;
        }

        /// <summary>
        /// Recast native navigation mesh wrapper
        /// </summary>
        public class RecastNavigationMesh : IDisposable
        {
            private IntPtr navmesh;

            public RecastNavigationMesh(NavigationMesh navigationMesh)
            {
                navmesh = Navigation.CreateNavmesh(navigationMesh.TileSize * navigationMesh.CellSize);
            }

            public void Dispose()
            {
                Navigation.DestroyNavmesh(navmesh);
            }

            public static implicit operator IntPtr(RecastNavigationMesh obj)
            {
                return obj.navmesh;
            }
        }

/*
        internal class NavigationMeshInternal : IDisposable
        {
            private readonly float cellTileSize;
            private readonly HashSet<object> references = new HashSet<object>();

            public IntPtr[] Layers;

            public NavigationMeshInternal(NavigationMesh navigationMesh)
            {
                cellTileSize = navigationMesh.TileSize * navigationMesh.CellSize;
                Layers = new IntPtr[navigationMesh];
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
*/
    }
}