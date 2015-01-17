// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core.MicroThreading
{
    /// <summary>
    /// Represents an execution context managed by a <see cref="Scheduler"/>, that can cooperatively yield execution to another <see cref="MicroThread"/> at any point (usually using async calls).
    /// </summary>
    public class MicroThread : IComparable<MicroThread>
    {
        internal ProfilingKey ProfilingKey;

        /// <summary>
        /// Gets the attached properties to this component.
        /// </summary>
        public PropertyContainer Tags;

        private static long globalCounterId;

        // Counter that will be used to have a "stable" microthread scheduling (first added is first scheduled)
        private int priority;
        private long schedulerCounter;

        private int state;
        internal PriorityQueueNode<MicroThread> ScheduledLinkedListNode;
        internal LinkedListNode<MicroThread> AllLinkedListNode; // Also used as lock for "CompletionTask"
        internal MicroThreadCallbackList Callbacks;
        internal SynchronizationContext SynchronizationContext;

        public MicroThread(Scheduler scheduler, MicroThreadFlags flags = MicroThreadFlags.None)
        {
            Id = Interlocked.Increment(ref globalCounterId);
            Scheduler = scheduler;
            ScheduledLinkedListNode = new PriorityQueueNode<MicroThread>(this);
            AllLinkedListNode = new LinkedListNode<MicroThread>(this);
            ScheduleMode = ScheduleMode.Last;
            Flags = flags;
            Tags = new PropertyContainer(this);
        }

        /// <summary>
        /// Gets or sets the priority of this <see cref="MicroThread"/>.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        /// <summary>
        /// Gets the id of this <see cref="MicroThread"/>.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public long Id { get; private set; }

        /// <summary>
        /// Gets or sets the name of this <see cref="MicroThread"/>.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the scheduler associated with this <see cref="MicroThread"/>.
        /// </summary>
        /// <value>The scheduler associated with this <see cref="MicroThread"/>.</value>
        public Scheduler Scheduler { get; private set; }

        /// <summary>
        /// Gets the state of this <see cref="MicroThread"/>.
        /// </summary>
        /// <value>The state of this <see cref="MicroThread"/>.</value>
        public MicroThreadState State { get { return (MicroThreadState)state; } internal set { state = (int)value; } }

        /// <summary>
        /// Gets the exception that was thrown by this <see cref="MicroThread"/>.
        /// </summary>
        /// It could come from either internally, or from <see cref="RaiseException"/> if it was successfully processed.
        /// <value>The exception.</value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the <see cref="MicroThread"/> flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public MicroThreadFlags Flags { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="MicroThread"/> scheduling mode.
        /// </summary>
        /// <value>
        /// The scheduling mode.
        /// </value>
        public ScheduleMode ScheduleMode { get; set; }

        /// <summary>
        /// Gets or sets the exception to raise.
        /// </summary>
        /// <value>The exception to raise.</value>
        internal Exception ExceptionToRaise { get; set; }

        /// <summary>
        /// Gets or sets the task that will be executed upon completion (used internally for <see cref="Scheduler.WhenAll"/>)
        /// </summary>
        internal TaskCompletionSource<int> CompletionTask { get; set; }

        /// <summary>
        /// Indicates whether the MicroThread is terminated or not, either in Completed, Cancelled or Failed status.
        /// </summary>
        public bool IsOver
        {
            get
            {
                return
                    State == MicroThreadState.Completed ||
                    State == MicroThreadState.Cancelled ||
                    State == MicroThreadState.Failed;
            }
        }

        /// <summary>
        /// Gets the current micro thread (self).
        /// </summary>
        /// <value>The current micro thread (self).</value>
        public static MicroThread Current
        {
            get { return Scheduler.CurrentMicroThread; }
        }

        public int CompareTo(MicroThread other)
        {
            var priorityDiff = priority.CompareTo(other.priority);
            if (priorityDiff != 0)
                return priorityDiff;

            return schedulerCounter.CompareTo(other.schedulerCounter);
        }

        public void Migrate(Scheduler scheduler)
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts this <see cref="MicroThread"/> with the specified function.
        /// </summary>
        /// <param name="microThreadFunction">The micro thread function.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="scheduleMode">The schedule mode.</param>
        /// <exception cref="System.InvalidOperationException">MicroThread was already started before.</exception>
        public void Start(Func<Task> microThreadFunction, ScheduleMode scheduleMode = ScheduleMode.Last)
        {
            ProfilingKey = new ProfilingKey("MicroThread " + microThreadFunction.Target);

            // TODO: Interlocked compare exchange?
            if (Interlocked.CompareExchange(ref state, (int)MicroThreadState.Starting, (int)MicroThreadState.None) != (int)MicroThreadState.None)
                throw new InvalidOperationException("MicroThread was already started before.");

            Action wrappedMicroThreadFunction = async () =>
            {
                try
                {
                    State = MicroThreadState.Running;

                    await microThreadFunction();

                    if (State != MicroThreadState.Running)
                        throw new InvalidOperationException("MicroThread completed in an invalid state.");
                    State = MicroThreadState.Completed;
                }
                catch (Exception e)
                {
                    Scheduler.Log.Error("Unexpected exception while executing a micro-thread. Reason: {0}", new object[] {e});
                    SetException(e);
                }
                finally
                {
                    lock (Scheduler.allMicroThreads)
                    {
                        Scheduler.allMicroThreads.Remove(AllLinkedListNode);
                    }
                }
            };

            Action callback = () =>
            {
                SynchronizationContext = new MicroThreadSynchronizationContext(this);
                SynchronizationContext.SetSynchronizationContext(SynchronizationContext);

                wrappedMicroThreadFunction();
            };

            lock (Scheduler.allMicroThreads)
            {
                Scheduler.allMicroThreads.AddLast(AllLinkedListNode);
            }

            ScheduleContinuation(scheduleMode, callback);
        }

        /// <summary>
        /// Yields to this <see cref="MicroThread"/>.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task Run()
        {
            Reschedule(ScheduleMode.First);
            var currentScheduler = Scheduler.Current;
            if (currentScheduler == Scheduler)
                await Scheduler.Yield();
        }

        /// <summary>
        /// Raises an exception from within the <see cref="MicroThread"/>.
        /// </summary>
        /// As an exception can only be raised cooperatively, there is no guarantee it will actually happen or when it will happen.
        /// The scheduler usually checks for them before and after a continuation is running.
        /// Only one Exception is currently recorded.
        /// <param name="e">The exception.</param>
        public void RaiseException(Exception e)
        {
            if (ExceptionToRaise == null)
                ExceptionToRaise = e;
        }

        internal void SetException(Exception exception)
        {
            Exception = exception;

            // Depending on if exception was raised from outside or inside, set appropriate state
            State = (exception == ExceptionToRaise) ? MicroThreadState.Cancelled : MicroThreadState.Failed;
        }

        internal void Reschedule(ScheduleMode scheduleMode)
        {
            lock (Scheduler.scheduledMicroThreads)
            {
                if (ScheduledLinkedListNode.Index != -1)
                {
                    Scheduler.scheduledMicroThreads.Remove(ScheduledLinkedListNode);

                    Schedule(scheduleMode);
                }
            }
        }

        internal void ScheduleContinuation(ScheduleMode scheduleMode, SendOrPostCallback callback, object callbackState)
        {
            Debug.Assert(callback != null);
            lock (Scheduler.scheduledMicroThreads)
            {
                var node = NewCallback();
                node.SendOrPostCallback = callback;
                node.CallbackState = callbackState;

                if (ScheduledLinkedListNode.Index == -1)
                    Schedule(scheduleMode);
            }
        }

        internal void ScheduleContinuation(ScheduleMode scheduleMode, Action callback)
        {
            Debug.Assert(callback != null);
            lock (Scheduler.scheduledMicroThreads)
            {
                var node = NewCallback();
                node.MicroThreadAction = callback;

                if (ScheduledLinkedListNode.Index == -1)
                    Schedule(scheduleMode);
            }
        }

        private void Schedule(ScheduleMode scheduleMode)
        {
            var nextCounter = Scheduler.SchedulerCounter++;
            if (scheduleMode == ScheduleMode.First)
                nextCounter = -nextCounter;

            schedulerCounter = nextCounter;

            Scheduler.scheduledMicroThreads.Enqueue(ScheduledLinkedListNode);
        }

        internal void ThrowIfExceptionRequest()
        {
            if (ExceptionToRaise != null)
                throw ExceptionToRaise;
        }

        private MicroThreadCallbackNode NewCallback()
        {
            MicroThreadCallbackNode node;
            var pool = Scheduler.callbackNodePool;

            if (Scheduler.callbackNodePool.Count > 0)
            {
                var index = pool.Count - 1;
                node = pool[index];
                pool.RemoveAt(index);
            }
            else
            {
                node = new MicroThreadCallbackNode();
            }

            Callbacks.Add(node);

            return node;
        }
    }

    internal class MicroThreadCallbackNode
    {
        public Action MicroThreadAction;

        public SendOrPostCallback SendOrPostCallback;

        public object CallbackState;

        public MicroThreadCallbackNode Next;

        public void Invoke()
        {
            if (MicroThreadAction != null)
            {
                MicroThreadAction();
            }
            else
            {
                SendOrPostCallback(CallbackState);
            }
        }

        public void Clear()
        {
            MicroThreadAction = null;
            SendOrPostCallback = null;
            CallbackState = null;
        }
    }

    internal struct MicroThreadCallbackList
    {
        public MicroThreadCallbackNode First { get; private set; }

        public MicroThreadCallbackNode Last { get; private set; }

        public void Add(MicroThreadCallbackNode node)
        {
            if (First == null)
                First = node;
            else
                Last.Next = node;

            Last = node;
        }

        public bool TakeFirst(out MicroThreadCallbackNode callback)
        {
            callback = First;

            if (First == null)
                return false;

            First = callback.Next;
            callback.Next = null;

            return true;
        }
    }
}