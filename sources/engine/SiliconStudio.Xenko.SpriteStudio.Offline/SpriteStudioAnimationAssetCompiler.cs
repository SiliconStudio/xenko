using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.SpriteStudio.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    internal class SpriteStudioAnimationAssetCompiler : AssetCompilerBase<SpriteStudioAnimationAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath,
            SpriteStudioAnimationAsset asset, AssetCompilerResult result)
        {
            var colorSpace = context.GetColorSpace();

            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new SpriteStudioAnimationAssetCommand(urlInStorage, asset, colorSpace)
            };
        }

        /// <summary>
        /// Command used by the build engine to convert the asset
        /// </summary>
        private class SpriteStudioAnimationAssetCommand : AssetCommand<SpriteStudioAnimationAsset>
        {
            private ColorSpace colorSpace;

            public SpriteStudioAnimationAssetCommand(string url, SpriteStudioAnimationAsset asset, ColorSpace colorSpace)
                : base(url, asset)
            {
                this.colorSpace = colorSpace;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var nodes = new List<SpriteStudioNode>();
                string modelName;
                if (!SpriteStudioXmlImport.ParseModel(AssetParameters.Source, nodes, out modelName))
                {
                    return null;
                }

                var anims = new List<SpriteStudioAnim>();
                if (!SpriteStudioXmlImport.ParseAnimations(AssetParameters.Source, anims))
                {
                    return null;
                }

                var assetManager = new ContentManager();

                var anim = anims.First(x => x.Name == AssetParameters.AnimationName);

                //Compile the animations
                var animation = new AnimationClip
                {
                    Duration = TimeSpan.FromSeconds((1.0 / anim.Fps) * anim.FrameCount),
                    RepeatMode = AssetParameters.RepeatMode
                };

                var nodeMapping = nodes.Select((x, i) => new { Name = x.Name, Index = i }).ToDictionary(x => x.Name, x => x.Index);

                foreach (var pair in anim.NodesData)
                {
                    int nodeIndex;
                    if (!nodeMapping.TryGetValue(pair.Key, out nodeIndex))
                        continue;

                    var data = pair.Value;
                    if (data.Data.Count == 0) continue;

                    var keyPrefix = $"[SpriteStudioComponent.Key].Nodes[{nodeIndex}]";

                    if (data.Data.ContainsKey("POSX"))
                    {
                        var posxCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Position)}.{nameof(Vector2.X)}", posxCurve);
                        posxCurve.InterpolationType = data.Data["POSX"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["POSX"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = float.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            posxCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("POSY"))
                    {
                        var posyCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Position)}.{nameof(Vector2.Y)}", posyCurve);
                        posyCurve.InterpolationType = data.Data["POSY"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["POSY"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = float.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            posyCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("ROTZ"))
                    {
                        var anglCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.RotationZ)}", anglCurve);
                        anglCurve.InterpolationType = data.Data["ROTZ"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["ROTZ"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = MathUtil.DegreesToRadians(float.Parse(nodeData["value"], CultureInfo.InvariantCulture));
                            anglCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("PRIO"))
                    {
                        var prioCurve = new AnimationCurve<int>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Priority)}", prioCurve);
                        prioCurve.InterpolationType = AnimationCurveInterpolationType.Constant;

                        foreach (var nodeData in data.Data["PRIO"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = int.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            prioCurve.KeyFrames.Add(new KeyFrameData<int>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("SCLX"))
                    {
                        var scaxCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Scale)}.{nameof(Vector2.X)}", scaxCurve);
                        scaxCurve.InterpolationType = data.Data["SCLX"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["SCLX"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = float.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            scaxCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("SCLY"))
                    {
                        var scayCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Scale)}.{nameof(Vector2.Y)}", scayCurve);
                        scayCurve.InterpolationType = data.Data["SCLY"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["SCLY"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = float.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            scayCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("ALPH"))
                    {
                        var tranCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Transparency)}", tranCurve);
                        tranCurve.InterpolationType = data.Data["ALPH"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["ALPH"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = float.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            tranCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("HIDE"))
                    {
                        var hideCurve = new AnimationCurve<int>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.Hide)}", hideCurve);
                        hideCurve.InterpolationType = AnimationCurveInterpolationType.Constant;

                        foreach (var nodeData in data.Data["HIDE"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = int.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            hideCurve.KeyFrames.Add(new KeyFrameData<int>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("FLPH"))
                    {
                        var flphCurve = new AnimationCurve<int>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.HFlipped)}", flphCurve);
                        flphCurve.InterpolationType = AnimationCurveInterpolationType.Constant;

                        foreach (var nodeData in data.Data["FLPH"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = int.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            flphCurve.KeyFrames.Add(new KeyFrameData<int>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("FLPV"))
                    {
                        var flpvCurve = new AnimationCurve<int>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.VFlipped)}", flpvCurve);
                        flpvCurve.InterpolationType = AnimationCurveInterpolationType.Constant;

                        foreach (var nodeData in data.Data["FLPV"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = int.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            flpvCurve.KeyFrames.Add(new KeyFrameData<int>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("CELL"))
                    {
                        var cellCurve = new AnimationCurve<int>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.SpriteId)}", cellCurve);
                        cellCurve.InterpolationType = AnimationCurveInterpolationType.Constant;

                        foreach (var nodeData in data.Data["CELL"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = int.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            cellCurve.KeyFrames.Add(new KeyFrameData<int>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("COLV"))
                    {
                        var colvCurve = new AnimationCurve<Vector4>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.BlendColor)}", colvCurve);
                        colvCurve.InterpolationType = AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["COLV"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var color = new Color4(Color.FromBgra(int.Parse(nodeData["value"], CultureInfo.InvariantCulture)));
                            color = colorSpace == ColorSpace.Linear ? color.ToLinear() : color;
                            colvCurve.KeyFrames.Add(new KeyFrameData<Vector4>(time, color.ToVector4()));
                        }
                    }

                    if (data.Data.ContainsKey("COLB"))
                    {
                        var colbCurve = new AnimationCurve<int>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.BlendType)}", colbCurve);
                        colbCurve.InterpolationType = AnimationCurveInterpolationType.Constant;

                        foreach (var nodeData in data.Data["COLB"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = int.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            colbCurve.KeyFrames.Add(new KeyFrameData<int>(time, value));
                        }
                    }

                    if (data.Data.ContainsKey("COLF"))
                    {
                        var colfCurve = new AnimationCurve<float>();
                        animation.AddCurve($"{keyPrefix}.{nameof(SpriteStudioNodeState.BlendFactor)}", colfCurve);
                        colfCurve.InterpolationType = data.Data["COLF"].Any(x => x["curve"] != "linear") ? AnimationCurveInterpolationType.Cubic : AnimationCurveInterpolationType.Linear;

                        foreach (var nodeData in data.Data["COLF"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / anim.Fps) * int.Parse(nodeData["time"], CultureInfo.InvariantCulture));
                            var value = float.Parse(nodeData["value"], CultureInfo.InvariantCulture);
                            colfCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
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