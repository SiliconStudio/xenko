using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Animations
{
    public class AnimationCurveEvaluatorDirectIntGroup : AnimationCurveEvaluatorDirectGroup<int>
    {
        protected unsafe override void ProcessChannel(ref Channel channel, CompressedTimeSpan newTime, IntPtr location)
        {
            SetTime(ref channel, newTime);

            var keyFrames = channel.Curve.KeyFrames;
            var currentIndex = channel.CurrentIndex;

            *(int*)(location + channel.Offset) = keyFrames[currentIndex].Value;
        }
    }
}