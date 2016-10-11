// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Native;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A Navigation Mesh, can be used for pathfinding.
    /// </summary>
    [DataContract("NavigationMesh")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        /// <summary>
        /// Multiple layers corresponding to multiple agent settings
        /// </summary>
        [DataMemberCustomSerializer] public NavigationMeshLayer[] Layers;

        /// <summary>
        /// Bounding box used when building
        /// </summary>
        public BoundingBox BoundingBox;

        // Initialized build settings, only used at build time
        [DataMemberIgnore]
        internal NavigationMeshBuildSettings BuildSettings;
        
        // Used internally to detect tile changes
        internal int TileHash;

        /// <summary>
        /// Tries to find a built tile inside the navigation mesh
        /// </summary>
        /// <param name="layerIndex">The layer on which you want to search</param>
        /// <param name="tileCoordinate"></param>
        /// <returns>The found tile or null</returns>
        public NavigationMeshTile FindTile(int layerIndex, Point tileCoordinate)
        {
            if (layerIndex < 0 || layerIndex >= Layers.Length)
                return null;
            NavigationMeshLayer layer = Layers[layerIndex];
            NavigationMeshTile foundTile;
            if (layer.Tiles.TryGetValue(tileCoordinate, out foundTile))
                return foundTile;
            return null;
        }
        
        /// <summary>
        /// Used to initialize the navigation mesh so it will allow building tiles
        /// This will store the build settings and create the amount of layers corresponding to the number of NavigationAgentSettings
        /// </summary>
        /// <param name="buildSettings"></param>
        /// <param name="agentSettings">Agent setting to use</param>
        public void Initialize(NavigationMeshBuildSettings buildSettings, NavigationAgentSettings[] agentSettings)
        {
            this.BuildSettings = buildSettings;
            if (agentSettings.Length > 0)
            {
                Layers = new NavigationMeshLayer[agentSettings.Length];
                for (int i = 0; i < agentSettings.Length; i++)
                {
                    Layers[i] = new NavigationMeshLayer();
                    Layers[i].AgentSettings = agentSettings[i];
                }
            }
            else
                Layers = null;
        }

        /// <summary>
        /// Build a specific navigation mesh tile for all layers
        /// </summary>
        /// <param name="tile">The tile to build</param>
        /// <param name="boundingBox">the bound</param>
        /// <param name="buildSettings"></param>
        /// <returns>A list of built tiles</returns>
        public List<NavigationMeshTile> BuildTile(Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            List<NavigationMeshTile> builtTiles = new List<NavigationMeshTile>();

            for (int i = 0; i < Layers.Length; i++)
            {
                NavigationMeshLayer layer = Layers[i];
                NavigationMeshTile tile = BuildTileInternal(layer, inputVertices, inputIndices, boundingBox, tileCoordinate);
                
                // Remove old tile
                if (layer.Tiles.ContainsKey(tileCoordinate))
                    layer.Tiles.Remove(tileCoordinate);

                if (tile != null && tile.NavmeshData != null)
                {
                    // Add
                    layer.Tiles.Add(tileCoordinate, tile);
                    builtTiles.Add(tile);
                }
            }

            // Update tile hash
            UpdateTileHash();

            return builtTiles;
        }

        /// <summary>
        /// Build a single tile for a single layer
        /// </summary>
        /// <returns></returns>
        public NavigationMeshTile BuildLayerTile(int layerIndex,
            Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            NavigationMeshLayer layer = Layers[layerIndex];

            // Remove old tile
            if (layer.Tiles.ContainsKey(tileCoordinate))
                layer.Tiles.Remove(tileCoordinate);

            NavigationMeshTile tile = BuildTileInternal(layer, inputVertices, inputIndices, boundingBox, tileCoordinate);
            if (tile != null && tile.NavmeshData != null)
            {
                // Add
                layer.Tiles.Add(tileCoordinate, tile);
            }
            
            // Update tile hash
            UpdateTileHash();

            return tile;
        }
        
        /// <summary>
        /// Removes a tile from a layer with given coordinate
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="tileCoordinate"></param>
        public void RemoveLayerTile(int layerIndex, Point tileCoordinate)
        {
            if (layerIndex < 0 || layerIndex >= Layers.Length)
                return;
            NavigationMeshLayer layer = Layers[layerIndex];
            layer.Tiles.Remove(tileCoordinate);
        }

        /// <summary>
        /// Updates <see cref="TileHash"/> to be "unique" to this specific combination of navigation mesh
        /// </summary>
        internal void UpdateTileHash()
        {
            TileHash = 0;
            if (Layers == null)
                return;

            foreach (NavigationMeshLayer layer in Layers)
            {
                foreach (var tile in layer.Tiles)
                {
                    TileHash = (TileHash * 397) ^ tile.GetHashCode();
                    TileHash = (TileHash * 397) ^ tile.Value.GetHashCode();
                }
            }
        }
        
        /// <summary>
        /// Builds a single tile for a given layer without adding it
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="inputVertices"></param>
        /// <param name="inputIndices"></param>
        /// <param name="boundingBox"></param>
        /// <param name="tileCoordinate"></param>
        /// <returns></returns>
        private unsafe NavigationMeshTile BuildTileInternal(NavigationMeshLayer layer,
            Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            // Turn settings into native structure format
            NavigationAgentSettings agentSettings = layer.AgentSettings;
            NavigationMeshTile tile = new NavigationMeshTile();

            // Initialize navigation builder
            IntPtr nav = Navigation.CreateBuilder();

            // Turn build settings into native structure format
            Navigation.BuildSettings internalBuildSettings = new Navigation.BuildSettings
            {
                // Tile settings
                BoundingBox = boundingBox,
                TilePosition = tileCoordinate,
                TileSize =  BuildSettings.TileSize,

                // General build settings
                CellHeight =  BuildSettings.CellHeight,
                CellSize = BuildSettings.CellSize,
                RegionMinSize = BuildSettings.RegionMinSize,
                RegionMergeSize = BuildSettings.RegionMergeSize,
                EdgeMaxLen = BuildSettings.EdgeMaxLen,
                EdgeMaxError = BuildSettings.EdgeMaxError,
                DetailSampleDistInput = BuildSettings.DetailSampleDistInput,
                DetailSampleMaxErrorInput = BuildSettings.DetailSampleMaxErrorInput,
                
                // Agent settings
                AgentHeight = agentSettings.Height,
                AgentRadius = agentSettings.Radius,
                AgentMaxClimb = agentSettings.MaxClimb,
                AgentMaxSlope = agentSettings.MaxSlope.Degrees
            };
            Navigation.SetSettings(nav, new IntPtr(&internalBuildSettings));
            
            // Generate mesh
            Navigation.GeneratedData data;
            IntPtr ret = Navigation.Build(nav, inputVertices.ToArray(), inputVertices.Length, inputIndices.ToArray(), inputIndices.Length);
            Navigation.GeneratedData* dataPtr = (Navigation.GeneratedData*)ret;
            data = *dataPtr;

            // Copy output data on success
            if (data.Success)
            {
                List<Vector3> outputVerts = new List<Vector3>();
                if (data.NumNavmeshVertices > 0)
                {
                    Vector3* navmeshVerts = (Vector3*)data.NavmeshVertices;
                    for (int j = 0; j < data.NumNavmeshVertices; j++)
                    {
                        outputVerts.Add(navmeshVerts[j]);
                    }

                    tile.MeshVertices = outputVerts.ToArray();
                }

                // Copy the generated navigationMesh data
                tile.NavmeshData = new byte[data.NavmeshDataLength];
                Marshal.Copy(data.NavmeshData, tile.NavmeshData, 0, data.NavmeshDataLength);
            }

            // Cleanup builder
            Navigation.DestroyBuilder(nav);

            return tile;
        }
    }
    
    internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>, IDataSerializerInitializer
    {
        private DictionarySerializer<Point, NavigationMeshTile> tilesSerializer;
        private DataSerializer<BoundingBox> boundingBoxSerializer;

        public void Initialize(SerializerSelector serializerSelector)
        {
            boundingBoxSerializer = MemberSerializer<BoundingBox>.Create(serializerSelector, false);
            tilesSerializer = new DictionarySerializer<Point, NavigationMeshTile>();
            tilesSerializer.Initialize(serializerSelector);
        }
        public override void Serialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
        {
            // Serialize tile size because it is needed
            stream.Serialize(ref obj.BuildSettings.TileSize);
            stream.Serialize(ref obj.BuildSettings.CellSize);

            boundingBoxSerializer.Serialize(ref obj.BoundingBox, mode, stream);

            int numLayers = obj.Layers?.Length ?? 0;
            stream.Serialize(ref numLayers);
            if (mode == ArchiveMode.Deserialize)
                obj.Layers = new NavigationMeshLayer[numLayers];

            for (int l = 0; l < numLayers; l++)
            {
                NavigationMeshLayer layer;
                if (mode == ArchiveMode.Deserialize)
                    layer = obj.Layers[l] = new NavigationMeshLayer();
                else
                    layer = obj.Layers[l];
                
                tilesSerializer.Serialize(ref layer.Tiles, mode, stream);
            }

            if(mode == ArchiveMode.Deserialize)
                obj.UpdateTileHash();
        }
    }
}