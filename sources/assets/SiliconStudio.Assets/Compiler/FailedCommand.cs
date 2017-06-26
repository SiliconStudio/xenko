// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Threading.Tasks;

using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Compiler
{
    public class FailedCommand: Command
    {
        private readonly string objectThatFailed;

        public FailedCommand(string objectThatFailed)
        {
            this.objectThatFailed = objectThatFailed;
        }

        public override string Title => $"Failed command [Object={objectThatFailed}]";

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            return Task.FromResult(ResultStatus.Failed);
        }

        public override string ToString()
        {
            return Title;
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            // force execution of the command with a new GUID
            var newGuid = Guid.NewGuid();
            writer.Serialize(ref newGuid, ArchiveMode.Serialize);
        }
    }
}
