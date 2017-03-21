// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
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
        internal Dictionary<Guid, NavigationMeshLayer> LayersInternal = new Dictionary<Guid, NavigationMeshLayer>();

        public IReadOnlyDictionary<Guid, NavigationMeshLayer> Layers => LayersInternal;

        internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>
        {
            private DictionarySerializer<Guid, NavigationMeshLayer> layersSerializer;
            private DataSerializer<NavigationMeshCache> cacheSerializer;

            public override void Initialize(SerializerSelector serializerSelector)
            {
                cacheSerializer = MemberSerializer<NavigationMeshCache>.Create(serializerSelector, false);
                
                layersSerializer = new DictionarySerializer<Guid, NavigationMeshLayer>();
                layersSerializer.Initialize(serializerSelector);
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
                layersSerializer.Serialize(ref obj.LayersInternal, mode, stream);
            }
        }
    }
}