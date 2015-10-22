// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Animations
{
    public abstract class AnimationCurveEvaluatorOptimizedGroup<T> : AnimationCurveEvaluatorGroup
    {
        private int animationSortedIndex;
        private AnimationData<T> animationData;
        private CompressedTimeSpan currentTime;
        private FastListStruct<Channel> channels = new FastListStruct<Channel>(8);

        public void Initialize(AnimationData<T> animationData)
        {
            this.animationData = animationData;

            foreach (var channel in animationData.AnimationInitialValues)
            {
                channels.Add(new Channel { InterpolationType = channel.InterpolationType });
            }

            // Setting infinite time means next time a rewind will be performed and initial values will be populated properly
            currentTime = CompressedTimeSpan.MaxValue;
        }

        public void Cleanup()
        {
            animationData = null;
            channels.Clear();
        }

        public void AddChannel(string name, int offset)
        {
            var targetKeys = animationData.TargetKeys;
            for (int i = 0; i < targetKeys.Length; ++i)
            {
                if (targetKeys[i] == name)
                {
                    var channel = channels.Items[i];
                    channel.Offset = offset;
                    channels.Items[i] = channel;
                    break;
                }
            }
        }

        public override void Evaluate(CompressedTimeSpan newTime, IntPtr location)
        {
            if (animationData == null)
                return;

            SetTime(newTime);

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

        protected void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location)
        {
            var startTime = channel.ValueStart.Time;

            // Sampling before start (should not really happen because we add a keyframe at TimeSpan.Zero, but let's keep it in case it changes later.
            if (currentTime <= startTime)
            {
                Utilities.UnsafeWrite(location + channel.Offset, ref channel.ValueStart.Value);
                return;
            }

            var endTime = channel.ValueEnd.Time;

            // Sampling after end
            if (currentTime >= endTime)
            {
                Utilities.UnsafeWrite(location + channel.Offset, ref channel.ValueEnd.Value);
                return;
            }

            float factor = (float)(currentTime.Ticks - startTime.Ticks) / (float)(endTime.Ticks - startTime.Ticks);

            ProcessChannel(ref channel, currentTime, location, factor);
        }

        protected abstract void ProcessChannel(ref Channel channel, CompressedTimeSpan currentTime, IntPtr location, float factor);

        private void SetTime(CompressedTimeSpan timeSpan)
        {
            // TODO: Add jump frames to do faster seeking.
            // If user seek back, need to start from beginning
            if (timeSpan < currentTime)
            {
                // Always start from beginning after a reset
                animationSortedIndex = 0;
                for (int channelIndex = 0; channelIndex < animationData.AnimationInitialValues.Length; ++channelIndex)
                {
                    InitializeAnimation(ref channels.Items[channelIndex], ref animationData.AnimationInitialValues[channelIndex]);
                }
            }

            currentTime = timeSpan;
            var animationSortedValueCount = animationData.AnimationSortedValueCount;
            var animationSortedValues = animationData.AnimationSortedValues;

            if (animationSortedValueCount == 0)
                return;

            // Advance until requested time is reached
            while (animationSortedIndex < animationSortedValueCount
                    && animationSortedValues[animationSortedIndex / AnimationData.AnimationSortedValueBlock][animationSortedIndex % AnimationData.AnimationSortedValueBlock].RequiredTime <= currentTime)
            {
                //int channelIndex = animationSortedValues[animationSortedIndex / animationSortedValueBlock][animationSortedIndex % animationSortedValueBlock].ChannelIndex;
                UpdateAnimation(ref animationSortedValues[animationSortedIndex / AnimationData.AnimationSortedValueBlock][animationSortedIndex % AnimationData.AnimationSortedValueBlock]);
                animationSortedIndex++;
            }

            currentTime = timeSpan;
        }

        private static void InitializeAnimation(ref Channel animationChannel, ref AnimationInitialValues<T> animationValue)
        {
            animationChannel.ValuePrev = animationValue.Value1;
            animationChannel.ValueStart = animationValue.Value1;
            animationChannel.ValueEnd = animationValue.Value1;
            animationChannel.ValueNext = animationValue.Value2;
        }

        private void UpdateAnimation(ref AnimationKeyValuePair<T> animationValue)
        {
            UpdateAnimation(ref channels.Items[animationValue.ChannelIndex], ref animationValue.Value);
        }

        private static void UpdateAnimation(ref Channel animationChannel, ref KeyFrameData<T> animationValue)
        {
            animationChannel.ValuePrev = animationChannel.ValueStart;
            animationChannel.ValueStart = animationChannel.ValueEnd;
            animationChannel.ValueEnd = animationChannel.ValueNext;
            animationChannel.ValueNext = animationValue;
        }

        protected struct Channel
        {
            public int Offset;
            public AnimationCurveInterpolationType InterpolationType;
            public KeyFrameData<T> ValuePrev;
            public KeyFrameData<T> ValueStart;
            public KeyFrameData<T> ValueEnd;
            public KeyFrameData<T> ValueNext;
        }
    }
}