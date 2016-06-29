// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Sprite;

namespace SiliconStudio.Xenko.Assets.Textures
{
    public class TextureImporter : AssetImporterBase
    {
        // Supported file extensions for this importer
        public const string FileExtensions = ".dds,.jpg,.jpeg,.png,.gif,.bmp,.tga,.psd,.tif,.tiff";

        private static readonly Guid Uid = new Guid("a60986f3-a594-4278-bd9d-68ea172f0558");
        public override Guid Id => Uid;

        public override string Description => "Texture importer for creating Texture assets";

        public override string SupportedFileExtensions => FileExtensions;

        public override IEnumerable<Type> RootAssetTypes
        {
            get
            {
                yield return typeof(TextureAsset);
                yield return typeof(SpriteSheetAsset); // TODO: this is temporary, until we can make the asset templates ask compilers instead of importer which type they support
            }
        }

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var asset = new TextureAsset { Source = rawAssetPath };

            // Creates the url to the texture
            var textureUrl = new UFile(rawAssetPath.GetFileName());

            yield return new AssetItem(textureUrl, asset);
        }
    }
}
