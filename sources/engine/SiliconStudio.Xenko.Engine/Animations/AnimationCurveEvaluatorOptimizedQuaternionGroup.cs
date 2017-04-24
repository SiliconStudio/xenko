// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Animations
{
    public class AnimationCurveEvaluatorOptimizedQuaternionGroup : AnimationCurveEvaluatorOptimizedBlittableGroupBase<Quaternion>
    {
        protected unsafe override void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor)
        {
            if (channel.InterpolationType == AnimationCurveInterpolationType.Cubic)
            {
                Interpolator.Quaternion.Cubic(
                    ref channel.ValuePrev.Value,
                    ref channel.ValueStart.Value,
                    ref channel.ValueEnd.Value,
                    ref channel.ValueNext.Value,
                    factor,
                    out *(Quaternion*)(location + channel.Offset));
            }
            else if (channel.InterpolationType == AnimationCurveInterpolationType.Linear)
            {
                Interpolator.Quaternion.SphericalLinear(
                    ref channel.ValueStart.Value,
                    ref channel.ValueEnd.Value,
                    factor,
                    out *(Quaternion*)(location + channel.Offset));
            }
            else if (channel.InterpolationType == AnimationCurveInterpolationType.Constant)
            {
                *(Quaternion*)(location + channel.Offset) = channel.ValueStart.Value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
