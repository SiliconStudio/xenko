// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// A <see cref="BuildStep"/> that can spawn multiple <see cref="BuildStep"/>.
    /// Input and output tracking and merging will be performed.
    /// Various input/output and output/output conflicts are detected, if <see cref="WaitBuildStep"/> is not used properly.
    /// </summary>
    public abstract class EnumerableBuildStep : BuildStep
    {
        /// <inheritdoc />
        public override string Title => ToString();

        private int mergeCounter;
        private readonly List<BuildStep> executedSteps = new List<BuildStep>();

        protected readonly Dictionary<ObjectUrl, OutputObject> outputObjects = new Dictionary<ObjectUrl, OutputObject>();
        public IDictionary<ObjectUrl, OutputObject> OutputObjects => outputObjects;

        /// <inheritdoc/>
        public override IEnumerable<KeyValuePair<ObjectUrl, ObjectId>> OutputObjectIds => outputObjects.Select(x => new KeyValuePair<ObjectUrl, ObjectId>(x.Key, x.Value.ObjectId));

        protected readonly Dictionary<ObjectUrl, InputObject> inputObjects = new Dictionary<ObjectUrl, InputObject>();
        public readonly IEnumerable<ObjectUrl> InputUrls;

        public IEnumerable<BuildStep> Steps { get; set; }

        protected EnumerableBuildStep()
        {
            InputUrls = inputObjects.Keys;
        }

        protected EnumerableBuildStep(IEnumerable<BuildStep> steps) : this()
        {
            Steps = steps;
        }

        public override async Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            var buildStepsToWait = new List<BuildStep>();

            // Process prerequisites build steps first
            if (PrerequisiteSteps.Count > 0)
                await CompleteCommands(executeContext, PrerequisiteSteps.ToList());

            foreach (var child in Steps)
            {
                // Wait for all the tasks before the WaitBuildStep to be finished
                if (child is WaitBuildStep)
                {
                    await CompleteCommands(executeContext, buildStepsToWait);
                }
                else
                {
                    executeContext.ScheduleBuildStep(child);
                    buildStepsToWait.Add(child);
                }

                executedSteps.Add(child);
            }

            await CompleteCommands(executeContext, buildStepsToWait);
            
            return ComputeResultStatusFromExecutedSteps();
        }

        /// <summary>
        /// Determine the result status of an execution of enumeration of build steps.
        /// </summary>
        /// <returns>The result status of the execution.</returns>
        protected ResultStatus ComputeResultStatusFromExecutedSteps()
        {
            if (executedSteps.Count == 0)
                return ResultStatus.Successful;

            // determine the result status of the list based on the children executed steps
            // -> One or more children canceled => canceled
            // -> One or more children failed (Prerequisite or Command) and none canceled => failed
            // -> One or more children succeeded and none canceled nor failed => succeeded
            // -> All the children were successful without triggering => not triggered was successful
            var result = executedSteps[0].Status;
            foreach (var executedStep in executedSteps)
            {
                if (executedStep.Status == ResultStatus.Cancelled)
                {
                    result = ResultStatus.Cancelled;
                    break;
                }

                if (executedStep.Failed)
                    result = ResultStatus.Failed;
                else if (executedStep.Status == ResultStatus.Successful && result != ResultStatus.Failed)
                    result = ResultStatus.Successful;
            }

            return result;
        }

        /// <summary>
        /// Wait for given build steps to finish, then processes their inputs and outputs.
        /// </summary>
        /// <param name="executeContext">The execute context.</param>
        /// <param name="buildStepsToWait">The build steps to wait.</param>
        /// <returns></returns>
        protected async Task CompleteCommands(IExecuteContext executeContext, List<BuildStep> buildStepsToWait)
        {
            await WaitCommands(buildStepsToWait);

            // TODO: Merge results of sub lists
            foreach (var buildStep in buildStepsToWait)
            {
                var enumerableBuildStep = buildStep as EnumerableBuildStep;
                if (enumerableBuildStep != null)
                {
                    // Merge results from sub list

                    // Step1: Check inputs/outputs conflicts
                    foreach (var inputObject in enumerableBuildStep.inputObjects)
                    {
                        CheckInputObject(executeContext, inputObject.Key, inputObject.Value.Command);
                    }

                    foreach (var outputObject in enumerableBuildStep.OutputObjects)
                    {
                        CheckOutputObject(executeContext, outputObject.Key, outputObject.Value.ObjectId, outputObject.Value.Command);
                    }

                    // Step2: Add inputs/outputs
                    foreach (var inputObject in enumerableBuildStep.inputObjects)
                    {
                        AddInputObject(executeContext, inputObject.Key, inputObject.Value.Command);
                    }

                    foreach (var outputObject in enumerableBuildStep.OutputObjects)
                    {
                        var newOutputObject = AddOutputObject(executeContext, outputObject.Key, outputObject.Value.ObjectId, outputObject.Value.Command);

                        // Merge tags
                        foreach (var tag in outputObject.Value.Tags)
                        {
                            newOutputObject.Tags.Add(tag);
                        }
                    }
                }

                var commandBuildStep = buildStep as CommandBuildStep;
                if (commandBuildStep != null)
                {
                    // Merge results from spawned step
                    ProcessCommandBuildStepResult(executeContext, commandBuildStep);
                }
            }

            buildStepsToWait.Clear();
            mergeCounter++;
        }

        protected internal static async Task WaitCommands(List<BuildStep> buildStepsToWait)
        {
            // Wait for steps to be finished
            if (buildStepsToWait.Count > 0)
                await Task.WhenAll(buildStepsToWait.Select(x => x.ExecutedAsync()));

            // Wait for spawned steps to be finished
            await Task.WhenAll(buildStepsToWait.SelectMany(EnumerateSpawnedBuildSteps).Select(x => x.ExecutedAsync()));
        }

        /// <summary>
        /// Processes the results from a <see cref="CommandBuildStep"/>.
        /// </summary>
        /// <param name="executeContext">The execute context.</param>
        /// <param name="buildStep">The build step.</param>
        private void ProcessCommandBuildStepResult(IExecuteContext executeContext, CommandBuildStep buildStep)
        {
            foreach (var resultInputObject in buildStep.Command.GetInputFiles())
            {
                AddInputObject(executeContext, resultInputObject, buildStep.Command);
            }

            if (buildStep.Result != null)
            {
                // Step1: Check inputs/outputs conflicts
                foreach (var resultInputObject in buildStep.Result.InputDependencyVersions)
                {
                    CheckInputObject(executeContext, resultInputObject.Key, buildStep.Command);
                }

                foreach (var resultOutputObject in buildStep.Result.OutputObjects)
                {
                    CheckOutputObject(executeContext, resultOutputObject.Key, resultOutputObject.Value, buildStep.Command);
                }

                // Step2: Add inputs/outputs
                foreach (var resultInputObject in buildStep.Result.InputDependencyVersions)
                {
                    AddInputObject(executeContext, resultInputObject.Key, buildStep.Command);
                }

                foreach (var resultOutputObject in buildStep.Result.OutputObjects)
                {
                    AddOutputObject(executeContext, resultOutputObject.Key, resultOutputObject.Value, buildStep.Command);
                }
            }

            // Forward logs
            buildStep.Logger.CopyTo(Logger);

            // Process recursively
            // TODO: Wait for completion of spawned step in case Task didn't wait for them
            foreach (var spawnedStep in buildStep.SpawnedSteps)
            {
                ProcessCommandBuildStepResult(executeContext, spawnedStep);
            }

            if (buildStep.Result != null)
            {
                // Resolve tags from TagSymbol
                // TODO: Handle removed tags
                foreach (var tagGroup in buildStep.Result
                    .TagSymbols
                    .Where(x => buildStep.Command.TagSymbols.ContainsKey(x.Value))
                    .GroupBy(x => x.Key, x => buildStep.Command.TagSymbols[x.Value].RealName))
                {
                    var url = tagGroup.Key;

                    // TODO: Improve search complexity?
                    OutputObject outputObject;
                    if (outputObjects.TryGetValue(url, out outputObject))
                    {
                        outputObject.Tags.UnionWith(tagGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the input object. Will try to detect input/output conflicts.
        /// </summary>
        /// <param name="executeContext">The execute context.</param>
        /// <param name="inputObjectUrl">The input object URL.</param>
        /// <param name="command">The command.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        private void CheckInputObject(IExecuteContext executeContext, ObjectUrl inputObjectUrl, Command command)
        {
            OutputObject outputObject;
            if (outputObjects.TryGetValue(inputObjectUrl, out outputObject)
                && outputObject.Command != command
                && outputObject.Counter == mergeCounter)
            {
                var error = string.Format("Command {0} is writing {1} while command {2} is reading it", outputObject.Command, inputObjectUrl, command);
                executeContext.Logger.Error(error);
                throw new InvalidOperationException(error);
            }
        }

        private void AddInputObject(IExecuteContext executeContext, ObjectUrl inputObjectUrl, Command command)
        {
            OutputObject outputObject;
            if (outputObjects.TryGetValue(inputObjectUrl, out outputObject)
                && mergeCounter > outputObject.Counter)
            {
                // Object was outputed by ourself, so reading it as input should be ignored.
                return;
            }

            inputObjects[inputObjectUrl] = new InputObject { Command = command, Counter = mergeCounter };
        }

        /// <summary>
        /// Adds the output object. Will try to detect input/output conflicts, and output with different <see cref="ObjectId" /> conflicts.
        /// </summary>
        /// <param name="executeContext">The execute context.</param>
        /// <param name="outputObjectUrl">The output object URL.</param>
        /// <param name="outputObjectId">The output object id.</param>
        /// <param name="command">The command that produced the output object.</param>
        /// <exception cref="System.InvalidOperationException">Two CommandBuildStep with same inputs did output different results.</exception>
        private void CheckOutputObject(IExecuteContext executeContext, ObjectUrl outputObjectUrl, ObjectId outputObjectId, Command command)
        {
            InputObject inputObject;
            if (inputObjects.TryGetValue(outputObjectUrl, out inputObject)
                && inputObject.Command != command
                && inputObject.Counter == mergeCounter)
            {
                var error = string.Format("Command {0} is writing {1} while command {2} is reading it", command, outputObjectUrl, inputObject.Command);
                executeContext.Logger.Error(error);
                throw new InvalidOperationException(error);
            }
        }

        internal OutputObject AddOutputObject(IExecuteContext executeContext, ObjectUrl outputObjectUrl, ObjectId outputObjectId, Command command)
        {
            OutputObject outputObject;

            if (!outputObjects.TryGetValue(outputObjectUrl, out outputObject))
            {
                // New item?
                outputObject = new OutputObject(outputObjectUrl, outputObjectId);
                outputObjects.Add(outputObjectUrl, outputObject);
            }
            else
            {
                // ObjectId should be similar (if no Wait happened), otherwise two tasks spawned with same parameters did output different results
                if (outputObject.ObjectId != outputObjectId && outputObject.Counter == mergeCounter)
                {
                    var error = string.Format("Commands {0} and {1} are both writing {2} at the same time", command, outputObject.Command, outputObjectUrl);
                    executeContext.Logger.Error(error);
                    throw new InvalidOperationException(error);
                }

                // Update new ObjectId
                outputObject.ObjectId = outputObjectId;
            }

            // Update Counter so that we know if a wait happened since this output object has been merged.
            outputObject.Counter = mergeCounter;
            outputObject.Command = command;

            return outputObject;
        }

        private static IEnumerable<BuildStep> EnumerateSpawnedBuildSteps(BuildStep buildStep)
        {
            foreach (var spawnedStep in buildStep.SpawnedSteps)
            {
                yield return spawnedStep;
                foreach (var childSpawnedStep in EnumerateSpawnedBuildSteps(spawnedStep))
                {
                    yield return childSpawnedStep;
                }
            }
        }

        protected struct InputObject
        {
            public Command Command;
            public int Counter;
        }
    }
}
