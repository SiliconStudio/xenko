// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel.Design;
using System.ServiceModel;

namespace SiliconStudio.ExecServer
{
    [ServiceContract]
    public interface IExecServerRemote
    {
        [OperationContract]
        void Check();

        [OperationContract]
        int Run(string[] args);
    }
}