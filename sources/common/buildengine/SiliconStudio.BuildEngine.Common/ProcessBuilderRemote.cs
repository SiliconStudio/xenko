// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class ProcessBuilderRemote : IProcessBuilderRemote
    {
        private readonly LocalCommandContext commandContext;
        private readonly Command remoteCommand;

        public CommandResultEntry Result { get; protected set; }

        public ProcessBuilderRemote(LocalCommandContext commandContext, Command remoteCommand)
        {
            this.commandContext = commandContext;
            this.remoteCommand = remoteCommand;
        }

        public Command GetCommandToExecute()
        {
            return remoteCommand;
        }

        public void RegisterResult(CommandResultEntry commandResult)
        {
            Result = commandResult;
        }

        public void ForwardLog(SerializableLogMessage message)
        {
            commandContext.Logger.Log(new LogMessage(message.Module, message.Type, message.Text));
            if (message.ExceptionInfo != null)
                commandContext.Logger.Log(new LogMessage(message.Module, message.Type, message.ExceptionInfo.ToString()));
        }

        public ObjectId ComputeInputHash(UrlType type, string filePath)
        {
            return commandContext.ComputeInputHash(type, filePath);
        }

        public Dictionary<ObjectUrl, ObjectId> GetOutputObjects()
        {
            var result = new Dictionary<ObjectUrl, ObjectId>();
            foreach (var outputObjects in commandContext.GetOutputObjectsGroups())
            {
                foreach (var outputObject in outputObjects)
                {
                    if (!result.ContainsKey(outputObject.Key))
                    {
                        result.Add(outputObject.Key, outputObject.Value.ObjectId);
                    }
                }
            }
            return result;
        }
    }
}
