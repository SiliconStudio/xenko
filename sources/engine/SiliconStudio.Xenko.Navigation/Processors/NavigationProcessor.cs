// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
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
        private Dictionary<NavigationMesh, NavigationMeshData> loadedNavigationMeshes = new Dictionary<NavigationMesh, NavigationMeshData>();

        private DynamicNavigationMeshSystem dynamicNavigationMeshSystem;
        private GameSystemCollection gameSystemCollection;

        public override void Update(GameTime time)
        {
            // Update scene offsets for navigation components
            foreach (var p in ComponentDatas)
            {
                UpdateSceneOffset(p.Value);
            }
        }

        protected override void OnSystemAdd()
        {
            gameSystemCollection = Services.GetServiceAs<IGameSystemCollection>() as GameSystemCollection;
            if(gameSystemCollection == null)
                throw new Exception("NavigationProcessor can not access the game systems collection");

            gameSystemCollection.CollectionChanged += GameSystemsOnCollectionChanged;
        }

        protected override void OnSystemRemove()
        {
            if (gameSystemCollection != null)
            {
                gameSystemCollection.CollectionChanged += GameSystemsOnCollectionChanged;
            }

            if (dynamicNavigationMeshSystem != null)
            {
                dynamicNavigationMeshSystem.NavigationUpdated -= DynamicNavigationMeshSystemOnNavigationUpdated;
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
            UpdateNavigationMesh(data);

            // Handle either a change of NavigationMesh or Group
            data.Component.NavigationMeshChanged += ComponentOnNavigationMeshChanged;
        }

        protected override void OnEntityComponentRemoved(Entity entity, NavigationComponent component, AssociatedData data)
        {
            data.Component.NavigationMeshChanged -= ComponentOnNavigationMeshChanged;
        }

        private void DynamicNavigationMeshSystemOnNavigationUpdated(object sender, EventArgs eventArgs)
        {
            // Send updates for components using dynamic navigation meshes
            var componentsToUpdate = ComponentDatas.Values.Where(x=>x.Component.NavigationMesh == null).ToArray();
            foreach (var component in componentsToUpdate)
            {
                UpdateNavigationMesh(component);
            }
        }

        private void ComponentOnNavigationMeshChanged(object sender, EventArgs eventArgs)
        {
            var data = ComponentDatas[(NavigationComponent)sender];
            UpdateNavigationMesh(data);
        }

        private void UpdateNavigationMesh(AssociatedData data)
        {
            var navigationMeshToLoad = data.Component.NavigationMesh;
            if (navigationMeshToLoad == null && dynamicNavigationMeshSystem != null)
            {
                // Load dynamic navigation mesh when no navigation mesh is specified on the component
                navigationMeshToLoad = dynamicNavigationMeshSystem.CurrentNavigationMesh;
            }

            NavigationMeshGroupData loadedGroup = Load(navigationMeshToLoad, data.Component.Group);
            if (data.LoadedGroup != null)
                Unload(data.LoadedGroup);
            
            data.Component.RecastNavigationMesh = loadedGroup?.RecastNavigationMesh;
            data.LoadedGroup = loadedGroup;

            UpdateSceneOffset(data);
        }

        private void UpdateSceneOffset(AssociatedData data)
        {
            // Store scene offset of entity in the component, which will make all the queries local to the baked navigation mesh (for baked navigation only)
            data.Component.SceneOffset = data.Component.NavigationMesh != null ? data.Component.Entity.Scene.Offset : Vector3.Zero;
        }
        
        private void GameSystemsOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            // Detect addition of dynamic navigation mesh system
            if (dynamicNavigationMeshSystem == null)
            {
                dynamicNavigationMeshSystem = gameSystemCollection.OfType<DynamicNavigationMeshSystem>().FirstOrDefault();
                if (dynamicNavigationMeshSystem != null)
                {
                    dynamicNavigationMeshSystem.NavigationUpdated += DynamicNavigationMeshSystemOnNavigationUpdated;
                }
            }
        }

        /// <summary>
        /// Loads or references a <see cref="RecastNavigationMesh"/> for a group of a navigation mesh
        /// </summary>
        [CanBeNull]
        private NavigationMeshGroupData Load(NavigationMesh mesh, NavigationMeshGroup group)
        {
            if (mesh == null || group == null)
                return null;

            NavigationMeshData data;
            if (!loadedNavigationMeshes.TryGetValue(mesh, out data))
            {
                loadedNavigationMeshes.Add(mesh, data = new NavigationMeshData());
            }

            NavigationMeshGroupData groupData;
            if (!data.LoadedGroups.TryGetValue(group.Id, out groupData))
            {
                NavigationMeshLayer layer;
                if (!mesh.Layers.TryGetValue(group.Id, out layer))
                    return null; // Group not present in navigation mesh
                
                data.LoadedGroups.Add(group.Id, groupData = new NavigationMeshGroupData
                {
                    NavigationMesh = mesh,
                    RecastNavigationMesh = new RecastNavigationMesh(mesh),
                });

                // Add initial tiles to the navigation mesh
                foreach (var tile in layer.Tiles)
                {
                    if(!groupData.RecastNavigationMesh.AddOrReplaceTile(tile.Value.Data))
                        throw new InvalidOperationException("Failed to add tile");
                }
            }

            groupData.AddReference();
            return groupData;
        }

        /// <summary>
        /// Removes a reference to a group
        /// </summary>
        private void Unload(NavigationMeshGroupData mesh)
        {
            int referenceCount = mesh.Release();
            if (referenceCount < 0)
                throw new ArgumentOutOfRangeException();

            if(referenceCount == 0)
            {
                // Remove group

            }
        }

        /// <summary>
        /// Associated data for navigation mesh components
        /// </summary>
        public class AssociatedData
        {
            public NavigationComponent Component;
            public NavigationMeshGroupData LoadedGroup;
        }

        /// <summary>
        /// Contains groups that are loaded for a navigation mesh
        /// </summary>
        public class NavigationMeshData
        {
            public readonly Dictionary<Guid, NavigationMeshGroupData> LoadedGroups = new Dictionary<Guid, NavigationMeshGroupData>();
        }

        /// <summary>
        /// A loaded group of a navigation mesh
        /// </summary>
        public class NavigationMeshGroupData : IReferencable
        {
            public NavigationMesh NavigationMesh;
            public RecastNavigationMesh RecastNavigationMesh;

            public int ReferenceCount { get; private set; } = 0;
            public int AddReference()
            {
                return --ReferenceCount;
            }

            public int Release()
            {
                return ++ReferenceCount;
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