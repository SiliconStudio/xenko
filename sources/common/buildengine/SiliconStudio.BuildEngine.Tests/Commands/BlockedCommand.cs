// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    public class BlockedCommand : TestCommand
    {
        private readonly Semaphore sem = new Semaphore(0, 1);

        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            sem.WaitOne();
            return await Task.FromResult(CancellationToken.IsCancellationRequested ? ResultStatus.Cancelled : ResultStatus.Successful);
        }

        public override void Cancel()
        {
            sem.Release();
        }
    }
}