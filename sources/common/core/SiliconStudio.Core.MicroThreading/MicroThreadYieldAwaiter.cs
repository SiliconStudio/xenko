// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.MicroThreading
{
    public struct MicroThreadYieldAwaiter : INotifyCompletion
    {
        private readonly MicroThread microThread;

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
                if (microThread.IsOver)
                    return true;

                lock (microThread.Scheduler.scheduledEntries)
                {
                    return microThread.Scheduler.scheduledEntries.Count == 0;
                }
            }
        }

        public void GetResult()
        {
            microThread.CancellationToken.ThrowIfCancellationRequested();
        }

        public void OnCompleted(Action continuation)
        {
            microThread.ScheduleContinuation(ScheduleMode.Last, continuation);
        }
    }
}