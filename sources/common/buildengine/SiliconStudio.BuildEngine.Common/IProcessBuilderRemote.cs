// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine
{
    [ServiceContract]
    public interface IProcessBuilderRemote
    {
        [OperationContract]
        [UseXenkoDataContractSerializer]
        Command GetCommandToExecute();

        [OperationContract]
        [UseXenkoDataContractSerializer]
        void ForwardLog(SerializableLogMessage message);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        void RegisterResult(CommandResultEntry commandResult);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        ObjectId ComputeInputHash(UrlType type, string filePath);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        Dictionary<ObjectUrl, ObjectId> GetOutputObjects();
    }
}
