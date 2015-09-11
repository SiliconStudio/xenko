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
            var xmlDoc = XDocument.Load(rawAssetPath);
            if (xmlDoc.Root == null) return null;

            var outputAssets = new List<AssetItem>();

            var sheet = new SpriteStudioSheetAsset();
            var anim = new SpriteStudioAnimationAsset();

            var nameSpace = xmlDoc.Root.Name.Namespace;

            if (!int.TryParse(xmlDoc.Descendants(nameSpace + "EndFrame").First().Value, out anim.EndFrame)) return null;
            if (!int.TryParse(xmlDoc.Descendants(nameSpace + "BaseTickTime").First().Value, out anim.Fps)) return null;

            var images = xmlDoc.Descendants(nameSpace + "Image").Select(x => UPath.Combine(rawAssetPath.GetFullDirectory(), new UFile(x.Attribute("Path").Value))).ToList();
            foreach (var image in images)
            {
                sheet.Textures.Add(image);
            }

            var parts = xmlDoc.Descendants(nameSpace + "Part").ToList();
            foreach (var part in parts)
            {
                var type = part.Descendants(nameSpace + "Type").First();
                var name = part.Descendants(nameSpace + "Name").First();
                if (type.Value == "1")
                {
                    var node = new SpriteStudioNode { Name = name.Value, Id = -1, ParentId = -2 };
                    sheet.Nodes.Add(node);
                    anim.Nodes.Add(node);
                    anim.NodesData.Add(new SpriteNodeData());
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

                    sheet.Nodes.Add(node);
                    anim.Nodes.Add(node);

                    var nodeData = new SpriteNodeData();

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

                        nodeData.Data.Add(tag, values);
                    }

                    anim.NodesData.Add(nodeData);
                }
            }

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