// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.BuildEngine
{
    public interface ICommandContext
    {
        Command CurrentCommand { get; }
        LoggerResult Logger { get; }
        BuildParameterCollection BuildParameters { get; }

        IMetadataProvider MetadataProvider { get; }

        Task<ResultStatus> ScheduleAndExecuteCommand(Command command);

        IEnumerable<IDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();

        void RegisterInputDependency(ObjectUrl url);
        void RegisterOutput(ObjectUrl url, ObjectId hash);
        void RegisterCommandLog(IEnumerable<ILogMessage> logMessages);

        void AddTag(ObjectUrl url, TagSymbol tagSymbol);

        void RegisterSpawnedCommandWithoutScheduling(Command command);
    }
}