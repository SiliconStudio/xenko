// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

using SiliconStudio.BuildEngine.Monitor.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.Service
{
    public class BuildEventArgs : EventArgs
    {
        public Guid BuildId { get; private set; }
        public DateTime Time { get; private set; }
        public BuildEventArgs(Guid buildId, DateTime time) { BuildId = buildId; Time = time; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BuildMonitorRemote : IBuildMonitorRemote
    {
        private readonly ICollection<BuildSessionViewModel> buildSessions;
        private readonly Dictionary<long, ResultStatus> updatedStatus = new Dictionary<long, ResultStatus>();

        private readonly int processId;

        int counterSendCommandLog;
        int counterSendMicrothreadEvents;
        int counterSendBuildStepInfo;
        int counterSendBuildStepResult;

        public BuildMonitorRemote(ICollection<BuildSessionViewModel> buildSessions)
        {
            this.buildSessions = buildSessions;
            processId = Process.GetCurrentProcess().Id;
        }

        public int Ping()
        {
            return processId;
        }

        public void SendCommandLog(Guid buildId, DateTime startTime, long executionId, List<SerializableTimestampLogMessage> messages)
        {
            ++counterSendCommandLog;
            BuildSessionViewModel buildSession = GetSession(buildId, true, startTime);
            buildSession.AppendLog(executionId, messages);
        }

        public void SendMicrothreadEvents(Guid buildId, DateTime startTime, DateTime now, IEnumerable<MicrothreadNotification> microthreadJobInfo)
        {
            ++counterSendMicrothreadEvents;
            BuildSessionViewModel buildSession = GetSession(buildId, true, startTime);

            foreach (MicrothreadNotification notification in microthreadJobInfo)
            {
                MicrothreadJobViewModel job = buildSession.GetJob(notification.MicrothreadJobInfoId);
                if (job == null)
                {
                    var newJob = new MicrothreadJobViewModel(buildSession.GetBuildStep(notification.MicrothreadId))
                    {
                        ThreadId = notification.ThreadId,
                        JobId = notification.MicrothreadJobInfoId,
                        InProgress = notification.Type == MicrothreadNotification.NotificationType.JobStarted
                    };

                    switch (notification.Type)
                    {
                        case MicrothreadNotification.NotificationType.JobStarted:
                            newJob.Start = startTime + TimeSpan.FromTicks(notification.Time);
                            newJob.End = now;
                            break;
                        case MicrothreadNotification.NotificationType.JobEnded:
                            newJob.Start = startTime;
                            newJob.End = startTime + TimeSpan.FromTicks(notification.Time);
                            break;
                    }

                    buildSession.AddJob(newJob);
                }
                else
                {
                    Debug.Assert(notification.Type == MicrothreadNotification.NotificationType.JobEnded);
                    job.End = startTime + TimeSpan.FromTicks(notification.Time);
                    job.InProgress = false;
                }
            }
            foreach (MicrothreadJobViewModel job in buildSession.Jobs.Where(job => job.InProgress))
            {
                job.End = now;
            }
            buildSession.EndTime = now;
        }

        public void SendBuildStepInfo(Guid buildId, long executionId, string description, DateTime startTime)
        {
            ++counterSendBuildStepInfo;
            BuildSessionViewModel buildSession = GetSession(buildId, true, startTime);
            BuildStepViewModel model = buildSession.RegisterBuildStep(executionId, description);
            lock (updatedStatus)
            {
                ResultStatus result;
                if (updatedStatus.TryGetValue(executionId, out result))
                    model.Status = result;
                updatedStatus.Remove(executionId);
            }
        }

        public void SendBuildStepResult(Guid buildId, DateTime startTime, long executionId, ResultStatus status)
        {
            ++counterSendBuildStepResult;
            BuildSessionViewModel buildSession = GetSession(buildId, true, startTime);
            var buildStep = buildSession.GetBuildStep(executionId);

            if (buildStep != null)
            {
                buildStep.Status = status;
            }
            else
            {
                lock (updatedStatus)
                {
                    updatedStatus[executionId] = status;
                }
            }
        }

        public void StartBuild(Guid buildId, DateTime time)
        {
            counterSendCommandLog = 0;
            counterSendMicrothreadEvents = 0;
            counterSendBuildStepInfo = 0;
            counterSendBuildStepResult = 0;

            var buildSession = new BuildSessionViewModel(buildId, time);
            buildSessions.Add(buildSession);
            buildSession.EndTime = time + new TimeSpan(0, 0, 0);
        }

        public void EndBuild(Guid buildId, DateTime time)
        {
            BuildSessionViewModel buildSession = GetSession(buildId, false);

            // If we didn't record anything but the end of the build, discard it since we have nothing to show anyway. Rare case but may happen. 
            if (buildSession != null)
            {
                buildSession.EndTime = time;
            }
            Console.WriteLine(@"counterSendCommandLog: {0}", counterSendCommandLog);
            Console.WriteLine(@"counterSendMicrothreadEvents: {0}", counterSendMicrothreadEvents);
            Console.WriteLine(@"counterSendBuildStepInfo: {0}", counterSendBuildStepInfo);
            Console.WriteLine(@"counterSendBuildStepResult: {0}", counterSendBuildStepResult);
        }

        private BuildSessionViewModel GetSession(Guid buildId, bool createIfNew, DateTime? startTime = null)
        {
            BuildSessionViewModel buildSession = buildSessions.SingleOrDefault(x => x.BuildId == buildId);
            if (buildSession == null && createIfNew)
            {
                buildSession = startTime != null ? new BuildSessionViewModel(buildId, startTime.Value) : new BuildSessionViewModel(buildId);
                buildSessions.Add(buildSession);
            }
            return buildSession;
        }
    }
}
