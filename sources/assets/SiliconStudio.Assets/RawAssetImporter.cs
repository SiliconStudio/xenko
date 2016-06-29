// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public class RawAssetImporter : AssetImporterBase
    {
        private static readonly Guid Uid = new Guid("6F86EC95-C1CA-41E1-8ADC-1449BB5CE3BE");

        public RawAssetImporter()
        {
            // Raw asset is always last
            Order = int.MaxValue;
        }

        public override Guid Id => Uid;

        public override string Description => "Generic importer for raw assets";

        public override bool IsSupportingFile(string filePath)
        {
            // Always return true
            return true;
        }

        public override IEnumerable<Type> RootAssetTypes { get { yield return typeof(RawAsset); } }

        public override string SupportedFileExtensions => "*.*";

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var asset = new RawAsset { Source = rawAssetPath };

            // Creates the url to the raw asset
            var rawAssetUrl = new UFile(rawAssetPath.GetFileName());

            yield return new AssetItem(rawAssetUrl, asset);
        }
    }
}
