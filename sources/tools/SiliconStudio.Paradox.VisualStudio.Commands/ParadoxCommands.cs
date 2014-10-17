// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SiliconStudio.Paradox.VisualStudio.BuildEngine;
using SiliconStudio.Paradox.VisualStudio.Commands.Shaders;
using SiliconStudio.Paradox.VisualStudio.DataGenerator;
using SiliconStudio.Paradox.VisualStudio.Shaders;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    public class ParadoxCommands : IParadoxCommands
    {
        public void StartRemoteBuildLogServer(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl)
        {
            new PackageBuildMonitorRemote(buildMonitorCallback, logPipeUrl);
        }

        public byte[] GenerateShaderKeys(string inputFileName, string inputFileContent)
        {
            return ShaderKeyFileHelper.GenerateCode(inputFileName, inputFileContent);
        }

        public byte[] GenerateDataClasses(string assemblyOutput, string projectFullName, string intermediateAssembly)
        {
            return DataCodeGeneratorHelper.GenerateSource(assemblyOutput, projectFullName, intermediateAssembly);
        }
    }
}