// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Paradox.Assets.Textures
{
    public class TextureImporter : AssetImporterBase
    {
        // Supported file extensions for this importer
        public const string FileExtensions = ".dds,.jpg,.png,.gif,.bmp,.tga,.psd";

        private static readonly Guid uid = new Guid("a60986f3-a594-4278-bd9d-68ea172f0558");
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
                return "Texture importer for creating Texture assets";
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
        private static readonly Type[] supportedTypes = { typeof(TextureAsset) };

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var asset = new TextureAsset { Source = rawAssetPath };

            // Creates the url to the texture
            var textureUrl = new UFile(rawAssetPath.GetFileName(), null);

            yield return new AssetItem(textureUrl, asset);
        }
    }
}