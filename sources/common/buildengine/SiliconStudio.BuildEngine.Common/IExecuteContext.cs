// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Threading;

using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.BuildEngine
{
    public interface IPrepareContext
    {
        Logger Logger { get; }
        ObjectId ComputeInputHash(UrlType type, string filePath);
    }

    public interface IExecuteContext : IPrepareContext
    {
        CancellationTokenSource CancellationTokenSource { get; }
        ObjectDatabase ResultMap { get; }
        Dictionary<string, string> Variables { get; }

        void ScheduleBuildStep(BuildStep step);

        IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();

        CommandBuildStep IsCommandCurrentlyRunning(ObjectId commandHash);
        void NotifyCommandBuildStepStarted(CommandBuildStep commandBuildStep, ObjectId commandHash);
        void NotifyCommandBuildStepFinished(CommandBuildStep commandBuildStep, ObjectId commandHash);
    }
}
