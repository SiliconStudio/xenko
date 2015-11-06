using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Animations
{
    public class AnimationCurveEvaluatorOptimizedBlittableGroup<T> : AnimationCurveEvaluatorOptimizedBlittableGroupBase<T>
    {
        protected override unsafe void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor)
        {
            Interop.CopyInline((void*)(location + channel.Offset), ref channel.ValueStart.Value);
        }
    }
}