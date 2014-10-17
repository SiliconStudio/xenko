// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Input;

using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.ViewModel
{
    public class BuildStepViewModel : ViewModelBase
    {
        private ResultStatus status;

        public BuildStepViewModel(BuildSessionViewModel session, long executionId, string description)
        {
            ExecutionId = executionId;
            Description = description;

            SelectLinkCommand = new AnonymousCommand(session.ServiceProvider, x => session.SelectBuildStep(Convert.ToInt64(x ?? -1)));
        }

        public string Description { get; private set; }

        public long ExecutionId { get; private set; }

        public ICommand SelectLinkCommand { get; private set; }

        public ResultStatus Status { get { return status; } set { SetValue(ref status, value); } }
    }
}
