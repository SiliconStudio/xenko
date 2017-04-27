// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ServiceModel;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Debugger.Target
{
    /// <summary>
    /// Represents the debugger host commands that the target can access
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IGameDebuggerTarget))]
    public interface IGameDebuggerHost
    {
        [OperationContract]
        void RegisterTarget();

        [OperationContract]
        void OnGameExited();

        [OperationContract(IsOneWay = true)]
        void OnLogMessage(SerializableLogMessage logMessage);
    }
}
