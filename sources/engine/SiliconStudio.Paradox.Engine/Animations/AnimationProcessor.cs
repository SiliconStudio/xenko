// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Animations
{
    public class AnimationProcessor : EntityProcessor<AnimationProcessor.AssociatedData>
    {
        private FastList<AnimationOperation> animationOperations = new FastList<AnimationOperation>(2);


        public AnimationProcessor()
            : base(new PropertyKey[] { ModelComponent.Key, AnimationComponent.Key })
        {
            Order = -500;
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData { ModelComponent = entity.Get(ModelComponent.Key), AnimationComponent = entity.Get(AnimationComponent.Key) };
        }

        protected override void OnEntityAdding(Entity entity, AssociatedData data)
        {
            base.OnEntityAdding(entity, data);

            data.MeshAnimationUpdater = new MeshAnimationUpdater();
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            base.OnEntityRemoved(entity, data);

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

            foreach (var entity in enabledEntities)
            {
                var associatedData = entity.Value;
                var meshAnimation = associatedData.MeshAnimationUpdater;
                var animationComponent = associatedData.AnimationComponent;

                // Advance time for all playing animations with AutoPlay set to on
                foreach (var playingAnimation in animationComponent.PlayingAnimations)
                {
                    if (playingAnimation.IsPlaying)
                    {
                        switch (playingAnimation.RepeatMode)
                        {
                            case AnimationRepeatMode.PlayOnce:
                                playingAnimation.CurrentTime = TimeSpan.FromTicks(playingAnimation.CurrentTime.Ticks + (long)(time.Elapsed.Ticks * (double)playingAnimation.TimeFactor));
                                if (playingAnimation.CurrentTime > playingAnimation.Clip.Duration)
                                    playingAnimation.CurrentTime = playingAnimation.Clip.Duration;
                                break;
                            case AnimationRepeatMode.LoopInfinite:
                                playingAnimation.CurrentTime = TimeSpan.FromTicks((playingAnimation.CurrentTime.Ticks + (long)(time.Elapsed.Ticks * (double)playingAnimation.TimeFactor)) % playingAnimation.Clip.Duration.Ticks);
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

                var modelViewHierarchy = associatedData.ModelComponent.ModelViewHierarchy;

                if (animationOperations.Count > 0)
                {
                    // Animation blending
                    animationComponent.Blender.Compute(animationOperations, ref associatedData.AnimationClipResult);

                    // Update animation data
                    if (modelViewHierarchy != null)
                        meshAnimation.Update(modelViewHierarchy, associatedData.AnimationClipResult);
                }
                else
                {
                    // If nothing is playing, reset to bind pose
                    if (modelViewHierarchy != null)
                        modelViewHierarchy.ResetInitialValues();
                }

                // Update weight animation
                for (int index = 0; index < animationComponent.PlayingAnimations.Count; index++)
                {
                    var playingAnimation = animationComponent.PlayingAnimations[index];
                    if (playingAnimation.RemainingTime > TimeSpan.Zero)
                    {
                        playingAnimation.Weight += (playingAnimation.WeightTarget - playingAnimation.Weight)*
                                                   ((float)time.Elapsed.Ticks / playingAnimation.RemainingTime.Ticks);
                        playingAnimation.RemainingTime -= time.Elapsed;
                        if (playingAnimation.RemainingTime <= TimeSpan.Zero)
                        {
                            playingAnimation.Weight = playingAnimation.WeightTarget;
                        }
                    }
                    else if (playingAnimation.Weight / totalWeight <= 0.01f)
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
            public MeshAnimationUpdater MeshAnimationUpdater;
            public ModelComponent ModelComponent;
            public AnimationComponent AnimationComponent;
            public AnimationClipResult AnimationClipResult;
        }
    }
}