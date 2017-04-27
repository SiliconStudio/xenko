// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Tiles contained within <see cref="NavigationMeshLayer"/>
    /// </summary>
    [DataContract("NavigationMeshTile")]
    [DataSerializer(typeof(NavigationMeshTileSerializer))]
    public class NavigationMeshTile
    {
        /// <summary>
        /// Vertices of the navigation mesh, used for visualization
        /// </summary>
        // TODO this data should be obtained from the Data blob instead, since it contains the same data
        [DataMemberCustomSerializer] public Vector3[] MeshVertices;

        /// <summary>
        /// Binary data of the built navigation mesh tile
        /// </summary>
        [DataMemberCustomSerializer] public byte[] Data;

        /// <summary>
        /// Serializes individually build tiles inside navigation meshes
        /// </summary>
        internal class NavigationMeshTileSerializer : DataSerializer<NavigationMeshTile>
        {
            private DataSerializer<Vector3> pointSerializer;

            public override void Initialize(SerializerSelector serializerSelector)
            {
                pointSerializer = MemberSerializer<Vector3>.Create(serializerSelector);
            }

            public override void Serialize(ref NavigationMeshTile tile, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                    tile = new NavigationMeshTile();

                int numMeshVertices = tile.MeshVertices?.Length ?? 0;
                stream.Serialize(ref numMeshVertices);
                if (mode == ArchiveMode.Deserialize)
                    tile.MeshVertices = new Vector3[numMeshVertices];

                for (int i = 0; i < numMeshVertices; i++)
                {
                    pointSerializer.Serialize(ref tile.MeshVertices[i], mode, stream);
                }

                int dataLength = tile.Data?.Length ?? 0;
                stream.Serialize(ref dataLength);
                if (mode == ArchiveMode.Deserialize)
                    tile.Data = new byte[dataLength];

                if (dataLength > 0)
                    stream.Serialize(tile.Data, 0, tile.Data.Length);
            }
        }
    }
}
