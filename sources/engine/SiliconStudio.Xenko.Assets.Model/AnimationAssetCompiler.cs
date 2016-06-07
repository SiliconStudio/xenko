// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Animations;

namespace SiliconStudio.Xenko.Assets.Model
{
    public class AnimationAssetCompiler : AssetCompilerBase<AnimationAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, AnimationAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;

            // Get absolute path of asset source on disk
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = GetAbsolutePath(assetAbsolutePath, asset.Source);
            var extension = assetSource.GetFileExtension();
            var buildStep = new AssetBuildStep(AssetItem);

            // Find skeleton asset, if any
            AssetItem skeleton = null;
            if (asset.Skeleton != null)
                skeleton = AssetItem.Package.FindAssetFromAttachedReference(asset.Skeleton);

            var sourceBuildStep = ImportModelCommand.Create(extension);
            if (sourceBuildStep == null)
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
                return;
            }

            sourceBuildStep.Mode = ImportModelCommand.ExportMode.Animation;
            sourceBuildStep.SourcePath = assetSource;
            sourceBuildStep.Location = urlInStorage;
            sourceBuildStep.AnimationRepeatMode = asset.RepeatMode;
            sourceBuildStep.AnimationRootMotion = asset.RootMotion;
            sourceBuildStep.ScaleImport = asset.ScaleImport;
            sourceBuildStep.SkeletonUrl = skeleton?.Location;

            var additiveAnimationAsset = asset as AdditiveAnimationAsset;
            if (additiveAnimationAsset != null)
            {
                var baseUrlInStorage = urlInStorage + "_animation_base";
                var sourceUrlInStorage = urlInStorage + "_animation_source";

                var baseAssetSource = UPath.Combine(assetDirectory, additiveAnimationAsset.BaseSource);
                var baseExtension = baseAssetSource.GetFileExtension();

                sourceBuildStep.Location = sourceUrlInStorage;

                var baseBuildStep = ImportModelCommand.Create(extension);
                if (baseBuildStep == null)
                {
                    result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", baseExtension, baseAssetSource);
                    return;
                }

                baseBuildStep.Mode = ImportModelCommand.ExportMode.Animation;
                baseBuildStep.SourcePath = baseAssetSource;
                baseBuildStep.Location = baseUrlInStorage;
                baseBuildStep.AnimationRepeatMode = asset.RepeatMode;
                baseBuildStep.AnimationRootMotion = asset.RootMotion;
                baseBuildStep.ScaleImport = asset.ScaleImport;
                baseBuildStep.SkeletonUrl = skeleton?.Location;

                // Import base and main animation
                buildStep.Add(sourceBuildStep);
                buildStep.Add(baseBuildStep);

                // Wait for both import fbx commands to be completed
                buildStep.Add(new WaitBuildStep());

                // Generate the diff of those two animations
                buildStep.Add(new AdditiveAnimationCommand(urlInStorage, new AdditiveAnimationParameters(baseUrlInStorage, sourceUrlInStorage, additiveAnimationAsset.Mode)));
            }
            else
            {
                // Import the main animation
                buildStep.Add(sourceBuildStep);
            }

            result.BuildSteps = buildStep;
        }

        internal class AdditiveAnimationCommand : AssetCommand<AdditiveAnimationParameters>
        {
            public AdditiveAnimationCommand(string url, AdditiveAnimationParameters assetParameters) : base(url, assetParameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                // Load source and base animations
                var baseAnimation = assetManager.Load<AnimationClip>(AssetParameters.BaseUrl);
                var sourceAnimation = assetManager.Load<AnimationClip>(AssetParameters.SourceUrl);

                // Generate diff animation
                var animation = SubtractAnimations(baseAnimation, sourceAnimation);
                
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
                    switch (AssetParameters.Mode)
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
                    animationOperations.Add(AnimationOperation.NewBlend(AnimationBlendOperation.Subtract, 1.0f));
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
