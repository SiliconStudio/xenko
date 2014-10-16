// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.ViewModel
{
    public class MicrothreadJobViewModel : ViewModelBase
    {
        private DateTime start;
        private DateTime end;
        private bool inProgress;
        private int threadId;
        private long jobId;
        
        public MicrothreadJobViewModel(BuildStepViewModel buildStep)
        {
            if (buildStep == null) throw new ArgumentNullException("buildStep");
            BuildStep = buildStep;
            BuildStep.PropertyChanging += BuildStepPropertyChanging;
            BuildStep.PropertyChanged += BuildStepPropertyChanged;
        }

        public BuildStepViewModel BuildStep { get; private set; }

        public long MicrothreadId { get { return BuildStep.ExecutionId; } }

        public DateTime Start { get { return start; } set { SetValue(ref start, value); } }

        public DateTime End { get { return end; } set { SetValue(ref end, value); } }

        public bool InProgress { get { return inProgress; } set { SetValue(ref inProgress, value); } }

        public int ThreadId { get { return threadId; } set { SetValue(ref threadId, value); } }

        public long JobId { get { return jobId; } set { SetValue(ref jobId, value); } }

        public ResultStatus Status { get { return BuildStep != null ? BuildStep.Status : ResultStatus.NotProcessed; } }

        public bool IsSuccessful { get { return Status == ResultStatus.Successful || Status == ResultStatus.NotTriggeredWasSuccessful; } }

        public bool HasFailed { get { return Status == ResultStatus.Failed || Status == ResultStatus.NotTriggeredPrerequisiteFailed; } }

        public string Description { get { return BuildStep != null ? BuildStep.Description : "<Unknown>"; } }

        public static long HighlightedMicrothreadJob { get; set; }

        public bool IsHighlighted { get { return MicrothreadId == HighlightedMicrothreadJob; } }

        private void BuildStepPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                OnPropertyChanging("Status");
                OnPropertyChanging("IsSuccessful");
                OnPropertyChanging("HasFailed");
            }
        }

        private void BuildStepPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                OnPropertyChanged("Status");
                OnPropertyChanged("IsSuccessful");
                OnPropertyChanged("HasFailed");
            }
        }

        public void UpdatingHighlightedMicrothreadJob()
        {
            OnPropertyChanging("IsHighlighted");
        }

        public void UpdatedHighlightedMicrothreadJob()
        {
            OnPropertyChanged("IsHighlighted");
        }
    }
}
