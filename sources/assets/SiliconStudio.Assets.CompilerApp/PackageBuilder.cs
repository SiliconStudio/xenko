// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Core.Serialization.Assets;

using System.Threading;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Assets.Effect;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Assets.CompilerApp
{
    public class PackageBuilder
    {
        private readonly PackageBuilderOptions builderOptions;
        private Builder builder;

        public PackageBuilder(PackageBuilderOptions packageBuilderOptions)
        {
            if (packageBuilderOptions == null) throw new ArgumentNullException("packageBuilderOptions");

            builderOptions = packageBuilderOptions;
        }

        public BuildResultCode Build()
        {
            BuildResultCode result;

            if (builderOptions.IsValidForSlave())
            {
                // Sleeps one second so that debugger can attach
                //Thread.Sleep(1000);

                result = BuildSlave();
            }
            else
            {
                // build the project to the build path
                result = BuildMaster();
            }

            return result;
        }

        private static void PrepareDatabases()
        {
            ContentManager.GetFileProvider = () => IndexFileCommand.DatabaseFileProvider;
        }

        private BuildResultCode BuildMaster()
        {
            // Only querying graphics platform, let's load package, print it and exit
            if (builderOptions.GetGraphicsPlatform)
            {
                return BuildGetGraphicsPlatform();
            }

            AssetCompilerContext context = null;
            PackageSession projectSession = null;
            try
            {
                // TODO handle solution file + package-id ?

                // When the current platform is not on windows, we need to make sure that all plugins are build, so we
                // setup auto-compile when loading the session
                var sessionLoadParameters = new PackageLoadParameters
                {
                    AutoCompileProjects = builderOptions.Platform != PlatformType.Windows || !builderOptions.DisableAutoCompileProjects,
                    ExtraCompileProperties = builderOptions.ExtraCompileProperties,
                };

                // Loads the root Package
                var projectSessionResult = PackageSession.Load(builderOptions.PackageFile, sessionLoadParameters);
                if (projectSessionResult.HasErrors)
                {
                    projectSessionResult.CopyTo(builderOptions.Logger);
                    return BuildResultCode.BuildError;
                }

                projectSession = projectSessionResult.Session;

                // Check build configuration
                var package = projectSession.LocalPackages.Last();

                // Check build profile
                var sharedProfile = package.Profiles.FindSharedProfile();
                var buildProfile = package.Profiles.FirstOrDefault(pair => pair.Name == builderOptions.BuildProfile);
                if (buildProfile == null)
                {
                    builderOptions.Logger.Error("Unable to find profile [{0}] in package [{1}]", builderOptions.BuildProfile, package.FullPath);
                    return BuildResultCode.BuildError;
                }

                // Setup variables
                var buildDirectory = builderOptions.BuildDirectory;
                var outputDirectory = builderOptions.OutputDirectory;

                // Process game settings asset
                var gameSettingsAsset = package.GetGameSettingsAsset();
                if (gameSettingsAsset == null)
                {
                    builderOptions.Logger.Warning("Could not find game settings asset at location [{0}]. Use a Default One", GameSettingsAsset.GameSettingsLocation);
                    gameSettingsAsset = GameSettingsFactory.Create();
                }

                // Create context
                context = new AssetCompilerContext
                {
                    Profile = builderOptions.BuildProfile,
                    Platform = builderOptions.Platform,
                    BuildConfiguration = builderOptions.ProjectConfiguration
                };

                // Command line properties
                foreach (var property in builderOptions.Properties)
                    context.OptionProperties.Add(property.Key, property.Value);

                // Set current game settings
                context.SetGameSettingsAsset(gameSettingsAsset);

                // Copy properties from shared profiles to context properties
                if (sharedProfile != null)
                {
                    sharedProfile.Properties.CopyTo(context.PackageProperties, true);
                }

                // Copy properties from build profile
                buildProfile.Properties.CopyTo(context.PackageProperties, true);

                // Builds the project
                var assetBuilder = new PackageCompiler(new RootPackageAssetEnumerator(package));
                assetBuilder.AssetCompiled += RegisterBuildStepProcessedHandler;

                var assetBuildResult = assetBuilder.Compile(context);
                assetBuildResult.CopyTo(builderOptions.Logger);
                if (assetBuildResult.HasErrors)
                    return BuildResultCode.BuildError;

                // Add specific steps to generate shaders
                // TODO: This doesn't really belong here, where should we move it?
                assetBuildResult.BuildSteps.Add(new WaitBuildStep());
                //assetBuildResult.BuildSteps.Add(new CompileDefaultSceneEffectCommand(context, package, assetBuildResult));

                // Create the builder
                var indexName = "index." + builderOptions.BuildProfile;
                builder = new Builder(builderOptions.Logger, buildDirectory, builderOptions.BuildProfile, indexName) { ThreadCount = builderOptions.ThreadCount };
                builder.MonitorPipeNames.AddRange(builderOptions.MonitorPipeNames);

                // Add build steps generated by AssetBuilder
                builder.Root.Add(assetBuildResult.BuildSteps);

                // Run builder
                var result = builder.Run(Builder.Mode.Build);
                builder.WriteIndexFile(false);

                // Fill list of bundles
                var bundlePacker = new BundlePacker();
                bundlePacker.Build(builderOptions.Logger, projectSession, buildProfile, indexName, outputDirectory, builder.DisableCompressionIds);

                return result;
            }
            finally
            {
                builder?.Dispose();
                // Dispose the session (in order to unload assemblies)
                projectSession?.Dispose();
                context?.Dispose();
            }
        }

        private BuildResultCode BuildGetGraphicsPlatform()
        {
            var localLogger = new LoggerResult();
            var simplePackage = Package.Load(localLogger, builderOptions.PackageFile, new PackageLoadParameters
            {
                AutoLoadTemporaryAssets = true,
                LoadAssemblyReferences = false,
                AutoCompileProjects = false,
                TemporaryAssetFilter = (asset) => asset.AssetPath == GameSettingsAsset.GameSettingsLocation,
                TemporaryAssetsInMsbuild = false,
            });

            if (simplePackage == null
                || localLogger.HasErrors)
            {
                localLogger.CopyTo(builderOptions.Logger);
                return BuildResultCode.BuildError;
            }

            var settings = simplePackage.GetGameSettingsAsset();
            var renderingSettings = settings.Get<RenderingSettings>();

            var buildProfile = simplePackage.Profiles.FirstOrDefault(pair => pair.Name == builderOptions.BuildProfile);
            if (buildProfile == null)
            {
                builderOptions.Logger.Error("Package {0} did not contain platform {1}", builderOptions.PackageFile, builderOptions.BuildProfile);
                return BuildResultCode.BuildError;
            }

            Console.WriteLine(RenderingSettings.GetGraphicsPlatform(builderOptions.Platform, renderingSettings.PreferredGraphicsPlatform));
            return BuildResultCode.Successful;
        }

        private void RegisterBuildStepProcessedHandler(object sender, AssetCompiledArgs e)
        {
            if (e.Result.BuildSteps == null)
                return;

            foreach (var buildStep in e.Result.BuildSteps.EnumerateRecursively())
            {
                buildStep.Tag = e.Asset;
                buildStep.StepProcessed += BuildStepProcessed;
            }
        }

        private void BuildStepProcessed(object sender, BuildStepEventArgs e)
        {
            var assetItem = (AssetItem)e.Step.Tag;
            var assetRef = assetItem.ToReference();
            var project = assetItem.Package;
            var stepLogger = e.Logger is BuildStepLogger ? ((BuildStepLogger)e.Logger).StepLogger : null;
            if (stepLogger != null)
            {
                foreach (var message in stepLogger.Messages.Where(x => x.LogMessage.IsAtLeast(LogMessageType.Warning)))
                {
                    var assetMessage = new AssetLogMessage(project, assetRef, message.LogMessage.Type, AssetMessageCode.CompilationMessage, assetRef.Location, message.LogMessage.Text)
                    {
                        Exception = message.LogMessage is LogMessage ? ((LogMessage)message.LogMessage).Exception : null
                    };
                    builderOptions.Logger.Log(assetMessage);
                }
            }
            switch (e.Step.Status)
            {
                // This case should never happen
                case ResultStatus.NotProcessed:
                    builderOptions.Logger.Log(new AssetLogMessage(project, assetRef, LogMessageType.Fatal, AssetMessageCode.InternalCompilerError, assetRef.Location));
                    break;
                case ResultStatus.Successful:
                    builderOptions.Logger.Log(new AssetLogMessage(project, assetRef, LogMessageType.Verbose, AssetMessageCode.CompilationSucceeded, assetRef.Location));
                    break;
                case ResultStatus.Failed:
                    builderOptions.Logger.Log(new AssetLogMessage(project, assetRef, LogMessageType.Error, AssetMessageCode.CompilationFailed, assetRef.Location));
                    break;
                case ResultStatus.Cancelled:
                    builderOptions.Logger.Log(new AssetLogMessage(project, assetRef, LogMessageType.Verbose, AssetMessageCode.CompilationCancelled, assetRef.Location));
                    break;
                case ResultStatus.NotTriggeredWasSuccessful:
                    builderOptions.Logger.Log(new AssetLogMessage(project, assetRef, LogMessageType.Verbose, AssetMessageCode.AssetUpToDate, assetRef.Location));
                    break;
                case ResultStatus.NotTriggeredPrerequisiteFailed:
                    builderOptions.Logger.Log(new AssetLogMessage(project, assetRef, LogMessageType.Error, AssetMessageCode.PrerequisiteFailed, assetRef.Location));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            e.Step.StepProcessed -= BuildStepProcessed;
        }

        private static void RegisterRemoteLogger(IProcessBuilderRemote processBuilderRemote)
        {
            // The pipe might be broken while we try to output log, so let's try/catch the call to prevent program for crashing here (it should crash at a proper location anyway if the pipe is broken/closed)
            // ReSharper disable EmptyGeneralCatchClause
            GlobalLogger.GlobalMessageLogged += logMessage =>
            {
                try
                {
                    var assetMessage = logMessage as AssetLogMessage;
                    var message = assetMessage != null ? new AssetSerializableLogMessage(assetMessage) : new SerializableLogMessage((LogMessage)logMessage);

                    processBuilderRemote.ForwardLog(message);
                }
                catch
                {
                }
            };
            // ReSharper restore EmptyGeneralCatchClause
        }

        private BuildResultCode BuildSlave()
        {
            // Mount build path
            ((FileSystemProvider)VirtualFileSystem.ApplicationData).ChangeBasePath(builderOptions.BuildDirectory);

            PrepareDatabases();

            VirtualFileSystem.CreateDirectory(VirtualFileSystem.ApplicationDatabasePath);

            // Open WCF channel with master builder
            var namedPipeBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { SendTimeout = TimeSpan.FromSeconds(300.0) };
            var processBuilderRemote = ChannelFactory<IProcessBuilderRemote>.CreateChannel(namedPipeBinding, new EndpointAddress(builderOptions.SlavePipe));

            try
            {
                RegisterRemoteLogger(processBuilderRemote);

                // Create scheduler
                var scheduler = new Scheduler();

                var status = ResultStatus.NotProcessed;

                // Schedule command
                string buildPath = builderOptions.BuildDirectory;
                string buildProfile = builderOptions.BuildProfile;

                Builder.SetupBuildPath(buildPath, VirtualFileSystem.ApplicationDatabaseIndexName);

                var logger = builderOptions.Logger;
                MicroThread microthread = scheduler.Add(async () =>
                {
                    // Deserialize command and parameters
                    Command command = processBuilderRemote.GetCommandToExecute();
                    BuildParameterCollection parameters = processBuilderRemote.GetBuildParameters();

                    // Run command
                    var inputHashes = FileVersionTracker.GetDefault();
                    var builderContext = new BuilderContext(buildPath, buildProfile, inputHashes, parameters, 0, null);

                    var commandContext = new RemoteCommandContext(processBuilderRemote, command, builderContext, logger);
                    IndexFileCommand.MountDatabase(commandContext.GetOutputObjectsGroups());
                    command.PreCommand(commandContext);
                    status = await command.DoCommand(commandContext);
                    command.PostCommand(commandContext, status);

                    // Returns result to master builder
                    processBuilderRemote.RegisterResult(commandContext.ResultEntry);
                });

                while (true)
                {
                    scheduler.Run();

                    // Exit loop if no more micro threads
                    lock (scheduler.MicroThreads)
                    {
                        if (!scheduler.MicroThreads.Any())
                            break;
                    }

                    Thread.Sleep(0);
                }

                // Rethrow any exception that happened in microthread
                if (microthread.Exception != null)
                {
                    builderOptions.Logger.Fatal(microthread.Exception.ToString());
                    return BuildResultCode.BuildError;
                }

                if (status == ResultStatus.Successful || status == ResultStatus.NotTriggeredWasSuccessful)
                    return BuildResultCode.Successful;

                return BuildResultCode.BuildError;
            }
            finally
            {
                // Close WCF channel
                // ReSharper disable SuspiciousTypeConversion.Global
                ((IClientChannel)processBuilderRemote).Close();
                // ReSharper restore SuspiciousTypeConversion.Global
            }
        }

        /// <summary>
        /// Cancels this build.
        /// </summary>
        /// <returns><c>true</c> if the build was cancelled, <c>false</c> otherwise.</returns>
        public bool Cancel()
        {
            if (builder != null && builder.IsRunning)
            {
                builder.CancelBuild();
                return true;
            }
            return false;
        }
    }
}
