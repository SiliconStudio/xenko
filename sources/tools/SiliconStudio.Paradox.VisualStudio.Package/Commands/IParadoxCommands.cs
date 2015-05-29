// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using EnvDTE80;

using NShader;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    /// <summary>
    /// Describes paradox commands accessed by VS Package to current paradox package (so that VSPackage doesn't depend on Paradox assemblies).
    /// </summary>
    public interface IParadoxCommands
    {
        /// <summary>
        /// Initialize parsing (this method can be called from a separate thread).
        /// </summary>
        void Initialize(string paradoxSdkDir);

        /// <summary>
        /// Test whether we should reload these commands (assemblies changed)
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShouldReload();

        void StartRemoteBuildLogServer(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl);

        byte[] GenerateShaderKeys(string inputFileName, string inputFileContent);

        RawShaderNavigationResult AnalyzeAndGoToDefinition(string sourceCode, RawSourceSpan span);
    }

    public interface IBuildMonitorCallback
    {
        void Message(string type, string module, string text);
    }
}