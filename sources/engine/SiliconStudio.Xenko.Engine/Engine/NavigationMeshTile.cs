// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Tiles contained within <see cref="NavigationMeshLayer"/>
    /// </summary>
    [DataContract("NavigationMeshTile")]
    [DataSerializer(typeof(NavigationMeshTileSerializer))]
    public class NavigationMeshTile
    {
        [DataMemberCustomSerializer]
        public Vector3[] MeshVertices;
        [DataMemberCustomSerializer]
        public byte[] NavmeshData;

        public override int GetHashCode()
        {
            return MeshVertices?.ComputeHash() ?? 0;
        }
    }

    /// <summary>
    /// Serializes individually build tiles inside navigation meshes
    /// </summary>
    internal class NavigationMeshTileSerializer : DataSerializer<NavigationMeshTile>, IDataSerializerInitializer
    {
        private DataSerializer<Vector3> pointSerializer;

        public void Initialize(SerializerSelector serializerSelector)
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

            int dataLength = tile.NavmeshData?.Length ?? 0;
            stream.Serialize(ref dataLength);
            if (mode == ArchiveMode.Deserialize)
                tile.NavmeshData = new byte[dataLength];

            if (dataLength > 0)
                stream.Serialize(tile.NavmeshData, 0, tile.NavmeshData.Length);
        }
    }
}