// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Updater;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    public partial class ImportModelCommand
    {
        public AnimationRepeatMode AnimationRepeatMode { get; set; }
        public bool AnimationRootMotion { get; set; }

        private unsafe object ExportAnimation(ICommandContext commandContext, ContentManager contentManager)
        {
            // Read from model file
            var modelSkeleton = LoadSkeleton(commandContext, contentManager); // we get model skeleton to compare it to real skeleton we need to map to

            TimeSpan duration;
            var animationClips = LoadAnimation(commandContext, contentManager, out duration);
            AnimationClip animationClip = null;

            if (animationClips.Count > 0)
            {
                animationClip = new AnimationClip();
                animationClip.Duration = duration;

                AnimationClip rootMotionAnimationClip = null;

                // If root motion is explicitely enabled, or if there is no skeleton, try to find root node and apply animation directly on TransformComponent
                if ((AnimationRootMotion || SkeletonUrl == null) && modelSkeleton.Nodes.Length >= 1)
                {
                    // No skeleton, map root node only
                    // TODO: For now, it seems to be located on node 1 in FBX files. Need to check if always the case, and what happens with Assimp
                    var rootNode0 = modelSkeleton.Nodes.Length >= 1 ? modelSkeleton.Nodes[0].Name : null;
                    var rootNode1 = modelSkeleton.Nodes.Length >= 2 ? modelSkeleton.Nodes[1].Name : null;
                    if ((rootNode0 != null && animationClips.TryGetValue(rootNode0, out rootMotionAnimationClip))
                        || (rootNode1 != null && animationClips.TryGetValue(rootNode1, out rootMotionAnimationClip)))
                    {
                        foreach (var channel in rootMotionAnimationClip.Channels)
                        {
                            var curve = rootMotionAnimationClip.Curves[channel.Value.CurveIndex];

                            // Root motion
                            var channelName = channel.Key;
                            if (channelName.StartsWith("Transform."))
                            {
                                animationClip.AddCurve($"[TransformComponent.Key]." + channelName.Replace("Transform.", string.Empty), curve);
                            }

                            // Also apply Camera curves
                            // TODO: Add some other curves?
                            if (channelName.StartsWith("Camera."))
                            {
                                animationClip.AddCurve($"[CameraComponent.Key]." + channelName.Replace("Camera.", string.Empty), curve);
                            }
                        }
                    }
                }

                // Load asset reference skeleton
                if (SkeletonUrl != null)
                {
                    var skeleton = contentManager.Load<Skeleton>(SkeletonUrl);
                    var skeletonMapping = new SkeletonMapping(skeleton, modelSkeleton);

                    // Process missing nodes
                    foreach (var nodeAnimationClipEntry in animationClips)
                    {
                        var nodeName = nodeAnimationClipEntry.Key;
                        var nodeAnimationClip = nodeAnimationClipEntry.Value;
                        var nodeIndex = modelSkeleton.Nodes.IndexOf(x => x.Name == nodeName);

                        // Node doesn't exist in skeleton? skip it
                        if (nodeIndex == -1 || skeletonMapping.SourceToSource[nodeIndex] != nodeIndex)
                            continue;

                        // Skip root motion node (if any)
                        if (nodeAnimationClip == rootMotionAnimationClip)
                            continue;

                        // Find parent node
                        var parentNodeIndex = modelSkeleton.Nodes[nodeIndex].ParentIndex;

                        if (parentNodeIndex != -1 && skeletonMapping.SourceToSource[parentNodeIndex] != parentNodeIndex)
                        {
                            // Some nodes were removed, we need to concat the anim curves
                            var currentNodeIndex = nodeIndex;
                            var nodesToMerge = new List<Tuple<ModelNodeDefinition, AnimationBlender, AnimationClipEvaluator>>();
                            while (currentNodeIndex != -1 && currentNodeIndex != skeletonMapping.SourceToSource[parentNodeIndex])
                            {
                                AnimationClip animationClipToMerge;
                                AnimationClipEvaluator animationClipEvaluator = null;
                                AnimationBlender animationBlender = null;
                                if (animationClips.TryGetValue(modelSkeleton.Nodes[currentNodeIndex].Name, out animationClipToMerge))
                                {
                                    animationBlender = new AnimationBlender();
                                    animationClipEvaluator = animationBlender.CreateEvaluator(animationClipToMerge);
                                }
                                nodesToMerge.Add(Tuple.Create(modelSkeleton.Nodes[currentNodeIndex], animationBlender, animationClipEvaluator));
                                currentNodeIndex = modelSkeleton.Nodes[currentNodeIndex].ParentIndex;
                            }

                            // Put them in proper parent to children order
                            nodesToMerge.Reverse();

                            // Find all key times
                            // TODO: We should detect discontinuities and keep them
                            var animationKeysSet = new HashSet<CompressedTimeSpan>();

                            foreach (var node in nodesToMerge)
                            {
                                foreach (var curve in node.Item3.Clip.Curves)
                                {
                                    foreach (CompressedTimeSpan time in curve.Keys)
                                    {
                                        animationKeysSet.Add(time);
                                    }
                                }
                            }

                            // Sort key times
                            var animationKeys = animationKeysSet.ToList();
                            animationKeys.Sort();

                            var animationOperations = new FastList<AnimationOperation>();

                            var combinedAnimationClip = new AnimationClip();

                            var translationCurve = new AnimationCurve<Vector3>();
                            var rotationCurve = new AnimationCurve<Quaternion>();
                            var scaleCurve = new AnimationCurve<Vector3>();

                            // Evaluate at every key frame
                            foreach (var animationKey in animationKeys)
                            {
                                var matrix = Matrix.Identity;

                                // Evaluate node
                                foreach (var node in nodesToMerge)
                                {
                                    // Get default position
                                    var modelNodeDefinition = node.Item1;

                                    // Compute
                                    AnimationClipResult animationClipResult = null;
                                    animationOperations.Clear();
                                    animationOperations.Add(AnimationOperation.NewPush(node.Item3, animationKey));
                                    node.Item2.Compute(animationOperations, ref animationClipResult);

                                    var updateMemberInfos = new List<UpdateMemberInfo>();
                                    foreach (var channel in animationClipResult.Channels)
                                        updateMemberInfos.Add(new UpdateMemberInfo { Name = channel.PropertyName, DataOffset = channel.Offset });

                                    // TODO: Cache this
                                    var compiledUpdate = UpdateEngine.Compile(typeof(ModelNodeDefinition), updateMemberInfos);

                                    unsafe
                                    {
                                        fixed (byte* data = animationClipResult.Data)
                                            UpdateEngine.Run(modelNodeDefinition, compiledUpdate, (IntPtr)data, null);
                                    }

                                    Matrix localMatrix;
                                    TransformComponent.CreateMatrixTRS(ref modelNodeDefinition.Transform.Position, ref modelNodeDefinition.Transform.Rotation, ref modelNodeDefinition.Transform.Scale,
                                        out localMatrix);
                                    matrix = Matrix.Multiply(localMatrix, matrix);
                                }

                                // Done evaluating, let's decompose matrix
                                TransformTRS transform;
                                matrix.Decompose(out transform.Scale, out transform.Rotation, out transform.Position);

                                // Create a key
                                translationCurve.KeyFrames.Add(new KeyFrameData<Vector3>(animationKey, transform.Position));
                                rotationCurve.KeyFrames.Add(new KeyFrameData<Quaternion>(animationKey, transform.Rotation));
                                scaleCurve.KeyFrames.Add(new KeyFrameData<Vector3>(animationKey, transform.Scale));
                            }

                            combinedAnimationClip.AddCurve($"{nameof(ModelNodeTransformation.Transform)}.{nameof(TransformTRS.Position)}", translationCurve);
                            combinedAnimationClip.AddCurve($"{nameof(ModelNodeTransformation.Transform)}.{nameof(TransformTRS.Rotation)}", rotationCurve);
                            combinedAnimationClip.AddCurve($"{nameof(ModelNodeTransformation.Transform)}.{nameof(TransformTRS.Scale)}", scaleCurve);
                            nodeAnimationClip = combinedAnimationClip;
                        }

                        foreach (var channel in nodeAnimationClip.Channels)
                        {
                            var curve = nodeAnimationClip.Curves[channel.Value.CurveIndex];

                            // TODO: Root motion
                            var channelName = channel.Key;
                            if (channelName.StartsWith("Transform."))
                            {
                                animationClip.AddCurve($"[ModelComponent.Key].Skeleton.NodeTransformations[{skeletonMapping.SourceToTarget[nodeIndex]}]." + channelName, curve);
                            }
                        }
                    }
                }
            }

            if (animationClip == null)
            {
                commandContext.Logger.Info("File {0} has an empty animation.", SourcePath);
            }
            else
            {
                if (animationClip.Duration.Ticks == 0)
                {
                    commandContext.Logger.Warning("File {0} has a 0 tick long animation.", SourcePath);
                }

                // Optimize and set common parameters
                animationClip.RepeatMode = AnimationRepeatMode;
                animationClip.Optimize();
            }
            return animationClip;
        }
    }
}