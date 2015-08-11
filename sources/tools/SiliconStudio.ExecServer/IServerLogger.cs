// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ServiceModel;

namespace SiliconStudio.ExecServer
{
    /// <summary>
    /// Interface used to log back standard output and error to client.
    /// </summary>
    [ServiceContract]
    public interface IServerLogger
    {
        [OperationContract(IsOneWay = true)]
        void OnLog(string text, ConsoleColor color);
    }
}