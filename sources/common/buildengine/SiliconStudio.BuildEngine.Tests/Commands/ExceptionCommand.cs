// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    public class ExceptionCommand : TestCommand
    {
        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            throw new NotImplementedException();
        }
    }
}
