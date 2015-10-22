// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Animations
{
    public abstract class AnimationCurveEvaluatorDirectGroup<T> : AnimationCurveEvaluatorGroup
    {
        FastListStruct<Channel> channels = new FastListStruct<Channel>(8);

        public void Initialize()
        {
            
        }

        public void Cleanup()
        {
            channels.Clear();
        }

        public void AddChannel(AnimationCurve curve, int offset)
        {
            channels.Add(new Channel { Offset = offset, Curve = (AnimationCurve<T>)curve, InterpolationType = curve.InterpolationType });
        }

        public override void Evaluate(CompressedTimeSpan newTime, IntPtr location)
        {
            var channelCount = channels.Count;
            var channelItems = channels.Items;

            for (int i = 0; i < channelCount; ++i)
            {
                ProcessChannel(ref channelItems[i], newTime, location);
            }
        }

        public override void Evaluate(CompressedTimeSpan newTime, object[] results)
        {
            throw new NotImplementedException();
        }
        
        protected static void SetTime(ref Channel channel, CompressedTimeSpan newTime)
        {
            var currentTime = channel.CurrentTime;
            if (newTime == currentTime)
                return;

            var currentIndex = channel.CurrentIndex;
            var keyFrames = channel.Curve.KeyFrames;

            var keyFramesItems = keyFrames.Items;
            var keyFramesCount = keyFrames.Count;

            if (newTime > currentTime)
            {
                while (currentIndex + 1 < keyFramesCount - 1 && newTime >= keyFramesItems[currentIndex + 1].Time)
                {
                    ++currentIndex;
                }
            }
            else if (newTime <= keyFramesItems[0].Time)
            {
                // Special case: fast rewind to beginning of animation
                currentIndex = 0;
            }
            else // newTime < currentTime
            {
                while (currentIndex - 1 >= 0 && newTime < keyFramesItems[currentIndex].Time)
                {
                    --currentIndex;
                }
            }

            channel.CurrentIndex = currentIndex;
            channel.CurrentTime = newTime;
        }

        protected abstract void ProcessChannel(ref Channel channel, CompressedTimeSpan newTime, IntPtr location);

        protected struct Channel
        {
            public int Offset;
            public AnimationCurveInterpolationType InterpolationType;
            public AnimationCurve<T> Curve;
            public int CurrentIndex;
            public CompressedTimeSpan CurrentTime;
        }
    }
}