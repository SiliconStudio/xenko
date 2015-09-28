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
using SiliconStudio.Core.Mathematics;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    internal class SpriteStudioSheetAssetCompiler : AssetCompilerBase<SpriteStudioSheetAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteStudioSheetAsset asset, AssetCompilerResult result)
        {
            var xmlDoc = XDocument.Load(asset.Source);
            if (xmlDoc.Root == null) return;

            var nameSpace = xmlDoc.Root.Name.Namespace;

            var images = xmlDoc.Descendants(nameSpace + "Image").Select(x => UPath.Combine(asset.Source.GetFullDirectory(), new UFile(x.Attribute("Path").Value))).ToList();

            var gameSettingsAsset = context.GetGameSettingsAsset();
            var colorSpace = context.GetColorSpace();

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
                new SpriteStudioSheetAssetCommand(urlInStorage, asset)
            });
        }

        /// <summary>
        /// Command used by the build engine to convert the asset
        /// </summary>
        private class SpriteStudioSheetAssetCommand : AssetCommand<SpriteStudioSheetAsset>
        {
            public SpriteStudioSheetAssetCommand(string url, SpriteStudioSheetAsset asset)
                : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                int endFrame, fps;
                var nodes = new List<SpriteStudioNode>();
                var nodesData = new List<SpriteNodeData>();

                if (!SpriteStudioXmlImport.Load(AssetParameters.Source, nodes, nodesData, out endFrame, out fps)) return null;

                var assetManager = new AssetManager();

                var sortedNodes = nodes.OrderBy(x => x.BaseXyPrioAngle.Z);

                //sprite sheet and textures
                var sheet = new SpriteSheet();
                foreach (var node in sortedNodes)
                {
                    var sprite = node.PictureId != -1
                        ? new Sprite(node.Name, AttachedReferenceManager.CreateSerializableVersion<Texture>(Guid.Empty, AssetParameters.BuildTextures[node.PictureId]))
                        {
                            Region = node.Rectangle,
                            IsTransparent = true,
                            Center = node.Pivot
                        }
                        : new Sprite(node.Name);
                    sheet.Sprites.Add(sprite);
                    node.Sprite = sprite;
                }

                assetManager.Save(Url + "_sheet", sheet);

                var ssAnim = new SpriteStudioSheet
                {
                    NodesInfo = sortedNodes.ToList(),
                    SpriteSheet = sheet
                };
                assetManager.Save(Url, ssAnim);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}