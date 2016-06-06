// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Audio
{
    public class RawSoundAssetImporter : AssetImporterBase
    {
        // Supported file extensions for this importer
        public const string FileExtensions = ".wav";

        private static readonly Guid uid = new Guid("634842fa-d1db-45c2-b13d-bc11486dae4d");
        public override Guid Id
        {
            get
            {
                return uid;
            }
        }

        public override string Description
        {
            get
            {
                return "Raw sound importer for creating SoundEffect assets";
            }
        }

        public override string SupportedFileExtensions
        {
            get
            {
                return FileExtensions;
            }
        }

        public override AssetImporterParameters GetDefaultParameters(bool isForReImport)
        {
            return new AssetImporterParameters(supportedTypes);
        }
        private static readonly Type[] supportedTypes = { typeof(SoundEffectAsset) };

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var asset = new SoundEffectAsset { Source = rawAssetPath };

            // Creates the url to the texture
            var textureUrl = new UFile(rawAssetPath.GetFileName());

            yield return new AssetItem(textureUrl, asset);
        }
    }
}
