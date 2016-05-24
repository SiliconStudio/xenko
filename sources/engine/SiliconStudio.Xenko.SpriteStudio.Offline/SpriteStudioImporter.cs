using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.SpriteStudio.Runtime;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    internal class SpriteStudioImporter : AssetImporterBase
    {
        private const string FileExtensions = ".ssae";

        public override IEnumerable<Type> RootAssetTypes
        {
            get
            {
                yield return typeof(SpriteStudioModelAsset);
                yield return typeof(SpriteStudioAnimationAsset);
            }
        }

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var outputAssets = new List<AssetItem>();

            if (!SpriteStudioXmlImport.SanityCheck(rawAssetPath))
            {
                importParameters.Logger.Error("Invalid xml file or some required files are missing.");
                return null;
            }

            //pre-process models
            var nodes = new List<SpriteStudioNode>();
            string modelName;
            if (!SpriteStudioXmlImport.ParseModel(rawAssetPath, nodes, out modelName))
            {
                importParameters.Logger.Error("Failed to parse Sprite Studio model.");
                return null;
            }

            if (importParameters.IsTypeSelectedForOutput<SpriteStudioModelAsset>())
            {
                var model = new SpriteStudioModelAsset { Source = rawAssetPath };
                foreach (var node in nodes)
                {
                    model.NodeNames.Add(node.Name);
                }
                outputAssets.Add(new AssetItem(modelName, model));
            }

            if (importParameters.IsTypeSelectedForOutput<SpriteStudioAnimationAsset>())
            {
                //pre-process anims
                var anims = new List<SpriteStudioAnim>();
                if (!SpriteStudioXmlImport.ParseAnimations(rawAssetPath, anims))
                {
                    importParameters.Logger.Error("Failed to parse Sprite Studio animations.");
                    return null;
                }

                foreach (var studioAnim in anims)
                {
                    var anim = new SpriteStudioAnimationAsset { Source = rawAssetPath, AnimationName = studioAnim.Name };
                    outputAssets.Add(new AssetItem(modelName + "_" + studioAnim.Name, anim));
                }    
            }

            return outputAssets;
        }

        public override Guid Id { get; } = new Guid("f0b76549-ed9c-4e74-8522-f44ec8e90806");
        public override string Description { get; } = "OPTPiX Sprite Studio Importer";

        public override string SupportedFileExtensions => FileExtensions;
    }
}
