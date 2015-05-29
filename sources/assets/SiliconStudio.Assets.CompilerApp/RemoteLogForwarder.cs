// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ServiceModel;

using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.CompilerApp
{
    class RemoteLogForwarder : LogListener
    {
        private readonly ILogger mainLogger;
        private readonly List<IForwardSerializableLogRemote> remoteLogs = new List<IForwardSerializableLogRemote>();
        
        public RemoteLogForwarder(ILogger mainLogger, IEnumerable<string> logPipeNames)
        {
            this.mainLogger = mainLogger;

            foreach (var logPipeName in logPipeNames)
            {
                var namedPipeBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { SendTimeout = TimeSpan.FromSeconds(300.0) };
                var remoteLog = ChannelFactory<IForwardSerializableLogRemote>.CreateChannel(namedPipeBinding, new EndpointAddress(logPipeName));
                remoteLogs.Add(remoteLog);
            }
        }

        public override void Dispose()
        {
            foreach (var remoteLog in remoteLogs)
            {
                try
                {
                    // ReSharper disable SuspiciousTypeConversion.Global
                    var channel = remoteLog as ICommunicationObject;
                    // ReSharper restore SuspiciousTypeConversion.Global
                    if (channel != null) 
                        channel.Close();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }

        protected override void OnLog(ILogMessage message)
        {
            var serializableMessage = message as SerializableLogMessage;
            if (serializableMessage == null)
            {
                var assetMessage = message as AssetLogMessage;
                if (assetMessage != null)
                {
                    assetMessage.Module = mainLogger.Module;
                    serializableMessage = new AssetSerializableLogMessage(assetMessage);
                }
                else
                {
                    var logMessage = message as LogMessage;
                    serializableMessage = logMessage != null ? new SerializableLogMessage(logMessage) : null;
                }
            }

            if (serializableMessage == null)
            {
                throw new ArgumentException(@"Unable to process the given log message.", "message");
            }

            foreach (var remoteLog in remoteLogs)
            {
                try
                {
                    remoteLog.ForwardSerializableLog(serializableMessage);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }
    }
}
