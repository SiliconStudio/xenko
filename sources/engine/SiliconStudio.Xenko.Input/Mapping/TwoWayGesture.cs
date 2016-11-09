// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    [DataContract]
    public class TwoWayGesture : ScalableInputGesture, IAxisGesture
    {
        private IButtonGesture positive;
        private IButtonGesture negative;

        public IButtonGesture Positive
        {
            get { return positive; }
            set
            {
                UpdateChild(positive, value);
                positive = value;
            }
        }

        public IButtonGesture Negative
        {
            get { return negative; }
            set
            {
                UpdateChild(negative, value);
                negative = value;
            }
        }

        [DataMemberIgnore]
        public float Axis => GetScaledOutput(Positive?.Button ?? false ? 1.0f : (Negative?.Button ?? false ? -1.0f : 0.0f));

        public override void Reset()
        {
            positive?.Reset();
            negative?.Reset();
        }

        public override string ToString()
        {
            return $"{nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}";
        }
    }
}