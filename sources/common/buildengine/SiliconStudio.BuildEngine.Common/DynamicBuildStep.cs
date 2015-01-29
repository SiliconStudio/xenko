// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    public class DynamicBuildStep : EnumerableBuildStep
    {
        private readonly IBuildStepProvider buildStepProvider;

        /// <summary>
        /// The <see cref="AutoResetEvent"/> used to notify the dynamic build step that new work is requested.
        /// </summary>
        private readonly AutoResetEvent newWorkAvailable = new AutoResetEvent(false);

        public DynamicBuildStep(IBuildStepProvider buildStepProvider, int maxParallelSteps)
        {
            this.buildStepProvider = buildStepProvider;
            MaxParallelSteps = maxParallelSteps;
        }

        /// <summary>
        /// Gets or sets the maximum number of steps that can run at the same time in parallel.
        /// </summary>
        public int MaxParallelSteps { get; set; }

        /// <summary>
        /// Notify the dynamic build step new work is available.
        /// </summary>
        public void NotifyNewWorkAvailable()
        {
            newWorkAvailable.Set();
        }

        public async override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            var buildStepsToWait = new List<BuildStep>();

            while (true)
            {
                // interrupt the build if cancellation is required.
                if (executeContext.CancellationTokenSource.Token.IsCancellationRequested)
                    return ResultStatus.Cancelled;

                // wait for a task to complete
                if (buildStepsToWait.Count >= MaxParallelSteps)
                    await CompleteOneBuildStep(executeContext, buildStepsToWait);

                // Transform item into build step
                var buildStep = buildStepProvider.GetNextBuildStep();

                // No job => passively wait
                if (buildStep == null)
                {
                    newWorkAvailable.WaitOne();

                    continue;
                }

                // Safeguard if the provided build step is already processed
                if(buildStep.Processed)
                    continue;

                if (buildStep is WaitBuildStep)
                {
                    // wait for all the task in execution to complete
                    while (buildStepsToWait.Count>0)
                        await CompleteOneBuildStep(executeContext, buildStepsToWait);

                    continue;
                }

                // Schedule build step
                executeContext.ScheduleBuildStep(buildStep);
                buildStepsToWait.Add(buildStep);
            }
        }

        /// <inheritdoc/>
        public override BuildStep Clone()
        {
            var clone = new DynamicBuildStep(buildStepProvider, MaxParallelSteps);
            return clone;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "DynamicBuildStep";
        }

        private async Task CompleteOneBuildStep(IExecuteContext executeContext, List<BuildStep> buildStepsToWait)
        {
            // Too many build steps, wait for one to finish
            var completeBuildStep = await Task.WhenAny(buildStepsToWait.Select(x => x.ExecutedAsync()));

            // Clear inputs and outputs (each step is independent)
            inputObjects.Clear();
            outputObjects.Clear();

            // Process input and outputs
            await CompleteCommands(executeContext, new List<BuildStep> { completeBuildStep.Result });

            // Remove from list of build step to wait
            buildStepsToWait.Remove(completeBuildStep.Result);
        }
    }
}