using System;

namespace SiliconStudio.Core.MicroThreading
{
    /// <summary>
    /// Either a microthread or an action with priority.
    /// </summary>
    internal struct SchedulerEntry : IComparable<SchedulerEntry>
    {
        public Action Action;
        public MicroThread MicroThread;
        public long Priority;
        public long SchedulerCounter;
        public object Token;

        public SchedulerEntry(MicroThread microThread) : this()
        {
            MicroThread = microThread;
        }

        public SchedulerEntry(Action action, long priority) : this()
        {
            Action = action;
            Priority = priority;
        }

        public int CompareTo(SchedulerEntry other)
        {
            var priorityDiff = Priority.CompareTo(other.Priority);
            if (priorityDiff != 0)
                return priorityDiff;

            return SchedulerCounter.CompareTo(other.SchedulerCounter);
        }
    }
}