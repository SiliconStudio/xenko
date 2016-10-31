// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A direction gesture generated from 2 <see cref="IAxisGesture"/>s (X and Y axis)
    /// </summary>
    [DataContract]
    public class FourWayGesture : InputGesture, IDirectionGesture
    {
        private IAxisGesture y;
        private IAxisGesture x;

        public IAxisGesture X
        {
            get { return x; }
            set
            {
                UpdateChild(x, value);
                x = value;
            }
        }

        public IAxisGesture Y
        {
            get { return y; }
            set
            {
                UpdateChild(y, value);
                y = value;
            }
        }

        /// <summary>
        /// If <c>true</c>, normalizes the direction if it's length is greater than 0
        /// </summary>
        /// <remarks>This still allows the axis to report smaller ranges, for e.g. walk/run.</remarks>
        public bool Normalized { get; set; } = true;

        public Vector2 Direction
        {
            get
            {
                var vec = new Vector2(X?.Axis ?? 0.0f, Y?.Axis ?? 0.0f);
                if (Normalized)
                {
                    float length = vec.Length();
                    if (length > 1.0f)
                        vec /= length;
                }
                return vec;
            }
        }

        public override void Reset()
        {
            x?.Reset();
            y?.Reset();
        }

        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Normalized)}: {Normalized}, {nameof(Direction)}: {Direction}";
        }
    }
}