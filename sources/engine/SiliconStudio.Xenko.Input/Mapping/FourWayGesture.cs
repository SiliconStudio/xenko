// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A direction gesture generated from 2 <see cref="IAxisGesture"/>s (X and Y axis)
    /// </summary>
    [DataContract]
    public class FourWayGesture : InvertibleInputGesture, IDirectionGesture
    {
        private IAxisGesture y;
        private IAxisGesture x;

        /// <summary>
        /// The source for the X-axis component of the generated direction
        /// </summary>
        public IAxisGesture X
        {
            get { return x; }
            set
            {
                UpdateChild(x, value);
                x = value;
            }
        }

        /// <summary>
        /// The source for the Y-axis component of the generated direction
        /// </summary>
        public IAxisGesture Y
        {
            get { return y; }
            set
            {
                UpdateChild(y, value);
                y = value;
            }
        }
        
        [DataMemberIgnore]
        public bool IsRelative
        {
            get { return X?.IsRelative ?? false; }
        }

        [DataMemberIgnore]
        public Vector2 Direction
        {
            get
            {
                var vec = new Vector2(X?.Axis ?? 0.0f, Y?.Axis ?? 0.0f);
                return GetScaledOutput(vec);
            }
        }

        public override void Reset(TimeSpan elapsedTime)
        {
            base.Reset(elapsedTime);
            x?.Reset(elapsedTime);
            y?.Reset(elapsedTime);
        }

        public override string ToString()
        {
            return $"{nameof(Direction)}: {Direction}, {nameof(Inverted)}: {Inverted}, {nameof(IsRelative)}: {IsRelative}";
        }
    }
}