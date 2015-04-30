// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Animations
{
    [DataContract(Inherited = true)]
    public class AnimationData
    {
        public const int AnimationSortedValueBlock = 4096;

        public int AnimationSortedValueCount { get; set; }
        public string[] TargetKeys { get; set; }
    }

    public class AnimationData<T> : AnimationData
    {
        public AnimationInitialValues<T>[] AnimationInitialValues { get; set; }
        public AnimationKeyValuePair<T>[][] AnimationSortedValues { get; set; }

        public TimeSpan Duration
        {
            get { return AnimationSortedValueCount == 0 ? TimeSpan.FromSeconds(1) : AnimationSortedValues[(AnimationSortedValueCount - 1) / AnimationSortedValueBlock][(AnimationSortedValueCount - 1) % AnimationSortedValueBlock].Value.Time; }
        }
        
        public static AnimationData<T> FromAnimationChannels(IDictionary<string, AnimationCurve<T>> animationChannelsByName)
        {
            var result = new AnimationData<T>();

            // Build target object and target properties lists
            var animationChannelsKeyValuePair = animationChannelsByName.ToList();
            var animationChannels = animationChannelsKeyValuePair.Select(x => x.Value).ToList();
            result.TargetKeys = animationChannelsKeyValuePair.Select(x => x.Key).ToArray();

            // Complexity _might_ be better by inserting directly in order instead of sorting later.
            var animationValues = new List<AnimationKeyValuePair<T>>[animationChannels.Count];
            result.AnimationInitialValues = new AnimationInitialValues<T>[animationChannels.Count];
            for (int channelIndex = 0; channelIndex < animationChannels.Count; ++channelIndex)
            {
                var channel = animationChannels[channelIndex];
                var animationChannelValues = animationValues[channelIndex] = new List<AnimationKeyValuePair<T>>();
                if (channel.KeyFrames.Count > 0)
                {
                    // Copy first two keys for when user start from beginning
                    result.AnimationInitialValues[channelIndex].InterpolationType = channel.InterpolationType;
                    result.AnimationInitialValues[channelIndex].Value1 = channel.KeyFrames[0];
                    result.AnimationInitialValues[channelIndex].Value2 = channel.KeyFrames[channel.KeyFrames.Count > 1 ? 1 : 0];

                    // Copy remaining keys for playback
                    for (int keyIndex = 2; keyIndex < channel.KeyFrames.Count; ++keyIndex)
                    {
                        // We need animation values two keys in advance
                        var requiredTime = channel.KeyFrames[keyIndex - 2].Time;

                        animationChannelValues.Add(new AnimationKeyValuePair<T> { ChannelIndex = channelIndex, RequiredTime = requiredTime, Value = channel.KeyFrames[keyIndex] });

                        // Add last frame again so that we have ValueNext == ValueEnd at end of curve
                        if (keyIndex == channel.KeyFrames.Count - 1)
                        {
                            requiredTime = channel.KeyFrames[keyIndex - 1].Time;    // important should not be "keyIndex - 2" or last frame will be skipped by update (two updates in a row)
                            animationChannelValues.Add(new AnimationKeyValuePair<T> { ChannelIndex = channelIndex, RequiredTime = requiredTime, Value = channel.KeyFrames[keyIndex] });
                        }
                    }
                }
            }

            // Gather all channel values in a single sorted array.
            // Since each channel values is already sorted, we can just merge them preserving sort order.
            // It is equivalent to:
            //  var animationConcatValues = Concat(animationValues);
            //  animationSortedValues = animationConcatValues.OrderBy(x => x.RequiredTime).ToArray();
            int animationValueCount = 0;

            // Setup and counting
            var animationChannelByNextTime = new MultiValueSortedDictionary<CompressedTimeSpan, KeyValuePair<int, int>>();
            for (int channelIndex = 0; channelIndex < animationChannels.Count; ++channelIndex)
            {
                var animationChannelValues = animationValues[channelIndex];
                animationValueCount += animationChannelValues.Count;
                if (animationChannelValues.Count > 0)
                    animationChannelByNextTime.Add(animationChannelValues[0].RequiredTime, new KeyValuePair<int, int>(channelIndex, 0));
            }

            // Initialize arrays
            result.AnimationSortedValueCount = animationValueCount;
            var animationSortedValues = new AnimationKeyValuePair<T>[(animationValueCount + AnimationSortedValueBlock - 1) / AnimationSortedValueBlock][];
            result.AnimationSortedValues = animationSortedValues;
            for (int i = 0; i < animationSortedValues.Length; ++i)
            {
                var remainingValueCount = animationValueCount - i * AnimationSortedValueBlock;
                animationSortedValues[i] = new AnimationKeyValuePair<T>[Math.Min(AnimationSortedValueBlock, remainingValueCount)];
            }

            // Fill with sorted values
            animationValueCount = 0;
            while (animationChannelByNextTime.Count > 0)
            {
                var firstItem = animationChannelByNextTime.First();
                animationSortedValues[animationValueCount / AnimationSortedValueBlock][animationValueCount % AnimationSortedValueBlock] = animationValues[firstItem.Value.Key][firstItem.Value.Value];
                animationValueCount++;
                animationChannelByNextTime.Remove(firstItem);

                // Add next item for further processing (if any)
                if (firstItem.Value.Value + 1 < animationValues[firstItem.Value.Key].Count)
                    animationChannelByNextTime.Add(animationValues[firstItem.Value.Key][firstItem.Value.Value + 1].RequiredTime, new KeyValuePair<int, int>(firstItem.Value.Key, firstItem.Value.Value + 1));
            }

            return result;
        }
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationTargetProperty
    {
        public string Name { get; set; }
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationKeyValuePair<T>
    {
        // 4 highest bit specifies format:
        // - 0: float
        // - 1: Vector3
        // - 2: Quaternion
        public int ChannelIndex;
        public CompressedTimeSpan RequiredTime;
        public KeyFrameData<T> Value;
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationInitialValues<T>
    {
        public AnimationCurveInterpolationType InterpolationType;
        public KeyFrameData<T> Value1;
        public KeyFrameData<T> Value2;
    }
}