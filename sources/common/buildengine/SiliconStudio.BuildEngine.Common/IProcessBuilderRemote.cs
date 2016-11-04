// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
        BuildParameterCollection GetBuildParameters();

        [OperationContract]
        [UseXenkoDataContractSerializer]
        void ForwardLog(SerializableLogMessage message);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        void RegisterResult(CommandResultEntry commandResult);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        Task<ResultStatus> SpawnCommand(Command command);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        ObjectId ComputeInputHash(UrlType type, string filePath);

        [OperationContract]
        [UseXenkoDataContractSerializer]
        Dictionary<ObjectUrl, ObjectId> GetOutputObjects();
    }
}