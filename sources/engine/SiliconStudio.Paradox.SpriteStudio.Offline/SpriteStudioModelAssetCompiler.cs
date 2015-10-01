using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.SpriteStudio.Runtime;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    internal class SpriteStudioModelAssetCompiler : AssetCompilerBase<SpriteStudioModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteStudioModelAsset asset, AssetCompilerResult result)
        {
            var gameSettingsAsset = context.GetGameSettingsAsset();
            var colorSpace = context.GetColorSpace();

            var cells = new List<SpriteStudioCell>();
            var images = new List<UFile>();
            if (!SpriteStudioXmlImport.ParseCellMaps(asset.Source, images, cells)) throw new Exception("Failed to parse Sprite Studio cell textures.");

            var texIndex = 0;
            asset.BuildTextures.Clear();
            foreach (var texture in images)
            {
                var textureAsset = new TextureAsset
                {
                    Id = Guid.Empty, // CAUTION: It is important to use an empty GUID here, as we don't want the command to be rebuilt (by default, a new asset is creating a new guid)
                    Alpha = AlphaFormat.Auto,
                    Format = TextureFormat.Color32Bits,
                    GenerateMipmaps = true,
                    PremultiplyAlpha = true,
                    ColorSpace = TextureColorSpace.Auto,
                    Hint = TextureHint.Color
                };

                result.BuildSteps.Add(
                new TextureAssetCompiler.TextureConvertCommand(
                    urlInStorage + texIndex,
                    new TextureConvertParameters(texture, textureAsset, context.Platform,
                        context.GetGraphicsPlatform(), gameSettingsAsset.DefaultGraphicsProfile,
                        gameSettingsAsset.TextureQuality, colorSpace)));

                asset.BuildTextures.Add(urlInStorage + texIndex);

                texIndex++;
            }

            result.BuildSteps.Add(new AssetBuildStep(AssetItem)
            {
                new SpriteStudioModelAssetCommand(urlInStorage, asset)
            });
        }

        /// <summary>
        /// Command used by the build engine to convert the asset
        /// </summary>
        private class SpriteStudioModelAssetCommand : AssetCommand<SpriteStudioModelAsset>
        {
            public SpriteStudioModelAssetCommand(string url, SpriteStudioModelAsset asset)
                : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var nodes = new List<SpriteStudioNode>();
                string modelName;
                if (!SpriteStudioXmlImport.ParseModel(AssetParameters.Source, nodes, out modelName)) return null;

                var cells = new List<SpriteStudioCell>();
                var textures = new List<UFile>();
                if (!SpriteStudioXmlImport.ParseCellMaps(AssetParameters.Source, textures, cells)) return null;

                var assetManager = new AssetManager();

                var sheet = new SpriteSheet();

                foreach (var cell in cells)
                {
                    var sprite = new Sprite(cell.Name, AttachedReferenceManager.CreateSerializableVersion<Texture>(Guid.Empty, AssetParameters.BuildTextures[cell.TextureIndex]))
                    {
                        Region = cell.Rectangle,
                        Center = cell.Pivot,
                        IsTransparent = true
                    };
                    sheet.Sprites.Add(sprite);
                }

                var spriteStudioSheet = new SpriteStudioSheet
                {
                    NodesInfo = nodes,
                    SpriteSheet = sheet
                };

                assetManager.Save(Url, spriteStudioSheet);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}