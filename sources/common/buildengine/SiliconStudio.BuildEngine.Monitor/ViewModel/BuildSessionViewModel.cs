// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.BuildEngine.Monitor.ViewModel
{
    public class BuildSessionViewModel : ViewModelBase
    {
        private Guid buildId;
        public Guid BuildId { get { return buildId; } set { SetValue(ref buildId, value); } }

        private DateTime startTime;
        public DateTime StartTime { get { return startTime; } set { SetValue(ref startTime, value); } }

        private DateTime endTime;
        public DateTime EndTime { get { return endTime; } set { SetValue(ref endTime, value); } }

        private readonly ObservableCollection<MicrothreadJobViewModel> jobs = new ObservableCollection<MicrothreadJobViewModel>();
        public IEnumerable<MicrothreadJobViewModel> Jobs { get { return jobs; } }

        private readonly Dictionary<long, MicrothreadJobViewModel> jobsById = new Dictionary<long, MicrothreadJobViewModel>();
        public IReadOnlyDictionary<long, MicrothreadJobViewModel> JobsById { get { return jobsById; } }

        public BuildStepViewModel SelectedBuildStep { get; private set; }

        private readonly Dictionary<long, ObservableCollection<SerializableTimestampLogMessage>> logs;
        private readonly MultiValueSortedList<long, SerializableTimestampLogMessage> fullLog;

        public long SelectedId { get { return SelectedBuildStep != null ? SelectedBuildStep.ExecutionId : -1; } }

        public IEnumerable<SerializableTimestampLogMessage> SelectedLog { get { lock (logs) { return GetLog(SelectedId); } } }

        private readonly Dictionary<long, BuildStepViewModel> buildStepIds;

        public BuildSessionViewModel()
        {
            buildStepIds = new Dictionary<long, BuildStepViewModel>();
            logs = new Dictionary<long, ObservableCollection<SerializableTimestampLogMessage>>();
            fullLog = new MultiValueSortedList<long, SerializableTimestampLogMessage>();
        }

        public BuildSessionViewModel(Guid buildId, DateTime startTime)
            : this()
        {
            BuildId = buildId;
            StartTime = startTime;
            EndTime = DateTime.MinValue;    
        }

        public BuildSessionViewModel(Guid buildId)
            : this()
        {
            BuildId = buildId;
            StartTime = DateTime.MaxValue;
            EndTime = DateTime.MinValue;
        }

        public void AddJob(MicrothreadJobViewModel job)
        {
            jobs.Add(job);
            jobsById.Add(job.JobId, job);
        }

        public MicrothreadJobViewModel GetJob(long jobId)
        {
            MicrothreadJobViewModel result;
            jobsById.TryGetValue(jobId, out result);
            return result;
        }

        public BuildStepViewModel GetBuildStep(long executionId)
        {
            BuildStepViewModel buildStepViewModel;
            buildStepIds.TryGetValue(executionId, out buildStepViewModel);
            return buildStepViewModel;
        }

        public BuildStepViewModel RegisterBuildStep(long executionId, string buildStepDesc)
        {
            BuildStepViewModel buildStepViewModel;

            if (!buildStepIds.TryGetValue(executionId, out buildStepViewModel))
            {
                buildStepViewModel = new BuildStepViewModel(this, executionId, buildStepDesc);
                buildStepIds.Add(executionId, buildStepViewModel);
            }

            return buildStepViewModel;
        }

        public void AppendLog(long executionId, List<SerializableTimestampLogMessage> messages)
        {
            if (SelectedId < 0 || SelectedId == executionId)
                OnPropertyChanging("SelectedLog");
            lock (logs)
            {
                ObservableCollection<SerializableTimestampLogMessage> logMessages;
                if (!logs.TryGetValue(executionId, out logMessages))
                {
                    logMessages = new ObservableCollection<SerializableTimestampLogMessage>();
                    logs.Add(executionId, logMessages);
                }

                foreach (SerializableTimestampLogMessage message in messages)
                {
                    logMessages.Add(message);
                    fullLog.Add(message.Timestamp, message);
                }
            }
            if (SelectedId < 0 || SelectedId == executionId)
                OnPropertyChanged("SelectedLog");
        }

        public void SelectBuildStep(long executionId)
        {
            OnPropertyChanging("SelectedBuildStep", "SelectedLog", "SelectedId");
            
            BuildStepViewModel buildStep;
            if (buildStepIds.TryGetValue(executionId, out buildStep))
            {
                lock (logs)
                {
                    if (!logs.ContainsKey(executionId))
                    {
                        logs.Add(executionId, new ObservableCollection<SerializableTimestampLogMessage>());
                    }
                }
            }

            SelectedBuildStep = buildStep;

            OnPropertyChanged("SelectedBuildStep", "SelectedLog", "SelectedId");
        }

        private IEnumerable<SerializableTimestampLogMessage> GetLog(long index)
        {
            lock (logs)
            {
                if (index < 0)
                    return fullLog.Values;

                if (!logs.ContainsKey(index))
                {
                    logs.Add(index, new ObservableCollection<SerializableTimestampLogMessage>());
                }
                return logs[index];
            }
        }
    }
}
