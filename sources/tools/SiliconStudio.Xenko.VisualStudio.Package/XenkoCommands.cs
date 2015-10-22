// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SiliconStudio.Xenko.VisualStudio.BuildEngine;
using Project = EnvDTE.Project;
using ProjectItem = EnvDTE.ProjectItem;
using Task = System.Threading.Tasks.Task;

namespace SiliconStudio.Xenko.VisualStudio
{
    public static class XenkoCommands
    {
        static class ProjectItemKind
        {
            public static string PhysicalFile = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
            public static string PhysicalFolder = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";
            public static string VirtualFolder = "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}";
            public static string Subproject = "{EA6618E8-6E24-4528-94BE-6889FE16485C}";
        }

        public static IServiceProvider ServiceProvider { get; set; }

        public static void CleanIntermediateAssetsProjectMenuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;

            // Default: Disabled
            menuCommand.Enabled = false;

            // Find selected project
            var project = GetSelectedProject();
            if (project == null)
            {
                menuCommand.Text = "Clean intermediate assets for ...";
                return;
            }

            // Update menu text to contains selected project name
            menuCommand.Text = string.Format("Clean intermediate assets for {0}", project.Name);
            menuCommand.Enabled = true;
        }


        public static async void CleanIntermediateAssetsSolutionMenuCommand_Callback(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(SDTE));

            // Is there any active solution?
            if (dte.Solution == null)
                return;

            foreach (var project in Projects())
            {
                await CleanIntermediateAsset(dte, project);
            }
        }

        /// <summary>
        /// Enumerates all projects in current solution.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Project> Projects()
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(SDTE));

            var projects = new List<Project>();

            var item = dte.Solution.Projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                    continue;

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    // Solution folder: recursive call
                    projects.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    // Project: add it
                    projects.Add(project);
                }
            }

            return projects;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            var projects = new List<Project>();

            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                    continue;

                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    // Solution folder: recursive call
                    projects.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    // Project: add it
                    projects.Add(subProject);
                }
            }

            return projects;
        }

        public static async void CleanIntermediateAssetsProjectMenuCommand_Callback(object sender, EventArgs e)
        {
            // Find selected project
            var project = GetSelectedProject();
            if (project == null)
                return;

            var dte = (DTE2)ServiceProvider.GetService(typeof(SDTE));

            // Is there any active solution?
            if (dte.Solution == null)
                return;

            await CleanIntermediateAsset(dte, project);
        }

        private static async Task CleanIntermediateAsset(DTE2 dte, Project project)
        {
            if (project.FileName == null || Path.GetExtension(project.FileName) != ".csproj")
                return;

            // Find current project active configuration
            var configManager = project.ConfigurationManager;
            var activeConfig = configManager.ActiveConfiguration;

            // Get global parameters for Configuration and Platform
            var globalProperties = new Dictionary<string, string>();
            globalProperties["Configuration"] = activeConfig.ConfigurationName;
            globalProperties["Platform"] = activeConfig.PlatformName == "Any CPU" ? "AnyCPU" : activeConfig.PlatformName;

            // Check if project has a SiliconStudioCurrentPackagePath
            var projectInstance = new ProjectInstance(project.FileName, globalProperties, null);
            var packagePathProperty = projectInstance.Properties.FirstOrDefault(x => x.Name == "SiliconStudioCurrentPackagePath");
            if (packagePathProperty == null)
                return;

            // Prepare build request
            var request = new BuildRequestData(project.FileName, globalProperties, null, new[] { "SiliconStudioCleanAsset" }, null);
            var pc = new Microsoft.Build.Evaluation.ProjectCollection();
            var buildParameters = new BuildParameters(pc);
            var buildLogger = new IDEBuildLogger(GetOutputPane(), new TaskProvider(ServiceProvider), VsHelper.ToHierarchy(project));
            buildParameters.Loggers = new[] { buildLogger };

            // Trigger async build
            buildLogger.OutputWindowPane.OutputStringThreadSafe(string.Format("Cleaning assets for project {0}...\r\n", project.Name));
            BuildManager.DefaultBuildManager.BeginBuild(buildParameters);
            var submission = BuildManager.DefaultBuildManager.PendBuildRequest(request);
            BuildResult buildResult = await submission.ExecuteAsync();
            BuildManager.DefaultBuildManager.EndBuild();
            buildLogger.OutputWindowPane.OutputStringThreadSafe("Done\r\n");
        }

        private static IVsOutputWindowPane GetOutputPane()
        {
            var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));

            // Get Output pane
            IVsOutputWindowPane pane;
            Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;
            outputWindow.CreatePane(ref generalPaneGuid, "General", 1, 0);
            outputWindow.GetPane(ref generalPaneGuid, out pane);
            return pane;
        }
        
        public static string GetProjectItemPath(ProjectItem projectItem)
        {
            // Get path (1 expected)
            if (projectItem.FileCount != 1)
                return null;

            return projectItem.FileNames[0];
        }

        public static Project GetSelectedProject()
        {
            var item = GetSelectedItem();

            // Project, return as is
            if (item is Project)
                return (Project)item;

            // ProjectItem, return containing project
            if (item is ProjectItem)
                return ((ProjectItem)item).ContainingProject;

            return null;
        }

        public static object GetSelectedItem()
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(SDTE));
            var selectedItems = (UIHierarchyItem[])dte.ToolWindows.SolutionExplorer.SelectedItems;

            // Expect a single result (no multi selection)
            if (selectedItems.Length != 1)
                return null;

            return selectedItems[0].Object;
        }
        
        internal static void RegisterCommands(OleMenuCommandService mcs)
        {
            // Create command for Xenko -> Clean intermediate assets for Solution
            var cleanIntermediateAssetsSolutionCommandID = new CommandID(GuidList.guidXenko_VisualStudio_PackageCmdSet, (int)XenkoPackageCmdIdList.cmdXenkoCleanIntermediateAssetsSolutionCommand);
            var cleanIntermediateAssetsSolutionMenuCommand = new OleMenuCommand(CleanIntermediateAssetsSolutionMenuCommand_Callback, cleanIntermediateAssetsSolutionCommandID);
            mcs.AddCommand(cleanIntermediateAssetsSolutionMenuCommand);

            // Create command for Xenko -> Clean intermediate assets for {selected project}
            var cleanIntermediateAssetsProjectCommandID = new CommandID(GuidList.guidXenko_VisualStudio_PackageCmdSet, (int)XenkoPackageCmdIdList.cmdXenkoCleanIntermediateAssetsProjectCommand);
            var cleanIntermediateAssetsProjectMenuCommand = new OleMenuCommand(CleanIntermediateAssetsProjectMenuCommand_Callback, cleanIntermediateAssetsProjectCommandID);
            cleanIntermediateAssetsProjectMenuCommand.BeforeQueryStatus += CleanIntermediateAssetsProjectMenuCommand_BeforeQueryStatus;
            cleanIntermediateAssetsProjectMenuCommand.Enabled = false;
            mcs.AddCommand(cleanIntermediateAssetsProjectMenuCommand);
        }
    }
}