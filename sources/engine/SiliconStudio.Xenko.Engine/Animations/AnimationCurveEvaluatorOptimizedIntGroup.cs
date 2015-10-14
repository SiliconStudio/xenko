using System;

namespace SiliconStudio.Paradox.Animations
{
    public class AnimationCurveEvaluatorOptimizedIntGroup : AnimationCurveEvaluatorOptimizedGroup<int>
    {
        protected unsafe override void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor)
        {
            *(int*)(location + channel.Offset) = channel.ValueStart.Value;
        }
    }
}