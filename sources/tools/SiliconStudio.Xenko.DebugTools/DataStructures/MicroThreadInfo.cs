// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Xenko.Framework.MicroThreading;

namespace SiliconStudio.Xenko.DebugTools.DataStructures
{
    public class MicroThreadInfo
    {
        public long Id { get; set; }
        public MicroThreadState BeginState { get; set; }
        public MicroThreadState EndState { get; set; }
        public double BeginTime { get; set; }
        public double EndTime { get; set; }

        public MicroThreadInfo Duplicate()
        {
            MicroThreadInfo duplicate = new MicroThreadInfo();

            duplicate.Id = Id;
            duplicate.BeginState = BeginState;
            duplicate.EndState = EndState;
            duplicate.BeginTime = BeginTime;
            duplicate.EndTime = EndTime;

            return duplicate;
        }
    }
}
