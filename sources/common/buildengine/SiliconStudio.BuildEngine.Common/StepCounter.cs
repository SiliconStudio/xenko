// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.BuildEngine
{
    public class StepCounter
    {
        private readonly int[] stepResults;
        public int Total { get; private set; }

        public StepCounter()
        {
            stepResults = new int[Enum.GetValues(typeof(ResultStatus)).Length];
        }

        public void AddStepResult(ResultStatus result)
        {
            lock (stepResults)
            {
                ++Total;
                ++stepResults[(int)result];
            }
        }

        public int Get(ResultStatus result)
        {
            lock (stepResults)
            {
                return stepResults[(int)result];
            }
        }

        public void Clear()
        {
            lock (stepResults)
            {
                Total = 0;
                foreach (var value in Enum.GetValues(typeof(ResultStatus)))
                    stepResults[(int)value] = 0;
            }
        }
    }
}
