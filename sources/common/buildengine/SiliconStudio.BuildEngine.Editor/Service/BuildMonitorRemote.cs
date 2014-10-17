using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.BuildEngine.Editor.ViewModel;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Quantum.Legacy;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;

namespace SiliconStudio.BuildEngine.Editor.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BuildMonitorRemote : IBuildMonitorRemote
    {
        private readonly ViewModelContext context = new ViewModelContext();
        private readonly BuildEditionViewModel edition;

        private readonly int processId;

        public BuildMonitorRemote(BuildEditionViewModel edition)
        {
            this.edition = edition;
            processId = Process.GetCurrentProcess().Id;
        }

        public int Ping()
        {
            return processId;
        }

        public void SendBuildStepInfo(Guid buildId, DateTime startTime, byte[] buildStepPacket)
        {
            const int Unused = 0;
            ViewModelController.NetworkDeserialize(Unused, context, buildStepPacket);

            foreach (IViewModelNode buildStepNode in context.ViewModelByGuid.Values)
            {
                var executionId = (long)buildStepNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.ExecutionIdPropertyName).Content.Value;
                var status = (ResultStatus)buildStepNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.StatusPropertyName).Content.Value;
                IViewModelNode tagNode = buildStepNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.TagPropertyName);
                if (tagNode.Content.Value != null)
                {
                    var tag = (Guid)tagNode.Content.Value;
                    IViewModelNode uiNode = edition.ActiveSession.GetBuildStepNode(tag);

                    if (uiNode != null)
                    {
                        ((ObservableViewModelNode<bool>)uiNode.Children.Single(x => x.Name == "IsRunning")).TValue = executionId != 0;
                        ((ObservableViewModelNode<ResultStatus>)uiNode.Children.Single(x => x.Name == "ExecutionStatus")).TValue = status;
                    }
                }
            }
        }

        public void SendCommandLog(Guid buildId, DateTime startTime, long executionId, List<TimestampLocalLogger.Message> messages)
        {
        }

        public void SendMicrothreadEvents(Guid buildId, DateTime startTime, DateTime now, IEnumerable<MicrothreadNotification> microthreadJobInfo)
        {
        }

        public void SendBuildStepResult(Guid buildId, DateTime startTime, long executionId, ResultStatus status)
        {
        }

        public void StartBuild(Guid buildId, DateTime time)
        {
        }

        public void EndBuild(Guid buildId, DateTime time)
        {
        }
    }
}
