// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using ILogger = SiliconStudio.Core.Diagnostics.ILogger;

namespace SiliconStudio.Assets
{
    public interface ICancellableAsyncBuild
    {
        string AssemblyPath { get; }

        Task<BuildResult> BuildTask { get; }

        bool IsCanceled { get; }

        void Cancel();
    }

    public static class VSProjectHelper
    {
        private const string SiliconStudioProjectType = "SiliconStudioProjectType";
        private const string SiliconStudioPlatform = "SiliconStudioPlatform";

        private static BuildManager mainBuildManager = new BuildManager();
        private static readonly string NugetPath;

        static VSProjectHelper()
        {
            var currentAssemblyLocation = typeof(VSProjectHelper).Assembly.Location;
            NugetPath = Path.Combine(Path.GetDirectoryName(currentAssemblyLocation), "NuGet.exe");
        }

        public static Guid GetProjectGuid(Microsoft.Build.Evaluation.Project project)
        {
            if (project == null) throw new ArgumentNullException("project");
            return Guid.Parse(project.GetPropertyValue("ProjectGuid"));
        }

        public static PlatformType? GetPlatformTypeFromProject(Microsoft.Build.Evaluation.Project project)
        {
            return GetEnumFromProperty<PlatformType>(project, SiliconStudioPlatform);
        }

        public static ProjectType? GetProjectTypeFromProject(Microsoft.Build.Evaluation.Project project)
        {
            return GetEnumFromProperty<ProjectType>(project, SiliconStudioProjectType);
        }

        private static T? GetEnumFromProperty<T>(Microsoft.Build.Evaluation.Project project, string propertyName) where T : struct
        {
            if (project == null) throw new ArgumentNullException("project");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            var value = project.GetPropertyValue(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return (T)Enum.Parse(typeof(T), value);
        }

        public static string GetOrCompileProjectAssembly(string solutionFullPath, string fullProjectLocation, ILogger logger, string targets, bool autoCompileProject, string configuration, string platform = "AnyCPU", Dictionary<string, string> extraProperties = null, bool onlyErrors = false, BuildRequestDataFlags flags = BuildRequestDataFlags.None)
        {
            if (fullProjectLocation == null) throw new ArgumentNullException("fullProjectLocation");
            if (logger == null) throw new ArgumentNullException("logger");

            var project = LoadProject(fullProjectLocation, configuration, platform, extraProperties);
            var assemblyPath = project.GetPropertyValue("TargetPath");
            try
            {
                if (!string.IsNullOrWhiteSpace(assemblyPath))
                {
                    if (autoCompileProject)
                    {
                        // NuGet restore
                        // TODO: We might want to call this less regularly than every build (i.e. project creation, and project.json update?)
                        // Probably not worth bothering since it might be part of MSBuild with VS15
                        var restoreNugetTask = RestoreNugetPackages(logger, solutionFullPath, project);

                        var asyncBuild = new CancellableAsyncBuild(project, assemblyPath);
                        asyncBuild.Build(restoreNugetTask, project, "Build", flags, new LoggerRedirect(logger, onlyErrors));
                        var buildResult = asyncBuild.BuildTask.Result;
                    }
                }
            }
            finally
            {
                project.ProjectCollection.UnloadAllProjects();
                project.ProjectCollection.Dispose();
            }

            return assemblyPath;
        }

        public static ICancellableAsyncBuild CompileProjectAssemblyAsync(string solutionFullPath, string fullProjectLocation, ILogger logger, string targets = "Build", string configuration = "Debug", string platform = "AnyCPU", Dictionary<string, string> extraProperties = null, BuildRequestDataFlags flags = BuildRequestDataFlags.None)
        {
            if (fullProjectLocation == null) throw new ArgumentNullException("fullProjectLocation");
            if (logger == null) throw new ArgumentNullException("logger");

            var project = LoadProject(fullProjectLocation, configuration, platform, extraProperties);
            var assemblyPath = project.GetPropertyValue("TargetPath");
            try
            {
                if (!string.IsNullOrWhiteSpace(assemblyPath))
                {
                    // NuGet restore
                    // TODO: We might want to call this less regularly than every build (i.e. project creation, and project.json update?)
                    // Probably not worth bothering since it might be part of MSBuild with VS15
                    var restoreNugetTask = RestoreNugetPackages(logger, solutionFullPath, project);

                    var asyncBuild = new CancellableAsyncBuild(project, assemblyPath);
                    asyncBuild.Build(restoreNugetTask, project, targets, flags, new LoggerRedirect(logger));
                    return asyncBuild;
                }
            }
            finally
            {
                
                project.ProjectCollection.UnloadAllProjects();
                project.ProjectCollection.Dispose();
            }

            return null;
        }

        public static async Task RestoreNugetPackages(ILogger logger, string solutionFullPath, Project project)
        {
            var addedProjs = new HashSet<string>(); //to avoid worst case circular dependencies.
            var allProjs = Utilities.IterateTree(project, project1 =>
            {
                var projs = new List<Project>();
                foreach (var item in project1.AllEvaluatedItems.Where(x => x.ItemType == "ProjectReference"))
                {
                    var path = Path.Combine(project.DirectoryPath, item.EvaluatedInclude);
                    if (!File.Exists(path)) continue;

                    if (addedProjs.Add(path))
                    {
                        projs.Add(project.ProjectCollection.LoadProject(path));
                    }
                }
                return projs;
            });

            foreach (var proj in allProjs)
            {
                // TODO: We directly find the project.json rather than the solution file (otherwise NuGet reports an error if the solution didn't contain a project.json or if solution is not saved yet)
                // However, the problem is that if Game was referencing another assembly with a project.json, it won't be updated
                // At some point we should find all project.json of the full solution, and keep regenerating them if any of them changed
                var projectJson = Path.Combine(proj.DirectoryPath, "project.json");

                // Nothing to do if there is no project.json
                if (!File.Exists(projectJson)) continue;

                // Check if project.json is newer than project.lock.json (GetLastWriteTimeUtc returns year 1601 if file doesn't exist so it will also generate it)
                var projectLockJson = Path.Combine(proj.DirectoryPath, "project.lock.json");
                if (File.GetLastWriteTimeUtc(projectJson) > File.GetLastWriteTimeUtc(projectLockJson))
                {
                    // Check if it needs to be regenerated
                    // Run NuGet.exe restore
                    var parameters = $"restore \"{projectJson}\"";
                    if (solutionFullPath != null)
                        parameters += $" -solutiondirectory \"{Path.GetDirectoryName(solutionFullPath)}\"";
                    await ShellHelper.RunProcessAndGetOutputAsync(NugetPath, parameters, logger);
                }
            }
        }

        public static Microsoft.Build.Evaluation.Project LoadProject(string fullProjectLocation, string configuration = "Debug", string platform = "AnyCPU", Dictionary<string, string> extraProperties = null)
        {
            configuration = configuration ?? "Debug";
            platform = platform ?? "AnyCPU";

            var globalProperties = new Dictionary<string, string>();
            globalProperties["Configuration"] = configuration;
            globalProperties["Platform"] = platform;

            if (extraProperties != null)
            {
                foreach (var extraProperty in extraProperties)
                {
                    globalProperties[extraProperty.Key] = extraProperty.Value;
                }
            }

            var projectCollection = new Microsoft.Build.Evaluation.ProjectCollection(globalProperties);
            projectCollection.LoadProject(fullProjectLocation);
            var project = projectCollection.LoadedProjects.First();
            return project;
        }

        private class LoggerRedirect : Microsoft.Build.Utilities.Logger
        {
            private readonly ILogger logger;
            private readonly bool onlyErrors;

            public LoggerRedirect(ILogger logger, bool onlyErrors = false)
            {
                if (logger == null) throw new ArgumentNullException("logger");
                this.logger = logger;
                this.onlyErrors = onlyErrors;
            }

            public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
            {
                if (eventSource == null) throw new ArgumentNullException("eventSource");
                if (!onlyErrors)
                {
                    eventSource.MessageRaised += MessageRaised;
                    eventSource.WarningRaised += WarningRaised;
                }
                eventSource.ErrorRaised += ErrorRaised;
            }

            void MessageRaised(object sender, BuildMessageEventArgs e)
            {
                var loggerResult = logger as LoggerResult;
                if (loggerResult != null)
                {
                    loggerResult.Module = $"{e.File}({e.LineNumber},{e.ColumnNumber})";
                }

                // Redirect task execution messages to verbose output
                var importance = e is TaskCommandLineEventArgs ? MessageImportance.Normal : e.Importance;

                switch (importance)
                {
                    case MessageImportance.High:
                        logger.Info(e.Message);
                        break;
                    case MessageImportance.Normal:
                        logger.Verbose(e.Message);
                        break;
                    case MessageImportance.Low:
                        logger.Debug(e.Message);
                        break;
                }
            }

            void WarningRaised(object sender, BuildWarningEventArgs e)
            {
                var loggerResult = logger as LoggerResult;
                if (loggerResult != null)
                {
                    loggerResult.Module = string.Format("{0}({1},{2})", e.File, e.LineNumber, e.ColumnNumber);
                }
                logger.Warning(e.Message);
            }

            void ErrorRaised(object sender, Microsoft.Build.Framework.BuildErrorEventArgs e)
            {
                var loggerResult = logger as LoggerResult;
                if (loggerResult != null)
                {
                    loggerResult.Module = string.Format("{0}({1},{2})", e.File, e.LineNumber, e.ColumnNumber);
                }
                logger.Error(e.Message);
            }
        }

        public static void Reset()
        {
            mainBuildManager.ResetCaches();
        }

        private class CancellableAsyncBuild : ICancellableAsyncBuild
        {
            public CancellableAsyncBuild(Project project, string assemblyPath)
            {
                Project = project;
                AssemblyPath = assemblyPath;
            }

            public string AssemblyPath { get; private set; }

            public Project Project { get; private set; }

            public Task<BuildResult> BuildTask { get; private set; }

            public bool IsCanceled { get; private set; }

            internal void Build(Task previousTask, Microsoft.Build.Evaluation.Project project, string targets, BuildRequestDataFlags flags, Microsoft.Build.Utilities.Logger logger)
            {
                if (project == null) throw new ArgumentNullException("project");
                if (logger == null) throw new ArgumentNullException("logger");

                // Make sure that we are using the project collection from the loaded project, otherwise we are getting
                // weird cache behavior with the msbuild system
                var projectInstance = new ProjectInstance(project.Xml, project.ProjectCollection.GlobalProperties, null, project.ProjectCollection);

                BuildTask = previousTask.ContinueWith(completedPreviousTask =>
                {
                    var buildResult = mainBuildManager.Build(
                        new BuildParameters(project.ProjectCollection)
                        {
                            Loggers = new[] { logger }
                        },
                        new BuildRequestData(projectInstance, targets.Split(';'), null, flags));

                    return buildResult;
                });
            }

            public void Cancel()
            {
                var localManager = mainBuildManager;
                if (localManager != null)
                {
                    localManager.CancelAllSubmissions();
                    IsCanceled = true;
                }
            }
        }
    }
}