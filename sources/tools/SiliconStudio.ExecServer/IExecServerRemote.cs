// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.ServiceModel;

namespace SiliconStudio.ExecServer
{
    /// <summary>
    /// Main server interface
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IServerLogger), SessionMode = SessionMode.Required)]
    public interface IExecServerRemote
    {
        [OperationContract(IsInitiating =  true)]
        void Check();

        [OperationContract(IsTerminating = true)]
        int Run(string currentDirectory, Dictionary<string, string> environmentVariables, string[] args, bool shadowCache, int? debuggerProcessId);
    }
}
