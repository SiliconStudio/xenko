// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public abstract class RawAssetImporterBase<TAsset> : AssetImporterBase
        where TAsset : AssetWithSource, new()
    {
        /// <inheritdoc />
        public sealed override IEnumerable<Type> RootAssetTypes { get { yield return typeof(TAsset); } }

        /// <inheritdoc />
        [ItemNotNull]
        public sealed override IEnumerable<AssetItem> Import([NotNull] UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            if (rawAssetPath == null) throw new ArgumentNullException(nameof(rawAssetPath));

            var asset = new TAsset { Source = rawAssetPath };
            // Creates the url to the raw asset
            var rawAssetUrl = new UFile(rawAssetPath.GetFileNameWithoutExtension());
            yield return new AssetItem(rawAssetUrl, asset);
        }
    }
}
