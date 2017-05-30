// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Assets.Materials;

namespace SiliconStudio.Xenko.Assets.Models
{
    [AssetCompiler(typeof(AnimationAsset), typeof(AssetCompilationContext))]
    public class AnimationAssetCompiler : AssetCompilerBase
    {
        public const string RefClipSuffix = "_reference_clip";
        public const string SrcClipSuffix = "_source_clip";

        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(SkeletonAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent);
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (AnimationAsset)assetItem.Asset;
            var assetAbsolutePath = assetItem.FullPath;
            // Get absolute path of asset source on disk
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = GetAbsolutePath(assetItem, asset.Source);
            var extension = assetSource.GetFileExtension();
            var buildStep = new AssetBuildStep(assetItem);

            // Find skeleton asset, if any
            AssetItem skeleton = null;
            if (asset.Skeleton != null)
                skeleton = assetItem.Package.FindAssetFromProxyObject(asset.Skeleton);

            var sourceBuildStep = ImportModelCommand.Create(extension);
            if (sourceBuildStep == null)
            {
                result.Error($"No importer found for model extension '{extension}. The model '{assetSource}' can't be imported.");
                return;
            }

            sourceBuildStep.Mode = ImportModelCommand.ExportMode.Animation;
            sourceBuildStep.SourcePath = assetSource;
            sourceBuildStep.Location = targetUrlInStorage;
            sourceBuildStep.AnimationRepeatMode = asset.RepeatMode;
            sourceBuildStep.AnimationRootMotion = asset.RootMotion;
            if (asset.ClipDuration.Enabled)
            {
                sourceBuildStep.StartFrame = asset.ClipDuration.StartAnimationTime;
                sourceBuildStep.EndFrame = asset.ClipDuration.EndAnimationTime;
            }
            else
            {
                sourceBuildStep.StartFrame = TimeSpan.Zero;
                sourceBuildStep.EndFrame = AnimationAsset.LongestTimeSpan;
            }
            sourceBuildStep.ScaleImport = asset.ScaleImport;
            sourceBuildStep.PivotPosition = asset.PivotPosition;

            if (skeleton != null)
            {
                sourceBuildStep.SkeletonUrl = skeleton.Location;
                // Note: skeleton override values
                sourceBuildStep.ScaleImport = ((SkeletonAsset)skeleton.Asset).ScaleImport;
                sourceBuildStep.PivotPosition = ((SkeletonAsset)skeleton.Asset).PivotPosition;
            }

            if (asset.Type.Type == AnimationAssetTypeEnum.AnimationClip)
            {
                // Import the main animation
                buildStep.Add(sourceBuildStep);
            }
            else if (asset.Type.Type == AnimationAssetTypeEnum.DifferenceClip)
            {
                var diffAnimationAsset = ((DifferenceAnimationAssetType)asset.Type);
                var referenceClip = diffAnimationAsset.BaseSource;
                var rebaseMode = diffAnimationAsset.Mode;

                var baseUrlInStorage = targetUrlInStorage + RefClipSuffix;
                var sourceUrlInStorage = targetUrlInStorage + SrcClipSuffix;

                var baseAssetSource = UPath.Combine(assetDirectory, referenceClip);
                var baseExtension = baseAssetSource.GetFileExtension();

                sourceBuildStep.Location = sourceUrlInStorage;

                var baseBuildStep = ImportModelCommand.Create(extension);
                if (baseBuildStep == null)
                {
                    result.Error($"No importer found for model extension '{baseExtension}. The model '{baseAssetSource}' can't be imported.");
                    return;
                }

                baseBuildStep.Mode = ImportModelCommand.ExportMode.Animation;
                baseBuildStep.SourcePath = baseAssetSource;
                baseBuildStep.Location = baseUrlInStorage;
                baseBuildStep.AnimationRepeatMode = asset.RepeatMode;
                baseBuildStep.AnimationRootMotion = asset.RootMotion;

                if (diffAnimationAsset.ClipDuration.Enabled)
                {
                    baseBuildStep.StartFrame = diffAnimationAsset.ClipDuration.StartAnimationTimeBox;
                    baseBuildStep.EndFrame = diffAnimationAsset.ClipDuration.EndAnimationTimeBox;
                }
                else
                {
                    baseBuildStep.StartFrame = TimeSpan.Zero;
                    baseBuildStep.EndFrame = AnimationAsset.LongestTimeSpan;
                }

                baseBuildStep.ScaleImport = asset.ScaleImport;
                baseBuildStep.PivotPosition = asset.PivotPosition;

                if (skeleton != null)
                {
                    baseBuildStep.SkeletonUrl = skeleton.Location;
                    // Note: skeleton override values
                    baseBuildStep.ScaleImport = ((SkeletonAsset)skeleton.Asset).ScaleImport;
                    baseBuildStep.PivotPosition = ((SkeletonAsset)skeleton.Asset).PivotPosition;
                }

                // Import base and main animation
                buildStep.Add(sourceBuildStep);
                buildStep.Add(baseBuildStep);

                // Wait for both import fbx commands to be completed
                buildStep.Add(new WaitBuildStep());

                // Generate the diff of those two animations
                buildStep.Add(new AdditiveAnimationCommand(targetUrlInStorage, new AdditiveAnimationParameters(baseUrlInStorage, sourceUrlInStorage, rebaseMode), assetItem.Package));
            }
            else
            {
                throw new NotImplementedException("This type of animation asset is not supported yet!");
            }


            result.BuildSteps = buildStep;
        }

        internal class AdditiveAnimationCommand : AssetCommand<AdditiveAnimationParameters>
        {
            public AdditiveAnimationCommand(string url, AdditiveAnimationParameters parameters, Package package) : 
                base(url, parameters, package)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                // Load source and base animations
                var baseAnimation = assetManager.Load<AnimationClip>(Parameters.BaseUrl);
                var sourceAnimation = assetManager.Load<AnimationClip>(Parameters.SourceUrl);

                // Generate diff animation
                var animation = (baseAnimation == null) ? sourceAnimation : SubtractAnimations(baseAnimation, sourceAnimation);

                // Optimize animation
                animation.Optimize();

                // Save diff animation
                assetManager.Save(Url, animation);

                return Task.FromResult(ResultStatus.Successful);
            }

            private AnimationClip SubtractAnimations(AnimationClip baseAnimation, AnimationClip sourceAnimation)
            {
                if (baseAnimation == null) throw new ArgumentNullException("baseAnimation");
                if (sourceAnimation == null) throw new ArgumentNullException("sourceAnimation");

                var animationBlender = new AnimationBlender();

                var baseEvaluator = animationBlender.CreateEvaluator(baseAnimation);
                var sourceEvaluator = animationBlender.CreateEvaluator(sourceAnimation);

                // Create a result animation with same channels
                var resultAnimation = new AnimationClip();
                foreach (var channel in sourceAnimation.Channels)
                {
                    // Create new instance of curve
                    var newCurve = (AnimationCurve)Activator.CreateInstance(typeof(AnimationCurve<>).MakeGenericType(channel.Value.ElementType));

                    // Quaternion curve are linear, others are cubic
                    if (newCurve.ElementType != typeof(Quaternion))
                        newCurve.InterpolationType = AnimationCurveInterpolationType.Cubic;

                    resultAnimation.AddCurve(channel.Key, newCurve);
                }

                var resultEvaluator = animationBlender.CreateEvaluator(resultAnimation);

                var animationOperations = new FastList<AnimationOperation>();

                // Perform animation blending for each frame and upload results in a new animation
                // Note that it does a simple per-frame sampling, so animation discontinuities will be lost.
                // TODO: Framerate is hardcoded at 30 FPS.
                var frameTime = TimeSpan.FromSeconds(1.0f / 30.0f);
                for (var time = TimeSpan.Zero; time < sourceAnimation.Duration + frameTime; time += frameTime)
                {
                    // Last frame, round it to end of animation
                    if (time > sourceAnimation.Duration)
                        time = sourceAnimation.Duration;

                    TimeSpan baseTime;
                    switch (Parameters.Mode)
                    {
                        case AdditiveAnimationBaseMode.FirstFrame:
                            baseTime = TimeSpan.Zero;
                            break;
                        case AdditiveAnimationBaseMode.Animation:
                            baseTime = TimeSpan.FromTicks(time.Ticks % baseAnimation.Duration.Ticks);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Generates result = source - base
                    animationOperations.Clear();
                    animationOperations.Add(AnimationOperation.NewPush(sourceEvaluator, time));
                    animationOperations.Add(AnimationOperation.NewPush(baseEvaluator, baseTime));
                    animationOperations.Add(AnimationOperation.NewBlend(CoreAnimationOperation.Subtract, 1.0f));
                    animationOperations.Add(AnimationOperation.NewPop(resultEvaluator, time));
                    
                    // Compute
                    AnimationClipResult animationClipResult = null;
                    animationBlender.Compute(animationOperations, ref animationClipResult);
                }

                resultAnimation.Duration = sourceAnimation.Duration;
                resultAnimation.RepeatMode = sourceAnimation.RepeatMode;

                return resultAnimation;
            }
        }

        [DataContract]
        public class AdditiveAnimationParameters
        {
            public string BaseUrl;
            public string SourceUrl;
            public AdditiveAnimationBaseMode Mode;
            public int BaseStartFrame;

            public AdditiveAnimationParameters()
            {
            }

            public AdditiveAnimationParameters(string baseUrl, string sourceUrl, AdditiveAnimationBaseMode mode)
            {
                BaseUrl = baseUrl;
                SourceUrl = sourceUrl;
                Mode = mode;
            }
        }
    }
}
