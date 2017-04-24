// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    public class FailingCommand : TestCommand
    {
        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            return Task.FromResult(ResultStatus.Failed);
        }
    }
}
