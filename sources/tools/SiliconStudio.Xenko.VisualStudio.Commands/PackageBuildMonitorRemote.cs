// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.ServiceModel;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.VisualStudio.Commands;

namespace SiliconStudio.Xenko.VisualStudio.BuildEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PackageBuildMonitorRemote : IForwardSerializableLogRemote
    {
        private string logPipeUrl;
        private IBuildMonitorCallback buildMonitorCallback;

        public PackageBuildMonitorRemote(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl)
        {
            this.buildMonitorCallback = buildMonitorCallback;
            this.logPipeUrl = logPipeUrl;

            // Listen to pipe with this as listener
            var host = new ServiceHost(this);
            host.AddServiceEndpoint(typeof(IForwardSerializableLogRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { MaxReceivedMessageSize = int.MaxValue }, this.logPipeUrl);
            host.Open();
        }

        public void ForwardSerializableLog(SerializableLogMessage message)
        {
            buildMonitorCallback.Message(message.Type.ToString(), message.Module, message.Text);
        }
    }
}
