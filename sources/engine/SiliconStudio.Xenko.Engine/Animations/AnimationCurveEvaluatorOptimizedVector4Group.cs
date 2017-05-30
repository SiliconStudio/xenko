// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Animations
{
    public class AnimationCurveEvaluatorOptimizedVector4Group : AnimationCurveEvaluatorOptimizedBlittableGroupBase<Vector4>
    {
        protected unsafe override void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor)
        {
            if (channel.InterpolationType == AnimationCurveInterpolationType.Cubic)
            {
                Interpolator.Vector4.Cubic(
                    ref channel.ValuePrev.Value,
                    ref channel.ValueStart.Value,
                    ref channel.ValueEnd.Value,
                    ref channel.ValueNext.Value,
                    factor,
                    out *(Vector4*)(location + channel.Offset));
            }
            else if (channel.InterpolationType == AnimationCurveInterpolationType.Linear)
            {
                Interpolator.Vector4.Linear(
                    ref channel.ValueStart.Value,
                    ref channel.ValueEnd.Value,
                    factor,
                    out *(Vector4*)(location + channel.Offset));
            }
            else if (channel.InterpolationType == AnimationCurveInterpolationType.Constant)
            {
                *(Vector4*)(location + channel.Offset) = channel.ValueStart.Value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
