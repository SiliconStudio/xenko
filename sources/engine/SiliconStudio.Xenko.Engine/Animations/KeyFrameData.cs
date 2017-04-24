// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Globalization;
using System.Runtime.InteropServices;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// A single key frame value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyFrameData<T>
    {
        public KeyFrameData(CompressedTimeSpan time, T value)
        {
            Time = time;
            Value = value;
        }

        public CompressedTimeSpan Time;
        public T Value;

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Time: {0} Value:{1}", Time.Ticks, Value);
        }
    }
}
