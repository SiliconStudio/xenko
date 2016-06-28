// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.IO;

using System.Diagnostics;
using System.ServiceModel;

namespace SiliconStudio.BuildEngine
{
    public class CommandBuildStep : BuildStep
    {
        /// <inheritdoc />
        public override string Title { get { return Command != null ? Command.Title : "<No command>"; } }

        public Command Command { get; private set; }

        /// <inheritdoc/>
        public override string OutputLocation => Command.OutputLocation;

        /// <inheritdoc/>
        public override IEnumerable<KeyValuePair<ObjectUrl, ObjectId>> OutputObjectIds => Result.OutputObjects;

        /// <summary>
        /// Command Result, set only after step completion. Not thread safe, should not be modified
        /// </summary>
        public CommandResultEntry Result;

        /// <summary>
        /// When the command is executed by another local process, a thread of the master builder will be blocked to save CPU for the slave one.
        /// However, the slave process may spawn another command on the master, in which case we would like to unblock the thread and await for the spawned command.
        /// </summary>
        private readonly List<Task> spawnedCommandsToWait = new List<Task>();

        public CommandBuildStep(Command command)
        {
            Command = command;
        }

        public override string ToString()
        {
            return Command.ToString();
        }

        public override BuildStep Clone()
        {
            return new CommandBuildStep(Command.Clone());
        }

        public override void Clean(IExecuteContext executeContext, BuilderContext builderContext, bool deleteOutput)
        {
            // try to retrieve result from one of the object store
            var commandHash = Command.ComputeCommandHash(executeContext);
            // If there was an error computing the hash, early exit
            if (commandHash == ObjectId.Empty)
            {
                return;
            }

            var commandResultsFileStream = executeContext.ResultMap.OpenStream(commandHash, VirtualFileMode.OpenOrCreate, VirtualFileAccess.ReadWrite, VirtualFileShare.ReadWrite);
            var commandResultEntries = new ListStore<CommandResultEntry>(commandResultsFileStream) { AutoLoadNewValues = false };
            commandResultEntries.LoadNewValues();
            commandResultsFileStream.Close();

            CommandResultEntry matchingResult = FindMatchingResult(executeContext, commandResultEntries.GetValues());
            if (matchingResult != null)
            {
                if (deleteOutput)
                {
                    foreach (KeyValuePair<ObjectUrl, ObjectId> outputObject in matchingResult.OutputObjects)
                    {
                        switch (outputObject.Key.Type)
                        {
                            case UrlType.File:
                                try
                                {
                                    if (File.Exists(outputObject.Key.Path))
                                        File.Delete(outputObject.Key.Path);
                                }
                                catch (Exception)
                                {
                                    executeContext.Logger.Error("Unable to delete file: " + outputObject.Key.Path);
                                }
                                break;
                            case UrlType.ContentLink:
                            case UrlType.Content:
                                executeContext.ResultMap.Delete(outputObject.Value);
                                break;
                        }
                    }
                }
                foreach (CommandBuildStep spawnedStep in matchingResult.SpawnedCommands.Select(spawnedCommand => new CommandBuildStep(spawnedCommand)))
                {
                    spawnedStep.Clean(executeContext, builderContext, deleteOutput);
                }
            }

            executeContext.ResultMap.Delete(commandHash);
        }

        public override async Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            ListStore<CommandResultEntry> commandResultEntries;

            // Prevent several command build step to evaluate wheither they should start at the same time. This increase the efficiency of the builder by avoiding the same command to be executed several time in parallel
            // NOTE: Careful here, there's no try/finally block around the monitor Enter/Exit, so no non-fatal exception is allowed!
            Monitor.Enter(executeContext);
            bool monitorExited = false;
            var status = ResultStatus.NotProcessed;
            // if any external input has changed since the last execution (or if we don't have a successful execution in cache, trigger the command
            CommandResultEntry matchingResult = null;

            try
            {
                ObjectId commandHash;
                {
                    // try to retrieve result from one of the object store
                    commandHash = Command.ComputeCommandHash(executeContext);
                    // Early exit if the hash of the command failed
                    if (commandHash == ObjectId.Empty)
                    {
                        return ResultStatus.Failed;
                    }

                    var commandResultsFileStream = executeContext.ResultMap.OpenStream(commandHash, VirtualFileMode.OpenOrCreate, VirtualFileAccess.ReadWrite, VirtualFileShare.ReadWrite);
                    commandResultEntries = new ListStore<CommandResultEntry>(commandResultsFileStream) { AutoLoadNewValues = false };
                    commandResultEntries.LoadNewValues();
                }

                if (ShouldExecute(executeContext, commandResultEntries.GetValues(), commandHash, out matchingResult))
                {
                    CommandBuildStep stepInProgress = executeContext.IsCommandCurrentlyRunning(commandHash);
                    if (stepInProgress != null)
                    {
                        Monitor.Exit(executeContext);
                        monitorExited = true;
                        executeContext.Logger.Debug("Command {0} delayed because it is currently running...", Command.ToString());
                        status = (await stepInProgress.ExecutedAsync()).Status;
                        matchingResult = stepInProgress.Result;
                    }
                    else
                    {
                        executeContext.NotifyCommandBuildStepStarted(this, commandHash);
                        Monitor.Exit(executeContext);
                        monitorExited = true;

                        executeContext.Logger.Debug("Command {0} scheduled...", Command.ToString());

                        // Register the cancel callback
                        var cancellationTokenSource = executeContext.CancellationTokenSource;
                        cancellationTokenSource.Token.Register(x => ((Command)x).Cancel(), Command);

                        Command.CancellationToken = cancellationTokenSource.Token;

                        try
                        {
                            status = await StartCommand(executeContext, commandResultEntries, builderContext);
                        }
                        finally
                        {
                            // Restore cancellation token (to avoid memory leak due to previous CancellationToken.Register
                            Command.CancellationToken = CancellationToken.None;
                        }

                        executeContext.NotifyCommandBuildStepFinished(this, commandHash);
                    }
                }
            }
            finally
            {
                if (!monitorExited)
                {
                    Monitor.Exit(executeContext);
                }
            }

            // The command has not been executed
            if (matchingResult != null)
            {
                using (commandResultEntries)
                {
                    // Replicate triggered builds
                    Debug.Assert(SpawnedStepsList.Count == 0);

                    foreach (Command spawnedCommand in matchingResult.SpawnedCommands)
                    {
                        var spawnedStep = new CommandBuildStep(spawnedCommand);
                        SpawnedStepsList.Add(spawnedStep);
                        executeContext.ScheduleBuildStep(spawnedStep);
                    }

                    // Re-output command log messages
                    foreach (var message in matchingResult.LogMessages)
                    {
                        executeContext.Logger.Log(message);
                    }

                    // Wait for all build steps to complete.
                    // TODO: Ideally, we should store and replicate the behavior of the command that spawned it
                    // (wait if it used ScheduleAndExecute, don't wait if it used RegisterSpawnedCommandWithoutScheduling)
                    await Task.WhenAll(SpawnedSteps.Select(x => x.ExecutedAsync()));

                    status = ResultStatus.NotTriggeredWasSuccessful;
                    RegisterCommandResult(commandResultEntries, matchingResult, status);
                }
            }


            return status;
        }

        internal async Task<ResultStatus> SpawnCommand(Command command, IExecuteContext executeContext)
        {
            var spawnedStep = new CommandBuildStep(command);
            SpawnedStepsList.Add(spawnedStep);

            executeContext.ScheduleBuildStep(spawnedStep);
            var resultStatus = (await spawnedStep.ExecutedAsync()).Status;

            return resultStatus;
        }

        private void RegisterCommandResult(ListStore<CommandResultEntry> commandResultEntries, CommandResultEntry result, ResultStatus status)
        {
            //foreach (var outputObject in result.OutputObjects.Where(outputObject => outputObject.Key.Type == UrlType.Internal))
            //{
            //    builderContext.AssetIndexMap[outputObject.Key.Path] = outputObject.Value;
            //}

            Result = result;

            // Only save to build cache if compilation was done and successful
            if (status == ResultStatus.Successful)
            {
                commandResultEntries.AddValue(result);
            }
        }

        internal bool ShouldExecute(IExecuteContext executeContext, CommandResultEntry[] previousResultCollection, ObjectId commandHash, out CommandResultEntry matchingResult)
        {
            IndexFileCommand.MountDatabase(executeContext.GetOutputObjectsGroups());
            try
            {
                matchingResult = FindMatchingResult(executeContext, previousResultCollection);
            }
            finally
            {
                IndexFileCommand.UnmountDatabase();
            }

            if (matchingResult == null || Command.ShouldForceExecution())
            {
                // Ensure we ignore existing results if the execution is forced
                matchingResult = null;
                return true;
            }

            return false;
        }

        internal CommandResultEntry FindMatchingResult(IPrepareContext prepareContext, CommandResultEntry[] commandResultCollection)
        {
            if (commandResultCollection == null)
                return null;

            // Then check input dependencies and output versions
            //builderContext.AssetIndexMap.LoadNewValues();

            foreach (CommandResultEntry entry in commandResultCollection)
            {
                bool entryMatch = true;

                foreach (var inputDepVersion in entry.InputDependencyVersions)
                {
                    var hash = prepareContext.ComputeInputHash(inputDepVersion.Key.Type, inputDepVersion.Key.Path);
                    if (hash != inputDepVersion.Value)
                    {
                        entryMatch = false;
                        break;
                    }
                }

                if (!entryMatch)
                    continue;

                if (entry.OutputObjects.Any(outputObject => !VirtualFileSystem.FileExists(FileOdbBackend.BuildUrl(VirtualFileSystem.ApplicationDatabasePath, outputObject.Value))))
                {
                    entryMatch = false;
                }

                if (!entryMatch)
                    continue;

                // TODO/Benlitz: check matching spawned commands if needed

                return entry;
            }

            return null;
        }

        private async Task<ResultStatus> StartCommand(IExecuteContext executeContext, ListStore<CommandResultEntry> commandResultEntries, BuilderContext builderContext)
        {
            var logger = executeContext.Logger;

            //await Scheduler.Yield();

            ResultStatus status;

            using (commandResultEntries)
            {
                logger.Debug("Starting command {0}...", Command.ToString());

                // Creating the CommandResult object
                var commandContext = new LocalCommandContext(executeContext, this, builderContext);

                // Actually processing the command
                if (Command.ShouldSpawnNewProcess() && builderContext.MaxParallelProcesses > 0)
                {
                    while (!builderContext.CanSpawnParallelProcess())
                    {
                        await Task.Delay(1, Command.CancellationToken);
                    }

                    var address = "net.pipe://localhost/" + Guid.NewGuid();
                    var arguments = string.Format("--slave=\"{0}\" --build-path=\"{1}\" --profile=\"{2}\"", address, builderContext.BuildPath, builderContext.BuildProfile);

                    var startInfo = new ProcessStartInfo
                        {
                            FileName = builderContext.SlaveBuilderPath,
                            Arguments = arguments,
                            WorkingDirectory = Environment.CurrentDirectory,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        };

                    // Start WCF pipe for communication with process
                    var processBuilderRemote = new ProcessBuilderRemote(commandContext, Command, builderContext.Parameters);
                    var host = new ServiceHost(processBuilderRemote);
                    host.AddServiceEndpoint(typeof(IProcessBuilderRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { MaxReceivedMessageSize = int.MaxValue }, address);
                    host.Open();

                    var output = new List<string>();

                    var process = new Process { StartInfo = startInfo };
                    process.Start();
                    process.OutputDataReceived += (_, args) => LockProcessAndAddDataToList(process, output, args);
                    process.ErrorDataReceived += (_, args) => LockProcessAndAddDataToList(process, output, args);
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Attach debugger to newly created process
                    // Add a reference to EnvDTE80 in the csproj and uncomment this (and also the Thread.Sleep in BuildEngineCmmands), then start the master process without debugger to attach to a slave.
                    //var dte = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.11.0");
                    //foreach (EnvDTE.Process dteProcess in dte.Debugger.LocalProcesses)
                    //{
                    //    if (dteProcess.ProcessID == process.Id)
                    //    {
                    //        dteProcess.Attach();
                    //        dte.Debugger.CurrentProcess = dteProcess;
                    //    }
                    //}

                    Task[] tasksToWait = null;

                    while (!process.HasExited)
                    {
                        Thread.Sleep(1);
                        lock (spawnedCommandsToWait)
                        {
                            if (spawnedCommandsToWait.Count > 0)
                            {
                                tasksToWait = spawnedCommandsToWait.ToArray();
                                spawnedCommandsToWait.Clear();
                            }
                        }

                        if (tasksToWait != null)
                        {
                            await Task.WhenAll(tasksToWait);
                            tasksToWait = null;
                        }
                    }
                    host.Close();

                    builderContext.NotifyParallelProcessEnded();

                    if (process.ExitCode != 0)
                    {
                        logger.Debug("Remote command crashed with output:\n{0}", string.Join(Environment.NewLine, output));
                    }

                    if (processBuilderRemote.Result != null)
                    {
                        // Register results back locally
                        foreach (var outputObject in processBuilderRemote.Result.OutputObjects)
                        {
                            commandContext.RegisterOutput(outputObject.Key, outputObject.Value);
                        }

                        // Register log messages
                        foreach (var logMessage in processBuilderRemote.Result.LogMessages)
                        {
                            commandContext.Logger.Log(logMessage);
                        }

                        // Register tags
                        foreach (var tag in processBuilderRemote.Result.TagSymbols)
                        {
                            TagSymbol tagSymbol;

                            // Resolve tag locally
                            if (!Command.TagSymbols.TryGetValue(tag.Value, out tagSymbol))
                            {
                                // Should we ignore silently? (with warning)
                                throw new InvalidOperationException("Could not find tag symbol.");
                            }

                            commandContext.AddTag(tag.Key, tagSymbol);
                        }
                    }

                    status = Command.CancellationToken.IsCancellationRequested ? ResultStatus.Cancelled : (process.ExitCode == 0 ? ResultStatus.Successful : ResultStatus.Failed);
                }
                else
                {
                    Command.PreCommand(commandContext);
                    if (!Command.BasePreCommandCalled)
                        throw new InvalidOperationException("base.PreCommand not called in command " + Command);

                    try
                    {
                        // Merge results from prerequisites
                        // TODO: This will prevent us from overwriting this asset with different content as it will result in a write conflict
                        // At some point we _might_ want to get rid of WaitBuildStep/ListBuildStep system and write a fully stateless input/output-based system; probably need further discussions
                        var fileProvider = ContentManager.FileProvider;
                        if (fileProvider != null)
                        {
                            var assetIndexMap = fileProvider.AssetIndexMap;
                            foreach (var prerequisiteStep in PrerequisiteSteps)
                            {
                                foreach (var output in prerequisiteStep.OutputObjectIds)
                                {
                                    assetIndexMap[output.Key.Path] = output.Value;
                                }
                            }
                        }

                        status = await Command.DoCommand(commandContext);
                    }
                    catch (Exception ex)
                    {
                        executeContext.Logger.Error("Exception in command " + this + ": " + ex);
                        status = ResultStatus.Failed;
                    }

                    Command.PostCommand(commandContext, status);
                    if (!Command.BasePostCommandCalled)
                        throw new InvalidOperationException("base.PostCommand not called in command " + Command);
                }

                // Ensure the command set at least the result status
                if (status == ResultStatus.NotProcessed)
                    throw new InvalidDataException("The command " + Command + " returned ResultStatus.NotProcessed after completion.");

                // Registering the result to the build cache
                RegisterCommandResult(commandResultEntries, commandContext.ResultEntry, status);
            }

            return status;
        }

        private static void LockProcessAndAddDataToList(Process process, List<string> output, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lock (process)
                {
                    output.Add(args.Data);
                }
            }
        }

        public void AwaitSpawnedCommand(Task<ResultStatus> task)
        {
            lock (spawnedCommandsToWait)
            {
                spawnedCommandsToWait.Add(task);
            }
        }
    }
}