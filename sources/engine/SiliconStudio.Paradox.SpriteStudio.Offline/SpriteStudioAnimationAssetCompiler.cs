using System;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Animations;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using SiliconStudio.Paradox.SpriteStudio.Runtime;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    internal class SpriteStudioAnimationAssetCompiler : AssetCompilerBase<SpriteStudioAnimationAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath,
            SpriteStudioAnimationAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new SpriteStudioAnimationAssetCommand(urlInStorage, asset)
            };
        }

        /// <summary>
        /// Command used by the build engine to convert the asset
        /// </summary>
        private class SpriteStudioAnimationAssetCommand : AssetCommand<SpriteStudioAnimationAsset>
        {
            public SpriteStudioAnimationAssetCommand(string url, SpriteStudioAnimationAsset asset)
                : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var xmlDoc = XDocument.Load(AssetParameters.Source);
                if (xmlDoc.Root == null) return null;

                var nameSpace = xmlDoc.Root.Name.Namespace;

                var nodes = new List<SpriteStudioNode>();
                var nodesData = new List<SpriteNodeData>();

                int endFrame, fps;
                if (!int.TryParse(xmlDoc.Descendants(nameSpace + "EndFrame").First().Value, out endFrame)) return null;
                if (!int.TryParse(xmlDoc.Descendants(nameSpace + "BaseTickTime").First().Value, out fps)) return null;

                var parts = xmlDoc.Descendants(nameSpace + "Part").ToList();
                foreach (var part in parts)
                {
                    var type = part.Descendants(nameSpace + "Type").First();
                    var name = part.Descendants(nameSpace + "Name").First();
                    if (type.Value == "1")
                    {
                        var node = new SpriteStudioNode { Name = name.Value, Id = -1, ParentId = -2 };
                        nodes.Add(node);
                        nodesData.Add(new SpriteNodeData());
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

                        nodes.Add(node);

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

                        nodesData.Add(nodeData);
                    }
                }

                var assetManager = new AssetManager();

                //Compile the animations
                var animation = new AnimationClip
                {
                    Duration = TimeSpan.FromSeconds((1.0 / fps) * (endFrame + 1)),
                    RepeatMode = AssetParameters.RepeatMode
                };

                for (var i = 0; i < nodesData.Count; i++)
                {
                    var data = nodesData[i];
                    var node = nodes[i];
                    if (data.Data.Count == 0) continue;

                    var posxCurve = new AnimationCurve<float>();
                    animation.AddCurve("posx[" + node.Name + "]", posxCurve);
                    posxCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                    var posyCurve = new AnimationCurve<float>();
                    animation.AddCurve("posy[" + node.Name + "]", posyCurve);
                    posyCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                    var anglCurve = new AnimationCurve<float>();
                    animation.AddCurve("angl[" + node.Name + "]", anglCurve);
                    anglCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                    var prioCurve = new AnimationCurve<float>();
                    animation.AddCurve("prio[" + node.Name + "]", prioCurve);
                    prioCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                    var hideCurve = new AnimationCurve<float>();
                    animation.AddCurve("hide[" + node.Name + "]", hideCurve);
                    hideCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                    if (data.Data.ContainsKey("POSX"))
                        foreach (var nodeData in data.Data["POSX"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToSingle(nodeData["value"]);
                            posxCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("POSY"))
                        foreach (var nodeData in data.Data["POSY"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = -Convert.ToSingle(nodeData["value"]);
                            posyCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("ANGL"))
                        foreach (var nodeData in data.Data["ANGL"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = MathUtil.DegreesToRadians(Convert.ToSingle(nodeData["value"]));
                            anglCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("PRIO"))
                        foreach (var nodeData in data.Data["PRIO"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            prioCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("HIDE"))
                        foreach (var nodeData in data.Data["HIDE"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            hideCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                }
                animation.Optimize();

                assetManager.Save(Url, animation);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}