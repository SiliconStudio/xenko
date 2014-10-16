// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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