// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SiliconStudio.Paradox.VisualStudio.CodeGenerator;
using SiliconStudio.Paradox.VisualStudio.Commands;

namespace SiliconStudio.Paradox.VisualStudio.BuildEngine
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid(GuidList.guidParadox_VisualStudio_DataCodeGenerator)]
    [ProvideObject(typeof(DataCodeGenerator), RegisterUsing = RegistrationMethod.CodeBase)]
    public class DataCodeGenerator : BaseCodeGeneratorWithSite
    {
        public const string DisplayName = "Paradox Data Code Generator";
        public const string InternalName = "ParadoxDataCodeGenerator";

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            // Get active project
            // TODO: Instead of a custom code generator, we should have a context command or something like that.
            // This should also allow generation of multiple files

            var lines = Regex.Split(inputFileContent, "\r\n|\r|\n");
            if (lines.Length == 0 || lines[0].Length == 0)
            {
                throw new InvalidOperationException("Source should contain project filename.");
            }

            var projectFullName = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(inputFileName), lines[0]));
            if (!File.Exists(projectFullName))
            {
                throw new InvalidOperationException("Project file doesn't exist.");
            }

            string assemblyOutput, intermediateAssembly;

            // Get Evaluation Project
            Microsoft.Build.Evaluation.Project msbuildProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectFullName).First();

            // Set ParadoxBuildStep variable and change IntermediateOutputPath
            var property1 = msbuildProject.SetProperty("ParadoxBuildStep", "StepData");
            var property2 = msbuildProject.SetProperty("IntermediateOutputPath", @"obj\StepData\");

            // Reevaluate dependent properties
            msbuildProject.ReevaluateIfNecessary();

            try
            {
                var outputPane = GetOutputPane();

                // Create logger
                var buildLogger = new IDEBuildLogger(outputPane, new TaskProvider(GlobalServiceProvider), VsHelper.GetCurrentHierarchy(GlobalServiceProvider));
                buildLogger.Verbosity = Microsoft.Build.Framework.LoggerVerbosity.Diagnostic;

                var evaluatedProperties = new Dictionary<string, Microsoft.Build.Evaluation.ProjectProperty>();
                foreach (var evaluatedProperty in msbuildProject.AllEvaluatedProperties)
                {
                    evaluatedProperties[evaluatedProperty.Name] = evaluatedProperty;
                }

                // Output properties
                foreach (var evaluatedProperty in evaluatedProperties)
                {
                    outputPane.OutputStringThreadSafe(string.Format(
                            "$({0}) = {1} was evaluated as {2}\n",
                            evaluatedProperty.Key,
                            evaluatedProperty.Value.UnevaluatedValue,
                            evaluatedProperty.Value.EvaluatedValue));
                }

                // Compile project (only intermediate assembly)
                // Dependencies will be built as well
                //var manager = BuildManager.DefaultBuildManager;
                using (var manager = new BuildManager())
                {
                    var pc = new Microsoft.Build.Evaluation.ProjectCollection();
                    var globalProperties = new Dictionary<string, string>();
                    globalProperties["SolutionName"] = evaluatedProperties["SolutionName"].EvaluatedValue;
                    globalProperties["SolutionDir"] = evaluatedProperties["SolutionDir"].EvaluatedValue;
                    var projectInstance = new ProjectInstance(projectFullName, globalProperties, null);
                    var buildResult = manager.Build(
                        new BuildParameters(pc)
                        {
                            Loggers = new[] {buildLogger},
                            DetailedSummary = true,
                        },
                        new BuildRequestData(projectInstance, new[] { "Compile" }, null));

                    if (buildResult.OverallResult == BuildResultCode.Failure)
                        throw new InvalidOperationException(string.Format("Build of {0} failed.", projectFullName));
                }

                // Get TargetPath and IntermediateAssembly
                assemblyOutput = msbuildProject.AllEvaluatedProperties.Last(x => x.Name == "TargetPath").EvaluatedValue;
                intermediateAssembly = msbuildProject.AllEvaluatedItems.First(x => x.ItemType == "IntermediateAssembly").EvaluatedInclude;
            }
            finally
            {
                msbuildProject.RemoveProperty(property1);
                msbuildProject.RemoveProperty(property2);
            }

            // Defer execution to current Paradox VS package plugin
            try
            {
                var remoteCommands = ParadoxCommandsProxy.GetProxy();
                return remoteCommands.GenerateDataClasses(assemblyOutput, projectFullName, intermediateAssembly);
            }
            catch (Exception ex)
            {
                GeneratorError(4, ex.ToString(), 0, 0);

                return new byte[0];
            }
        }

        protected override string GetDefaultExtension()
        {
            return ".cs";
        }

        protected IVsOutputWindowPane GetOutputPane()
        {
            var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));

            // Get Output pane
            IVsOutputWindowPane pane;
            Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;
            outputWindow.CreatePane(ref generalPaneGuid, "General", 1, 0);
            outputWindow.GetPane(ref generalPaneGuid, out pane);
            return pane;
        }
    }
}