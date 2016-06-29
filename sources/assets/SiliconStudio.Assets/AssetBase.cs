// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset item part of a <see cref="Package"/> accessible through <see cref="Package.Assets"/>.
    /// </summary>
    [DataContract("AssetBase")]
    [DataSerializer(typeof(AssetBase.Serializer))]
    [NonIdentifiable]
    public sealed class AssetBase : IReference
    {
        private readonly UFile location;
        private readonly Asset asset;

        /// <summary>
        /// The location used for the default import base.
        /// </summary>
        public static readonly UFile DefaultImportBase = new UFile("--import--");

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBase"/> class.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public AssetBase(Asset asset) : this(DefaultImportBase, asset)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItem"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="asset">The asset.</param>
        public AssetBase(UFile location, Asset asset)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (asset == null) throw new ArgumentNullException("asset");
            this.location = location;
            this.asset = asset;
        }

        /// <summary>
        /// Gets the location of this asset.
        /// </summary>
        /// <value>The location.</value>
        public string Location
        {
            get
            {
                return location;
            }
        }

        /// <summary>
        /// Gets the asset unique identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id
        {
            get
            {
                return asset.Id;
            }
        }

        /// <summary>
        /// Gets or sets the asset.
        /// </summary>
        /// <value>The asset.</value>
        public Asset Asset
        {
            get
            {
                return asset;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a root import base (the original import).
        /// </summary>
        /// <value><c>true</c> if this instance is an import base; otherwise, <c>false</c>.</value>
        public bool IsRootImport
        {
            get
            {
                return location == DefaultImportBase && Id == Guid.Empty;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} => {1}", location, Id);
        }

        internal class Serializer : DataSerializer<AssetBase>
        {
            public override void Serialize(ref AssetBase assetBase, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(assetBase.Location);
                    stream.SerializeExtended(assetBase.Asset, mode);
                }
                else
                {
                    var location = stream.ReadString();
                    Asset asset = null;
                    stream.SerializeExtended(ref asset, mode);
                    assetBase = new AssetBase(location, asset);
                }
            }
        }
    }
}
