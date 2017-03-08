// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Layer containing built tiles for a single <see cref="NavigationAgentSettings"/>
    /// </summary>
    public class NavigationMeshLayer
    {
        // Agent settings for generating this layer, only used at build time
        internal NavigationAgentSettings AgentSettings;

        // Build settings, only used at build time
        internal NavigationMeshBuildSettings BuildSettings;

        // Backing field of Tiles
        internal Dictionary<Point, NavigationMeshTile> TilesInternal = new Dictionary<Point, NavigationMeshTile>();

        /// <summary>
        /// Contains all the built tiles mapped to their tile coordinates
        /// </summary>
        public IReadOnlyDictionary<Point, NavigationMeshTile> Tiles => TilesInternal;

        /// <summary>
        /// Tries to find a built tile inside this layer
        /// </summary>
        /// <param name="tileCoordinate">The coordinate of the tile</param>
        /// <returns>The found tile or null</returns>
        public NavigationMeshTile FindTile(Point tileCoordinate)
        {
            NavigationMeshTile foundTile;
            if (TilesInternal.TryGetValue(tileCoordinate, out foundTile))
                return foundTile;
            return null;
        }

        /// <summary>
        /// Removes a tile with given coordinate
        /// </summary>
        /// <param name="tileCoordinate">The coordinate of the tile</param>
        public void RemoveLayerTile(Point tileCoordinate)
        {
            TilesInternal.Remove(tileCoordinate);
        }

        /// <summary>
        /// Build or rebuild a single tile for this layer
        /// </summary>
        /// <param name="inputVertices">Input vertex data for the input mesh</param>
        /// <param name="inputIndices">Input index data for the input mesh</param>
        /// <param name="boundingBox">Bounding box of the tile</param>
        /// <param name="tileCoordinate">Tile coordinate to of the tile to build</param>
        /// <returns>The tile that was built</returns>
        public NavigationMeshTile BuildTile(Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            // Remove old tile
            if (TilesInternal.ContainsKey(tileCoordinate))
                TilesInternal.Remove(tileCoordinate);

            NavigationMeshTile tile = BuildTileInternal(inputVertices, inputIndices, boundingBox, tileCoordinate);
            if (tile?.Data != null)
            {
                // Add
                TilesInternal.Add(tileCoordinate, tile);
            }

            return tile;
        }

        /// <summary>
        /// Builds a single tile for a given layer without adding it
        /// </summary>
        /// <param name="inputVertices">Input vertex data for the input mesh</param>
        /// <param name="inputIndices">Input index data for the input mesh</param>
        /// <param name="boundingBox">Bounding box of the tile</param>
        /// <param name="tileCoordinate">Tile coordinate to of the tile to build</param>
        /// <returns>Teh tile that was built</returns>
        private unsafe NavigationMeshTile BuildTileInternal(Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            // Turn settings into native structure format
            NavigationAgentSettings agentSettings = AgentSettings;
            NavigationMeshTile tile = new NavigationMeshTile();

            // Initialize navigation builder
            IntPtr nav = Navigation.CreateBuilder();

            // Turn build settings into native structure format
            Navigation.BuildSettings internalBuildSettings = new Navigation.BuildSettings
            {
                // Tile settings
                BoundingBox = boundingBox,
                TilePosition = tileCoordinate,
                TileSize = BuildSettings.TileSize,

                // General build settings
                CellHeight = BuildSettings.CellHeight,
                CellSize = BuildSettings.CellSize,
                RegionMinArea = BuildSettings.MinRegionArea,
                RegionMergeArea = BuildSettings.RegionMergeArea,
                EdgeMaxLen = BuildSettings.MaxEdgeLen,
                EdgeMaxError = BuildSettings.MaxEdgeError,
                DetailSampleDist = BuildSettings.DetailSamplingDistance,
                DetailSampleMaxError = BuildSettings.MaxDetailSamplingError,

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
                tile.Data = new byte[data.NavmeshDataLength];
                Marshal.Copy(data.NavmeshData, tile.Data, 0, data.NavmeshDataLength);
            }

            // Cleanup builder
            Navigation.DestroyBuilder(nav);

            return tile;
        }
    }
}