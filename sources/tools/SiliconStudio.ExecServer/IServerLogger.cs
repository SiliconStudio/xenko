// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
