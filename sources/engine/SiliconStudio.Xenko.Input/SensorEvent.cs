// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event indicating a change of a sensor reading
    /// </summary>
    public class SensorEvent : EventArgs
    {
        public IReadOnlyList<float> Values;

        public SensorEvent(IReadOnlyList<float> values)
        {
            this.Values = values;
        }
    }
}