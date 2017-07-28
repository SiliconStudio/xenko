// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.CompilerApp
{
    public class RemoteCommandContext : CommandContextBase
    {
        private readonly IProcessBuilderRemote processBuilderRemote;

        public RemoteCommandContext(IProcessBuilderRemote processBuilderRemote, Command command, BuilderContext builderContext, LoggerResult logger)
            : base(command, builderContext)
        {
            this.processBuilderRemote = processBuilderRemote;
            Logger = logger;
        }

        public override LoggerResult Logger { get; }

        internal new CommandResultEntry ResultEntry => base.ResultEntry;

        public override IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups()
        {
            yield return processBuilderRemote.GetOutputObjects().ToDictionary(x => x.Key, x => new OutputObject(x.Key, x.Value));
        }

        protected override ObjectId ComputeInputHash(UrlType type, string filePath)
        {
            return processBuilderRemote.ComputeInputHash(type, filePath);
        }
    }
}
