// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Xenko.Games.Time
{
    public interface ITimedValue<out T>
    {
        double Time { get; }
        T Value { get; }
    }

    public class ReadOnlyTimedValue<T> : ITimedValue<T>
    {
        public ReadOnlyTimedValue(double time, T value)
        {
            Time = time;
            Value = value;
        }

        public ReadOnlyTimedValue(ITimedValue<T> timedValue)
        {
            Time = timedValue.Time;
            Value = timedValue.Value;
        }

        public double Time { get; private set; }
        public T Value { get; private set; }
    }

    public class TimedValue<T> : ITimedValue<T>
    {
        public TimedValue(double time, T value)
        {
            Time = time;
            Value = value;
        }

        public double Time { get; set; }
        public T Value { get; set; }
    }
}
