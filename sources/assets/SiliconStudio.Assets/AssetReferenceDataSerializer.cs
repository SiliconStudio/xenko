// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Serializer for <see cref="AssetReference{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AssetReferenceDataSerializer<T> : DataSerializer<AssetReference<T>> where T : Asset
    {
        /// <inheritdoc/>
        public override void Serialize(ref AssetReference<T> assetReference, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(assetReference.Id);
                stream.Write(assetReference.Location);
            }
            else
            {
                var id = stream.Read<Guid>();
                var location = stream.ReadString();

                assetReference = new AssetReference<T>(id, location);
            }
        }
    }
}