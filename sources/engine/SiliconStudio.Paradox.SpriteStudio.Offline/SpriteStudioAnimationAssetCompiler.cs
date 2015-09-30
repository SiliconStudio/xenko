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
using System.Diagnostics;
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
                var nodes = new List<SpriteStudioNode>();
                var nodesData = new List<SpriteNodeData>();
                var extraNodes = new List<SpriteStudioNode>();

                int endFrame, fps;
                if (!SpriteStudioXmlImport.Load(AssetParameters.Source, nodes, nodesData, extraNodes, out endFrame, out fps)) return null;

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

                    if (data.Data.ContainsKey("POSX"))
                    {
                        var posxCurve = new AnimationCurve<float>();
                        animation.AddCurve("posx[" + node.Name + "]", posxCurve);
                        posxCurve.InterpolationType = data.Data["POSX"].Any(x => x["curve"] == "2") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["POSX"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToSingle(nodeData["value"]);
                            posxCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("POSY"))
                    {
                        var posyCurve = new AnimationCurve<float>();
                        animation.AddCurve("posy[" + node.Name + "]", posyCurve);
                        posyCurve.InterpolationType = data.Data["POSY"].Any(x => x["curve"] == "2") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["POSY"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = -Convert.ToSingle(nodeData["value"]);
                            posyCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("ANGL"))
                    {
                        var anglCurve = new AnimationCurve<float>();
                        animation.AddCurve("angl[" + node.Name + "]", anglCurve);
                        anglCurve.InterpolationType = data.Data["ANGL"].Any(x => x["curve"] == "2") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["ANGL"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = MathUtil.DegreesToRadians(Convert.ToSingle(nodeData["value"]));
                            anglCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("PRIO"))
                    {
                        var prioCurve = new AnimationCurve<float>();
                        animation.AddCurve("prio[" + node.Name + "]", prioCurve);
                        prioCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["PRIO"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            prioCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("SCAX"))
                    {
                        var scaxCurve = new AnimationCurve<float>();
                        animation.AddCurve("scax[" + node.Name + "]", scaxCurve);
                        scaxCurve.InterpolationType = data.Data["SCAX"].Any(x => x["curve"] == "2") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["SCAX"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToSingle(nodeData["value"]);
                            scaxCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("SCAY"))
                    {
                        var scayCurve = new AnimationCurve<float>();
                        animation.AddCurve("scay[" + node.Name + "]", scayCurve);
                        scayCurve.InterpolationType = data.Data["SCAY"].Any(x => x["curve"] == "2") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["SCAY"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToSingle(nodeData["value"]);
                            scayCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("TRAN"))
                    {
                        var tranCurve = new AnimationCurve<float>();
                        animation.AddCurve("tran[" + node.Name + "]", tranCurve);
                        tranCurve.InterpolationType = data.Data["TRAN"].Any(x => x["curve"] == "2") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["TRAN"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToSingle(nodeData["value"]);
                            tranCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("HIDE"))
                    {
                        var hideCurve = new AnimationCurve<float>();
                        animation.AddCurve("hide[" + node.Name + "]", hideCurve);
                        hideCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["HIDE"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            hideCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("FLPH"))
                    {
                        var flphCurve = new AnimationCurve<float>();
                        animation.AddCurve("flph[" + node.Name + "]", flphCurve);
                        flphCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["FLPH"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            flphCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("FLPV"))
                    {
                        var flpvCurve = new AnimationCurve<float>();
                        animation.AddCurve("flpv[" + node.Name + "]", flpvCurve);
                        flpvCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["FLPV"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            flpvCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("CELL"))
                    {
                        var cellCurve = new AnimationCurve<float>();
                        animation.AddCurve("cell[" + node.Name + "]", cellCurve);
                        cellCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["CELL"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            cellCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }
                }

                animation.Optimize();

                assetManager.Save(Url, animation);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}