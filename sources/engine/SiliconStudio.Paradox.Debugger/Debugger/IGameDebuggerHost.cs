// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ServiceModel;

namespace SiliconStudio.Paradox.Debugger.Target
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
    }
}