// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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