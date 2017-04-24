// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// A single animation operation (push or blend).
    /// </summary>
    public struct AnimationOperation
    {
        public AnimationOperationType Type;

        // Blend parameters
        public CoreAnimationOperation CoreBlendOperation;
        public float BlendFactor;

        // Push parameters
        public AnimationClipEvaluator Evaluator;
        public TimeSpan Time;

        /// <summary>
        /// Creates a new animation push operation.
        /// </summary>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        public static AnimationOperation NewPush(AnimationClipEvaluator evaluator)
        {
            return new AnimationOperation { Type = AnimationOperationType.Push, Evaluator = evaluator, Time = TimeSpan.Zero };
        }

        /// <summary>
        /// Creates a new animation push operation.
        /// </summary>
        /// <param name="evaluator">The evaluator.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public static AnimationOperation NewPush(AnimationClipEvaluator evaluator, TimeSpan time)
        {
            return new AnimationOperation { Type = AnimationOperationType.Push, Evaluator = evaluator, Time = time };
        }

        /// <summary>
        /// Creates a new animation pop operation.
        /// </summary>
        /// <param name="evaluator">The evaluator.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public static AnimationOperation NewPop(AnimationClipEvaluator evaluator, TimeSpan time)
        {
            return new AnimationOperation { Type = AnimationOperationType.Pop, Evaluator = evaluator, Time = time };
        }

        /// <summary>
        /// Creates a new animation blend operation.
        /// </summary>
        /// <param name="operation">The blend operation.</param>
        /// <param name="blendFactor">The blend factor.</param>
        /// <returns></returns>
        public static AnimationOperation NewBlend(CoreAnimationOperation operation, float blendFactor)
        {
            return new AnimationOperation { Type = AnimationOperationType.Blend, CoreBlendOperation = operation, BlendFactor = blendFactor };
        }
    }
}
