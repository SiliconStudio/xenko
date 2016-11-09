// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A button gesture generated from an <see cref="IAxisGesture"/>, with a customizable threshold
    /// </summary>
    [DataContract]
    public class AxisButtonGesture : InputGesture, IButtonGesture
    {
        public float Threshold = 0.5f;
        private IAxisGesture axis;

        public IAxisGesture Axis
        {
            get { return axis; }
            set
            {
                UpdateChild(axis, value);
                axis = value;
            }
        }

        [DataMemberIgnore]
        public bool Button => Axis?.Axis > Threshold;

        public override void Reset()
        {
            axis?.Reset();
        }

        protected bool Equals(AxisButtonGesture other)
        {
            return Threshold.Equals(other.Threshold);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AxisButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return Threshold.GetHashCode();
        }
    }
}