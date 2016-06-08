// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Audio
{
    public class RawMusicAssetImporter : AssetImporterBase
    {
        // Supported file extensions for this importer
        public const string FileExtensions = ".mp3,.wav";

        private static readonly Guid Uid = new Guid("5adcb5b0-7011-4d28-9741-23ec7c3df617");
        public override Guid Id => Uid;

        public override string Description => "Music importer for creating SoundMusic assets";

        public override string SupportedFileExtensions => FileExtensions;

        public override IEnumerable<Type> RootAssetTypes { get { yield return typeof(SoundMusicAsset); } }

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var asset = new SoundMusicAsset { Source = rawAssetPath };

            // Creates the url to the texture
            var textureUrl = new UFile(rawAssetPath.GetFileName());

            yield return new AssetItem(textureUrl, asset);
        }
    }
}
