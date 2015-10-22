// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
