// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.Diagnostics
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ProfilingCustomValue
    {
        [FieldOffset(0)]
        public int IntValue;

        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public long LongValue;

        [FieldOffset(0)]
        public double DoubleValue;

        [FieldOffset(8)]
        public Type ValueType;

        public static implicit operator ProfilingCustomValue(int value)
        {
            return new ProfilingCustomValue { IntValue = value, ValueType = typeof(int) };
        }

        public static implicit operator ProfilingCustomValue(float value)
        {
            return new ProfilingCustomValue { FloatValue = value, ValueType = typeof(float) };
        }

        public static implicit operator ProfilingCustomValue(long value)
        {
            return new ProfilingCustomValue { LongValue = value, ValueType = typeof(long) };
        }

        public static implicit operator ProfilingCustomValue(double value)
        {
            return new ProfilingCustomValue { DoubleValue = value, ValueType = typeof(double) };
        }
    }
}
