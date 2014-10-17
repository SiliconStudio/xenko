// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.MicroThreading
{
    public struct MicroThreadYieldAwaiter : INotifyCompletion
    {
        private MicroThread microThread;

        public MicroThreadYieldAwaiter(MicroThread microThread)
        {
            this.microThread = microThread;
        }

        public MicroThreadYieldAwaiter GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get
            {
                lock (microThread.Scheduler.scheduledMicroThreads)
                {
                    return microThread.Scheduler.scheduledMicroThreads.Count == 0;
                }
            }
        }

        public void GetResult()
        {
            // Check Task Result (exception, etc...)
        }

        public void OnCompleted(Action continuation)
        {
            microThread.ScheduleContinuation(ScheduleMode.Last, continuation);
        }
    }
}