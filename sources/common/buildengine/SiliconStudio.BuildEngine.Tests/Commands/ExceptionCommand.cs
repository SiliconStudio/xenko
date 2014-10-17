// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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