// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ServiceModel;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine
{
    [ServiceContract]
    public interface IForwardSerializableLogRemote
    {
        [OperationContract(IsOneWay = true)]
        [UseXenkoDataContractSerializer]
        void ForwardSerializableLog(SerializableLogMessage message);
    }
}
