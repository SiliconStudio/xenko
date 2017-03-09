// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Xenko.Navigation
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
        // Stores the cached build information to allow incremental building on this navigation mesh
        internal NavigationMeshTileCache TileCache;

        // Initialized build settings, only used at build time
        [DataMemberIgnore] internal NavigationMeshBuildSettings BuildSettings;

        // Backing value of Layers and NumLayers
        [DataMemberCustomSerializer] internal readonly List<NavigationMeshLayer> LayersInternal = new List<NavigationMeshLayer>();
        
        /// <summary>
        /// Multiple layers corresponding to multiple agent settings
        /// </summary>
        public IReadOnlyList<NavigationMeshLayer> Layers => LayersInternal;

        /// <summary>
        /// Number of layers
        /// </summary>
        public int NumLayers => LayersInternal.Count;
        
        /// <summary>
        /// Used to initialize the navigation mesh so it will allow building tiles
        /// This will store the build settings and create the amount of layers corresponding to the number of NavigationAgentSettings
        /// </summary>
        /// <param name="buildSettings"></param>
        /// <param name="agentSettings">Agent setting to use</param>
        public void Initialize(NavigationMeshBuildSettings buildSettings, NavigationAgentSettings[] agentSettings)
        {
            BuildSettings = buildSettings;

            // Remove layers that are no longer needed
            if (LayersInternal.Count > agentSettings.Length)
                LayersInternal.RemoveRange(agentSettings.Length, LayersInternal.Count - agentSettings.Length);

            // Initialize layers
            for (int i = 0; i < agentSettings.Length; i++)
            {
                NavigationMeshLayer layer;
                if (LayersInternal.Count <= i)
                {
                    layer = new NavigationMeshLayer();
                    LayersInternal.Add(layer);
                }
                else
                {
                    layer = LayersInternal[i];
                }
                layer.AgentSettings = agentSettings[i];
                layer.BuildSettings = buildSettings;
            }
        }

        /// <summary>
        /// Build a single tile for all layers
        /// Initialize should have been called before calling this
        /// </summary>
        /// <param name="inputVertices">Input vertex data for the input mesh</param>
        /// <param name="inputIndices">Input index data for the input mesh</param>
        /// <param name="boundingBox">Bounding box of the tile</param>
        /// <param name="tileCoordinate">Tile coordinate to of the tile to build</param>
        /// <returns>A list of built tiles</returns>
        public List<NavigationMeshTile> BuildAllLayers(Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            List<NavigationMeshTile> builtTiles = new List<NavigationMeshTile>();

            for (int i = 0; i < LayersInternal.Count; i++)
            {
                NavigationMeshLayer layer = Layers[i];
                layer.BuildTile(inputVertices, inputIndices, boundingBox, tileCoordinate);
            }

            return builtTiles;
        }

        internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>
        {
            private DictionarySerializer<Point, NavigationMeshTile> tilesSerializer;
            private DataSerializer<BoundingBox> boundingBoxSerializer;
            private DataSerializer<NavigationMeshTileCache> tileCacheSerializer;

            public override void Initialize(SerializerSelector serializerSelector)
            {
                boundingBoxSerializer = MemberSerializer<BoundingBox>.Create(serializerSelector, false);
                tileCacheSerializer = MemberSerializer<NavigationMeshTileCache>.Create(serializerSelector, false);
                tilesSerializer = new DictionarySerializer<Point, NavigationMeshTile>();
                tilesSerializer.Initialize(serializerSelector);
            }

            public override void PreSerialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
            {
                base.PreSerialize(ref obj, mode, stream);
                if (mode == ArchiveMode.Deserialize)
                {
                    if (obj == null)
                        obj = new NavigationMesh();
                }
            }

            public override void Serialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
            {
                // Serialize tile size because it is needed
                stream.Serialize(ref obj.BuildSettings.TileSize);
                stream.Serialize(ref obj.BuildSettings.CellSize);

                tileCacheSerializer.Serialize(ref obj.TileCache, mode, stream);

                int numLayers = obj.Layers.Count;
                stream.Serialize(ref numLayers);
                if (mode == ArchiveMode.Deserialize)
                    obj.LayersInternal.Clear();

                for (int l = 0; l < numLayers; l++)
                {
                    NavigationMeshLayer layer;
                    if (mode == ArchiveMode.Deserialize)
                    {
                        // Create a new layer
                        layer = new NavigationMeshLayer();
                        obj.LayersInternal.Add(layer);
                    }
                    else
                        layer = obj.LayersInternal[l];

                    tilesSerializer.Serialize(ref layer.TilesInternal, mode, stream);
                }
            }
        }
    }
}