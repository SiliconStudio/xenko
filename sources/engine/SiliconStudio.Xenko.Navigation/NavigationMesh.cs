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
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        // Stores the cached build information to allow incremental building on this navigation mesh
        internal NavigationMeshCache Cache;

        internal float TileSize;

        internal float CellSize;

        // Backing value of Layers and NumLayers
        [DataMemberCustomSerializer]
        internal readonly List<NavigationMeshLayer> LayersInternal = new List<NavigationMeshLayer>();

        /// <summary>
        /// Multiple layers corresponding to multiple agent settings
        /// </summary>
        public IReadOnlyList<NavigationMeshLayer> Layers => LayersInternal;

        /// <summary>
        /// Number of layers
        /// </summary>
        public int NumLayers => LayersInternal.Count;

        internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>
        {
            private DictionarySerializer<Point, NavigationMeshTile> tilesSerializer;
            private DataSerializer<NavigationMeshCache> cacheSerializer;

            public override void Initialize(SerializerSelector serializerSelector)
            {
                cacheSerializer = MemberSerializer<NavigationMeshCache>.Create(serializerSelector, false);
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
                stream.Serialize(ref obj.TileSize);
                stream.Serialize(ref obj.CellSize);

                cacheSerializer.Serialize(ref obj.Cache, mode, stream);

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