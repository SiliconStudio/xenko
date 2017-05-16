// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// When embedded in a <see cref="EnumerableBuildStep"/>, this build step will force all previous computations to be finished before continuing.
    /// </summary>
    [Description("Wait previous steps")]
    public class WaitBuildStep : BuildStep
    {
        public WaitBuildStep()
            : base(ResultStatus.NotTriggeredWasSuccessful)
        {
        }
        
        /// <inheritdoc />
        public override string Title => ToString();

        public override BuildStep Clone()
        {
            return new WaitBuildStep();
        }

        public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            // Dependencies are done in ListBuildStep, Execute() should never be called directly.
            return Task.FromResult(ResultStatus.Failed);
        }

        public override string ToString()
        {
            return "Wait steps completion";
        }
    }
}
