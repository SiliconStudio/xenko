// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Context used by <see cref="IAssetUpgrader"/>.
    /// </summary>
    public class AssetMigrationContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetMigrationContext"/>.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetReference"></param>
        /// <param name="assetFullPath"></param>
        /// <param name="log"></param>
        public AssetMigrationContext(Package package, IReference assetReference, string assetFullPath, ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            Package = package;
            AssetReference = assetReference;
            AssetFullPath = assetFullPath;
            Log = new AssetLogger(package, assetReference, assetFullPath, log);
        }

        /// <summary>
        /// The current package where the current asset is being migrated. This is null when the asset being migrated is a package.
        /// </summary>
        public Package Package { get; }

        public IReference AssetReference { get; }

        public string AssetFullPath { get; }

        /// <summary>
        /// The logger for this context.
        /// </summary>
        public ILogger Log { get; }
    }
}
