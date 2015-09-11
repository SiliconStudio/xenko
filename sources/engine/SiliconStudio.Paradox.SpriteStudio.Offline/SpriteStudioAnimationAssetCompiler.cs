using System;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Animations;

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
                var assetManager = new AssetManager();

                //Compile the animations
                var animation = new AnimationClip
                {
                    Duration = TimeSpan.FromSeconds((1.0 / AssetParameters.Fps) * (AssetParameters.EndFrame + 1)),
                    RepeatMode = AssetParameters.RepeatMode
                };

                for (var i = 0; i < AssetParameters.NodesData.Count; i++)
                {
                    var data = AssetParameters.NodesData[i];
                    var node = AssetParameters.Nodes[i];
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
                            var time = CompressedTimeSpan.FromSeconds((1.0 / AssetParameters.Fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToSingle(nodeData["value"]);
                            posxCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("POSY"))
                        foreach (var nodeData in data.Data["POSY"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / AssetParameters.Fps) * Convert.ToInt32(nodeData["time"]));
                            var value = -Convert.ToSingle(nodeData["value"]);
                            posyCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("ANGL"))
                        foreach (var nodeData in data.Data["ANGL"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / AssetParameters.Fps) * Convert.ToInt32(nodeData["time"]));
                            var value = MathUtil.DegreesToRadians(Convert.ToSingle(nodeData["value"]));
                            anglCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("PRIO"))
                        foreach (var nodeData in data.Data["PRIO"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / AssetParameters.Fps) * Convert.ToInt32(nodeData["time"]));
                            var value = Convert.ToInt32(nodeData["value"]);
                            prioCurve.KeyFrames.Add(new KeyFrameData<float>(time, value));
                        }
                    if (data.Data.ContainsKey("HIDE"))
                        foreach (var nodeData in data.Data["HIDE"])
                        {
                            var time = CompressedTimeSpan.FromSeconds((1.0 / AssetParameters.Fps) * Convert.ToInt32(nodeData["time"]));
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