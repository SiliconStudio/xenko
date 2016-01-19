// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Animations
{
    public class AnimationProcessor : EntityProcessor<AnimationComponent, AnimationProcessor.AssociatedData>
    {
        private readonly FastList<AnimationOperation> animationOperations = new FastList<AnimationOperation>(2);

        public AnimationProcessor()
        {
            Order = -500;
        }

        protected override AssociatedData GenerateComponentData(Entity entity, AnimationComponent component)
        {
            return new AssociatedData
            {
                AnimationComponent = component,
                ModelComponent = entity.Get<ModelComponent>()
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, AnimationComponent component, AssociatedData associatedData)
        {
            return
                component == associatedData.AnimationComponent &&
                entity.Get<ModelComponent>() == associatedData.ModelComponent;
        }

        protected override void OnEntityComponentAdding(Entity entity, AnimationComponent component, AssociatedData data)
        {
            base.OnEntityComponentAdding(entity, component, data);

            data.AnimationUpdater = new AnimationUpdater();
        }

        protected override void OnEntityComponentRemoved(Entity entity, AnimationComponent component, AssociatedData data)
        {
            base.OnEntityComponentRemoved(entity, component, data);

            // Return AnimationClipEvaluators to pool
            foreach (var playingAnimation in data.AnimationComponent.PlayingAnimations)
            {
                var evaluator = playingAnimation.Evaluator;
                if (evaluator != null)
                {
                    data.AnimationComponent.Blender.ReleaseEvaluator(evaluator);
                    playingAnimation.Evaluator = null;
                }
            }

            // Return AnimationClipResult to pool
            if (data.AnimationClipResult != null)
                data.AnimationComponent.Blender.FreeIntermediateResult(data.AnimationClipResult);
        }

        public override void Draw(RenderContext context)
        {
            var time = context.Time;

            foreach (var entity in ComponentDatas)
            {
                var associatedData = entity.Value;
                var animationUpdater = associatedData.AnimationUpdater;
                var animationComponent = associatedData.AnimationComponent;

                // Advance time for all playing animations with AutoPlay set to on
                foreach (var playingAnimation in animationComponent.PlayingAnimations)
                {
                    if (playingAnimation.Enabled)
                    {
                        switch (playingAnimation.RepeatMode)
                        {
                            case AnimationRepeatMode.PlayOnce:
                                playingAnimation.CurrentTime = TimeSpan.FromTicks(playingAnimation.CurrentTime.Ticks + (long)(time.Elapsed.Ticks * (double)playingAnimation.TimeFactor));
                                if (playingAnimation.CurrentTime > playingAnimation.Clip.Duration)
                                    playingAnimation.CurrentTime = playingAnimation.Clip.Duration;
                                break;
                            case AnimationRepeatMode.LoopInfinite:
                                playingAnimation.CurrentTime = playingAnimation.Clip.Duration == TimeSpan.Zero ? TimeSpan.Zero : TimeSpan.FromTicks((playingAnimation.CurrentTime.Ticks + (long)(time.Elapsed.Ticks * (double)playingAnimation.TimeFactor)) % playingAnimation.Clip.Duration.Ticks);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                // Regenerate animation operations
                animationOperations.Clear();

                float totalWeight = 0.0f;

                for (int index = 0; index < animationComponent.PlayingAnimations.Count; index++)
                {
                    var playingAnimation = animationComponent.PlayingAnimations[index];
                    var animationWeight = playingAnimation.Weight;

                    // Skip animation with 0.0f weight
                    if (animationWeight == 0.0f)
                        continue;

                    // Default behavior for linea blending (it will properly accumulate multiple blending with their cumulative weight)
                    totalWeight += animationWeight;
                    float currentBlend = animationWeight / totalWeight;

                    if (playingAnimation.BlendOperation == AnimationBlendOperation.Add
                        || playingAnimation.BlendOperation == AnimationBlendOperation.Subtract)
                    {
                        // Additive or substractive blending will use the weight as is (and reset total weight with it)
                        currentBlend = animationWeight;
                        totalWeight = animationWeight;
                    }

                    // Create evaluator
                    var evaluator = playingAnimation.Evaluator;
                    if (evaluator == null)
                    {
                        evaluator = animationComponent.Blender.CreateEvaluator(playingAnimation.Clip);
                        playingAnimation.Evaluator = evaluator;
                    }

                    animationOperations.Add(CreatePushOperation(playingAnimation));

                    if (animationOperations.Count >= 2)
                        animationOperations.Add(AnimationOperation.NewBlend(playingAnimation.BlendOperation, currentBlend));
                }

                if (animationOperations.Count > 0)
                {
                    // Animation blending
                    animationComponent.Blender.Compute(animationOperations, ref associatedData.AnimationClipResult);
                    animationComponent.CurrentFrameResult = associatedData.AnimationClipResult;

                    // Update animation data if we have a model component
                    animationUpdater.Update(animationComponent.Entity, associatedData.AnimationClipResult);
                }

                // Update weight animation
                for (int index = 0; index < animationComponent.PlayingAnimations.Count; index++)
                {
                    var playingAnimation = animationComponent.PlayingAnimations[index];
                    bool removeAnimation = false;
                    if (playingAnimation.RemainingTime > TimeSpan.Zero)
                    {
                        playingAnimation.Weight += (playingAnimation.WeightTarget - playingAnimation.Weight)*
                                                   ((float)time.Elapsed.Ticks / playingAnimation.RemainingTime.Ticks);
                        playingAnimation.RemainingTime -= time.Elapsed;
                        if (playingAnimation.RemainingTime <= TimeSpan.Zero)
                        {
                            playingAnimation.Weight = playingAnimation.WeightTarget;

                            // If weight target was 0, removes the animation
                            if (playingAnimation.Weight == 0.0f)
                                removeAnimation = true;
                        }
                    }

                    if (playingAnimation.RepeatMode == AnimationRepeatMode.PlayOnce && playingAnimation.CurrentTime == playingAnimation.Clip.Duration)
                    {
                        removeAnimation = true;
                    }

                    if (removeAnimation)
                    {
                        animationComponent.PlayingAnimations.RemoveAt(index--);

                        var evaluator = playingAnimation.Evaluator;
                        if (evaluator != null)
                        {
                            animationComponent.Blender.ReleaseEvaluator(evaluator);
                            playingAnimation.Evaluator = null;
                        }
                    }
                }
            }
        }

        private AnimationOperation CreatePushOperation(PlayingAnimation playingAnimation)
        {
            return AnimationOperation.NewPush(playingAnimation.Evaluator, playingAnimation.CurrentTime);
        }

        public class AssociatedData
        {
            public AnimationUpdater AnimationUpdater;
            public ModelComponent ModelComponent;
            public AnimationComponent AnimationComponent;
            public AnimationClipResult AnimationClipResult;
        }
    }
}