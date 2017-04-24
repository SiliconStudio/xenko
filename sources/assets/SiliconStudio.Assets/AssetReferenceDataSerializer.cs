// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Serializer for <see cref="AssetReference"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AssetReferenceDataSerializer : DataSerializer<AssetReference>
    {
        /// <inheritdoc/>
        public override void Serialize(ref AssetReference assetReference, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(assetReference.Id);
                stream.Write(assetReference.Location);
            }
            else
            {
                var id = stream.Read<AssetId>();
                var location = stream.ReadString();

                assetReference = new AssetReference(id, location);
            }
        }
    }
}
