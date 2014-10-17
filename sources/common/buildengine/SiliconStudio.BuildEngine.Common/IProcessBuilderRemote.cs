// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.BuildEngine
{
    [ServiceContract]
    public interface IProcessBuilderRemote
    {
        [OperationContract]
        [UseParadoxDataContractSerializer]
        Command GetCommandToExecute();

        [OperationContract]
        [UseParadoxDataContractSerializer]
        BuildParameterCollection GetBuildParameters();

        [OperationContract]
        [UseParadoxDataContractSerializer]
        void ForwardLog(SerializableLogMessage message);

        [OperationContract]
        [UseParadoxDataContractSerializer]
        void RegisterResult(CommandResultEntry commandResult);

        [OperationContract]
        [UseParadoxDataContractSerializer]
        Task<ResultStatus> SpawnCommand(Command command);

        [OperationContract]
        [UseParadoxDataContractSerializer]
        ObjectId ComputeInputHash(UrlType type, string filePath);

        [OperationContract]
        [UseParadoxDataContractSerializer]
        Dictionary<ObjectUrl, ObjectId> GetOutputObjects();
    }
}