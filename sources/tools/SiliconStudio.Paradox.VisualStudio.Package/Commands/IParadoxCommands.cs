// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using EnvDTE80;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    /// <summary>
    /// Describes paradox commands accessed by VS Package to current paradox package (so that VSPackage doesn't depend on Paradox assemblies).
    /// </summary>
    public interface IParadoxCommands
    {
        void StartRemoteBuildLogServer(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl);

        byte[] GenerateShaderKeys(string inputFileName, string inputFileContent);

        byte[] GenerateDataClasses(string assemblyOutput, string projectFullName, string intermediateAssembly);
    }

    public interface IBuildMonitorCallback
    {
        void Message(string type, string module, string text);
    }
}