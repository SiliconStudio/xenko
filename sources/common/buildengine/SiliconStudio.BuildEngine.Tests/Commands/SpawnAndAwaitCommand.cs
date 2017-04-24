// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    public class SpawnAndAwaitCommand : TestCommand
    {
        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            for (int i = 0; i < 10; ++i)
            {
                var command = new DummyBlockingCommand { Delay = 100 };
                ResultStatus subCommandStatus = await commandContext.ScheduleAndExecuteCommand(command);
                if (subCommandStatus != ResultStatus.Successful)
                    return subCommandStatus;
            }

            return ResultStatus.Successful;
        }
    }
}
