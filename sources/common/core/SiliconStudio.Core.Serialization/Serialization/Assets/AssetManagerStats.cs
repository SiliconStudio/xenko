// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Serialization.Assets
{
    /// <summary>
    /// A class representing stats computed for an instance of <see cref="ContentManager"/> at a given time. This class
    /// is intended to be used for debug purpose only.
    /// </summary>
    public class AssetManagerStats
    {
        /// <summary>
        /// A class representing information on a single loaded asset. This class is intended to be used for debug purpose only.
        /// </summary>
        public class LoadedAsset
        {
            /// <summary>
            /// The url of the loaded asset.
            /// </summary>
            public readonly string Url;
            /// <summary>
            /// The public reference count, corresponding to the number of times this asset has been manually loaded.
            /// </summary>
            public readonly int PublicReferenceCount;
            /// <summary>
            /// The private reference count, corresponding to the number of times this asset has been loaded indirectly because it is referenced by another asset.
            /// </summary>
            public readonly int PrivateReferenceCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="LoadedAsset"/> class.
            /// </summary>
            /// <param name="url">The url of the asset.</param>
            /// <param name="publicReferenceCount">The public reference count.</param>
            /// <param name="privateReferenceCount">The private reference count.</param>
            internal LoadedAsset(string url, int publicReferenceCount, int privateReferenceCount)
            {
                Url = url;
                PublicReferenceCount = publicReferenceCount;
                PrivateReferenceCount = privateReferenceCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetManagerStats"/> class.
        /// </summary>
        /// <param name="assetReferences">The collection of <see cref="ContentManager.AssetReference"/> representing the currently loaded assets.</param>
        internal AssetManagerStats(IEnumerable<ContentManager.AssetReference> assetReferences)
        {
            LoadedAssets = new List<LoadedAsset>(assetReferences.Select(x => new LoadedAsset(x.Url, x.PublicReferenceCount, x.PrivateReferenceCount)));
        }

        /// <summary>
        /// Gets a collection representing information on all currently loaded assets.
        /// </summary>
        public IReadOnlyCollection<LoadedAsset> LoadedAssets { get; private set; }
    }
}