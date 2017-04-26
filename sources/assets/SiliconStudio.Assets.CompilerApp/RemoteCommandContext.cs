// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.CompilerApp
{
    public class RemoteCommandContext : CommandContextBase
    {
        public override LoggerResult Logger { get { return logger; } }

        internal new CommandResultEntry ResultEntry { get { return base.ResultEntry; } }

        private readonly LoggerResult logger;
        private readonly IProcessBuilderRemote processBuilderRemote;

        public RemoteCommandContext(IProcessBuilderRemote processBuilderRemote, Command command, BuilderContext builderContext, LoggerResult logger)
            : base(command, builderContext)
        {
            this.processBuilderRemote = processBuilderRemote;
            this.logger = logger;
        }

        public override IEnumerable<IDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups()
        {
            yield return processBuilderRemote.GetOutputObjects().ToDictionary(x => x.Key, x => new OutputObject(x.Key, x.Value));
        }

        protected override Task<ResultStatus> ScheduleAndExecuteCommandInternal(Command command)
        {
            // Send serialized command
            return processBuilderRemote.SpawnCommand(command);
        }

        protected override ObjectId ComputeInputHash(UrlType type, string filePath)
        {
            return processBuilderRemote.ComputeInputHash(type, filePath);
        }
    }
}
