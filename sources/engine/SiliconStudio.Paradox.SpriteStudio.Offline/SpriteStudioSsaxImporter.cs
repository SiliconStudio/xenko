using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.SpriteStudio.Runtime;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    internal class SpriteStudioSsaxImporter : AssetImporterBase
    {
        private const string FileExtensions = ".ssax";

        private static readonly Type[] SupportedTypes = { typeof(SpriteStudioSheetAsset), typeof(TextureAsset), typeof(SpriteStudioAnimationAsset) };

        public override AssetImporterParameters GetDefaultParameters(bool isForReImport)
        {
            return new AssetImporterParameters(SupportedTypes);
        }

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var outputAssets = new List<AssetItem>();

            var sheet = new SpriteStudioSheetAsset();
            var anim = new SpriteStudioAnimationAsset();

            sheet.Source = rawAssetPath;
            outputAssets.Add(new AssetItem(rawAssetPath.GetFileName() + "_sheet", sheet));

            anim.Source = rawAssetPath;
            outputAssets.Add(new AssetItem(rawAssetPath.GetFileName() + "_anim", anim));

            return outputAssets;
        }

        public override Guid Id { get; } = new Guid("f0b76549-ed9c-4e74-8522-f44ec8e90806");
        public override string Description { get; } = "OPTPiX SSAX Xml Importer";

        public override string SupportedFileExtensions => FileExtensions;
    }
}