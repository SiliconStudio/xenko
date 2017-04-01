// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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