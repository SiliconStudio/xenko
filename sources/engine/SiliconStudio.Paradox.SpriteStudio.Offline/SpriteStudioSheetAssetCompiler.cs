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
                var xmlDoc = XDocument.Load(AssetParameters.Source);
                if (xmlDoc.Root == null) return null;

                var nameSpace = xmlDoc.Root.Name.Namespace;

                var nodes = new List<SpriteStudioNode>();

                var parts = xmlDoc.Descendants(nameSpace + "Part").ToList();
                foreach (var part in parts)
                {
                    var type = part.Descendants(nameSpace + "Type").First();
                    var name = part.Descendants(nameSpace + "Name").First();
                    if (type.Value == "1")
                    {
                        var node = new SpriteStudioNode { Name = name.Value, Id = -1, ParentId = -2 };
                        nodes.Add(node);
                    }
                    else if (type.Value == "0")
                    {
                        int nodeId, parentId;
                        if (!int.TryParse(part.Descendants(nameSpace + "ID").First().Value, out nodeId)) continue;
                        if (!int.TryParse(part.Descendants(nameSpace + "ParentID").First().Value, out parentId)) continue;

                        int textureId;
                        if (!int.TryParse(part.Descendants(nameSpace + "PicID").First().Value, out textureId)) continue;
                        var pictAreaX = part.Descendants(nameSpace + "PictArea").First();
                        int top, left, bottom, right;
                        if (!int.TryParse(pictAreaX.Descendants(nameSpace + "Top").First().Value, out top)) continue;
                        if (!int.TryParse(pictAreaX.Descendants(nameSpace + "Left").First().Value, out left)) continue;
                        if (!int.TryParse(pictAreaX.Descendants(nameSpace + "Bottom").First().Value, out bottom)) continue;
                        if (!int.TryParse(pictAreaX.Descendants(nameSpace + "Right").First().Value, out right)) continue;
                        var rect = new RectangleF(left, top, right - left, bottom - top);

                        int pivotX, pivotY;
                        if (!int.TryParse(part.Descendants(nameSpace + "OriginX").First().Value, out pivotX)) continue;
                        if (!int.TryParse(part.Descendants(nameSpace + "OriginY").First().Value, out pivotY)) continue;

                        var node = new SpriteStudioNode
                        {
                            Name = name.Value,
                            Id = nodeId,
                            ParentId = parentId,
                            PictureId = textureId,
                            Rectangle = rect,
                            Pivot = new Vector2(pivotX, pivotY)
                        };

                        //discover base pose (frame 1)

                        var attribs = part.Descendants(nameSpace + "Attribute");
                        foreach (var attrib in attribs)
                        {
                            var tag = attrib.Attributes("Tag").First().Value;
                            var keys = attrib.Descendants(nameSpace + "Key");
                            var values = keys.Select(key => new Dictionary<string, string>
                        {
                            {"time", key.Attribute("Time").Value}, {"value", key.Descendants(nameSpace + "Value").FirstOrDefault().Value}
                        }).ToList();

                            switch (tag)
                            {
                                case "POSX":
                                    node.BaseXyPrioAngle.X = Convert.ToSingle(values.Select(x => x.First(y => y.Key == "value").Value).First());
                                    break;

                                case "POSY":
                                    node.BaseXyPrioAngle.Y = -Convert.ToSingle(values.Select(x => x.First(y => y.Key == "value").Value).First());
                                    break;

                                case "PRIO":
                                    node.BaseXyPrioAngle.Z = Convert.ToSingle(values.Select(x => x.First(y => y.Key == "value").Value).First());
                                    break;

                                case "ANGL":
                                    node.BaseXyPrioAngle.W = MathUtil.DegreesToRadians(Convert.ToSingle(values.Select(x => x.First(y => y.Key == "value").Value).First()));
                                    break;
                            }
                        }

                        nodes.Add(node);
                    }
                }

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