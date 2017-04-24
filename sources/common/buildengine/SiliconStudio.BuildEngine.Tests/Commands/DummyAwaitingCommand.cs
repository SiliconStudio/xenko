// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    public class DummyAwaitingCommand : TestCommand
    {
        public int Delay = 0;

        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            // Simulating awaiting result
            try
            {
                await Task.Delay(Delay, CancellationToken);

            }
            catch (TaskCanceledException) {}

            return CancellationToken.IsCancellationRequested ? ResultStatus.Cancelled : ResultStatus.Successful;
        }
    }
}
