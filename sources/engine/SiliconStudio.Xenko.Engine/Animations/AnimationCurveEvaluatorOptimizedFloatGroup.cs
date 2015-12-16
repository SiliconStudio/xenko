// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Animations
{
    public class AnimationCurveEvaluatorOptimizedFloatGroup : AnimationCurveEvaluatorOptimizedBlittableGroupBase<float>
    {
        protected unsafe override void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor)
        {
            if (channel.InterpolationType == AnimationCurveInterpolationType.Cubic)
            {
                *(float*)(location + channel.Offset) = Interpolator.Cubic(
                    channel.ValuePrev.Value,
                    channel.ValueStart.Value,
                    channel.ValueEnd.Value,
                    channel.ValueNext.Value,
                    factor);
            }
            else if (channel.InterpolationType == AnimationCurveInterpolationType.Linear)
            {
                *(float*)(location + channel.Offset) = Interpolator.Linear(
                    channel.ValueStart.Value,
                    channel.ValueEnd.Value,
                    factor);
            }
            else if (channel.InterpolationType == AnimationCurveInterpolationType.Constant)
            {
                *(float*)(location + channel.Offset) = channel.ValueStart.Value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class AnimationCurveEvaluatorOptimizedIntGroup : AnimationCurveEvaluatorOptimizedBlittableGroupBase<int>
    {
        protected unsafe override void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor)
        {
            *(int*)(location + channel.Offset) = channel.ValueStart.Value;
        }
    }
}